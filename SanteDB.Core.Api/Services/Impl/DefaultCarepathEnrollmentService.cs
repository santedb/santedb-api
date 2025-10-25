/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2024-12-12
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services.Impl.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Default carepath enrolment service
    /// </summary>
    public class DefaultCarepathEnrollmentService : ICarePathwayEnrollmentService, IDisposable
    {

        /// <summary>
        /// Annotation for reconilation of actions in the care plan
        /// </summary>
        private struct ReconiliationAnnotation
        {
            public ReconiliationAnnotation(Guid reconciledTo)
            {
                this.To = reconciledTo;
            }

            /// <summary>
            /// The redirected UUID
            /// </summary>
            public Guid To { get; }
        }

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultCarepathEnrollmentService));
        private readonly CarePathwayConfigurationSection m_configuration;
        private readonly IDataCachingService m_dataCacheService;
        private readonly IPolicyEnforcementService m_pepService;
        private readonly INotifyRepositoryService<Patient> m_patientRepository;
        private readonly IRepositoryService<CarePlan> m_carePlanRepository;
        private readonly INotifyRepositoryService<CarePathwayDefinition> m_carePathwayRepository;
        private readonly IJobManagerService m_jobManager;
        private readonly IDecisionSupportService m_decisionSupportService;
        private readonly ConcurrentDictionary<Guid, Func<Patient, bool>> m_compiledExpressions = new ConcurrentDictionary<Guid, Func<Patient, bool>>();
        private readonly INotifyRepositoryService<Bundle> m_bundleRepository;

        /// <summary>DI constructor</summary>
        public DefaultCarepathEnrollmentService(
            IConfigurationManager configurationManager,
            IPolicyEnforcementService policyService,
            INotifyRepositoryService<CarePathwayDefinition> carePathwayRepository,
            IJobManagerService jobManager,
            IDecisionSupportService decisionSupportService,
            IPrivacyEnforcementService privacyService,
            INotifyRepositoryService<Patient> patientRepository,
            IDataCachingService dataCaching,
            INotifyRepositoryService<Bundle> bundleRepository,
            IRepositoryService<CarePlan> careplanRepository)
        {
            this.m_configuration = configurationManager.GetSection<CarePathwayConfigurationSection>() ?? new CarePathwayConfigurationSection()
            {
                EnableAutoEnrollment = true
            };
            this.m_dataCacheService = dataCaching;
            this.m_pepService = policyService;
            this.m_patientRepository = patientRepository;
            this.m_carePlanRepository = careplanRepository;
            this.m_carePathwayRepository = carePathwayRepository;
            this.m_jobManager = jobManager;
            this.m_decisionSupportService = decisionSupportService;
            // Monitor the patient registration subsystem
            this.m_patientRepository.Inserted += patientRepositoryChange;
            this.m_patientRepository.Saved += patientRepositoryChange;
            this.m_bundleRepository = bundleRepository;
            this.m_carePathwayRepository.Inserted += carePathwayRepositoryChange;
            this.m_carePathwayRepository.Saved += carePathwayRepositoryChange;
            bundleRepository.Inserting += bundleRepositoryChange;
            bundleRepository.Saving += bundleRepositoryChange;
            // Register the care planning job
            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                try
                {
                    if (!this.m_jobManager.IsJobRegistered(typeof(CareplanEnrollmentJob)))
                    {
                        this.m_jobManager.RegisterJob(typeof(CareplanEnrollmentJob));
                    }
                }
                catch (Exception ex)
                {
                    this.m_tracer.TraceWarning("Error registering CarePlan Enrolment Job");
                }
            };
        }

        private void bundleRepositoryChange(object sender, DataPersistingEventArgs<Bundle> e)
        {
            foreach (var p in e.Data.Item.OfType<Patient>().ToArray())
            {
                var eligibleCarePaths = this.GetEligibleCarePaths(p).ToList();
                // All enrolled carepaths where the person is no longer eligible
                foreach (var er in this.GetEnrolledCarePaths(p))
                {
                    if (!eligibleCarePaths.Any(cp => cp.Key == er.Key))
                    {
                        this.UnEnroll(p, er);
                    }
                }

                foreach (var cp in eligibleCarePaths)
                {
                    if (cp.EnrollmentMode == CarePathwayEnrollmentMode.Automatic)
                    {
                        this.m_tracer.TraceInfo("Patient {0} meets eligibility criteria for {1} - automatically enrolling", p, cp);
                        if (!e.Data.Item.OfType<CarePlan>().Any(c => c.CarePathwayKey == cp.Key))
                        {
                            // HACK: Bundles often contain historical data so we need to reconstitute the bundle 
                            p.Participations?.ForEach(part =>
                            {
                                if (part.Act == null && part.ActKey.HasValue)
                                {
                                    part.Act = e.Data.Item.Find(o => o.Key == part.ActKey) as Act;
                                }
                            });
                            var carePlan = this.CreateCarePlan(p, cp);
                            e.Data.Item.Add(carePlan);
                            e.Data.Item.AddRange(carePlan.Relationships.SelectMany(o => this.ExtractCarePlanObjects(o)));
                        }
                    }
                }
            }
        }

        private IEnumerable<Act> ExtractCarePlanObjects(ActRelationship actRelationship)
        {
            if (actRelationship.TargetAct == null)
            {
                yield break;
            }

            actRelationship.TargetActKey = actRelationship.TargetAct.Key = actRelationship.TargetAct?.Key ?? Guid.NewGuid();
            var ta = actRelationship.TargetAct;
            actRelationship.TargetAct = null;
            if (ta.Relationships?.Any() == true)
            {
                foreach (var itm in ta.Relationships.SelectMany(o => this.ExtractCarePlanObjects(o)))
                {
                    yield return itm;
                }
            }
            yield return ta;
        }


        /// <inheritdoc/>
        public string ServiceName => "Default Care Pathway Management Service";

        /// <summary>
        /// Refresh for automatic enrolment
        /// </summary>
        private void carePathwayRepositoryChange(object sender, Event.DataPersistedEventArgs<CarePathwayDefinition> e)
        {
            if (e.Data.EnrollmentMode == CarePathwayEnrollmentMode.Automatic && this.m_configuration.EnableAutoEnrollment)
            {
                this.m_jobManager.StartJob(typeof(CareplanEnrollmentJob), new object[] { e.Data.Key.Value });
            }
            this.m_compiledExpressions.TryRemove(e.Data.Key.Value, out _);
        }

        /// <summary>
        /// Monitor callback for registered patients - will apply automatic enrolment
        /// </summary>
        private void patientRepositoryChange(object sender, Event.DataPersistedEventArgs<Patient> e)
        {
            var eligibleCarePaths = this.GetEligibleCarePaths(e.Data).ToArray();
            // All enrolled carepaths where the person is no longer eligible
            foreach (var er in this.GetEnrolledCarePaths(e.Data))
            {
                if(!eligibleCarePaths.Any(cp=>cp.Key == er.Key))
                {
                    this.UnEnroll(e.Data, er);
                }
            }

            foreach (var cp in eligibleCarePaths)
            {
                if (cp.EnrollmentMode == CarePathwayEnrollmentMode.Automatic)
                {
                    this.m_tracer.TraceInfo("Patient {0} meets eligibility criteria for {1} - automatically enrolling", e.Data, cp);
                    this.Enroll(e.Data, cp);
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.m_patientRepository.Inserted -= this.patientRepositoryChange;
            this.m_patientRepository.Saved -= this.patientRepositoryChange;
        }


        /// <inheritdoc/>
        public CarePlan Enroll(Patient patient, CarePathwayDefinition carePathway)
        {
            var carePlan = this.CreateCarePlan(patient, carePathway);
            return this.m_carePlanRepository.Insert(carePlan);
        }

        /// <summary>
        /// Enroll internal
        /// </summary>
        private CarePlan CreateCarePlan(Patient patient, CarePathwayDefinition carePathway)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }
            else if (carePathway == null)
            {
                throw new ArgumentNullException(nameof(carePathway));
            }
            else if (!this.ValidateEligibilityInternal(patient, carePathway))
            {
                throw new DetectedIssueException(BusinessRules.DetectedIssuePriorityType.Error, "carepath.enroll.eligibility", String.Format("Patient {0} is ineligible to be enrolled in {1}", patient, carePathway), DetectedIssueKeys.SafetyConcernIssue, null);
            }

            this.m_pepService.Demand(PermissionPolicyIdentifiers.WriteClinicalData);
            var cp = this.m_decisionSupportService.CreateCarePlan(patient, true, new Dictionary<String, object>() { { CdssParameterNames.PATHWAY_SCOPE, carePathway.Mnemonic }, { CdssParameterNames.PERSISTENT_OUTPUT, true } });
            cp.StatusConceptKey = StatusKeys.Active;
            cp.BatchOperation = Model.DataTypes.BatchOperationType.InsertOrUpdate;

            if (this.TryGetEnrollment(patient, carePathway, out var existingCp))
            {
                cp.Key = existingCp.Key;
                cp.BatchOperation = Model.DataTypes.BatchOperationType.Update;
            }

            return cp;
        }

        /// <summary>
        /// Validate the patient <paramref name="patient"/>'s eligibility in <paramref name="carePathway"/>
        /// </summary>
        private bool ValidateEligibilityInternal(Patient patient, CarePathwayDefinition carePathway)
        {
            if (!this.m_compiledExpressions.TryGetValue(carePathway.Key.Value, out var fn))
            {
                var eligibilityLinq = QueryExpressionParser.BuildLinqExpression<Patient>(carePathway.EligibilityCriteria.ParseQueryString(), "o", null, safeNullable: true, forceLoad: true, lazyExpandVariables: true) as Expression<Func<Patient, bool>>;
                fn = eligibilityLinq.Compile();
                this.m_compiledExpressions.TryAdd(carePathway.Key.Value, fn);
            }
            return fn(patient);
        }

        /// <inheritdoc/>
        public IEnumerable<CarePathwayDefinition> GetEligibleCarePaths(Patient patient)
        {
            foreach (var cp in this.m_carePathwayRepository.Find(o => o.ObsoletionTime == null))
            {
                if (this.ValidateEligibilityInternal(patient, cp))
                {
                    yield return cp;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<CarePathwayDefinition> GetEnrolledCarePaths(Patient patient)
        {
            // Get the care pathways
            var cpIds = this.m_carePathwayRepository.Find(o => o.ObsoletionTime == null).Select(o => o.Key.Value).ToArray();
            var carePlans = this.m_carePlanRepository.Find(o => o.Participations.Where(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget).Any(p => p.PlayerEntityKey == patient.Key) && o.StatusConceptKey == StatusKeys.Active && cpIds.Contains(o.CarePathwayKey.Value)).ToList();
            return carePlans.Select(o => o.LoadProperty(c => c.CarePathway));
        }

        /// <inheritdoc/>
        public CarePlan UnEnroll(Patient patient, CarePathwayDefinition carePathway)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }
            else if (carePathway == null)
            {
                throw new ArgumentNullException(nameof(carePathway));
            }
            else if (this.TryGetEnrollment(patient, carePathway, out var carePlan))
            {
                this.m_pepService.Demand(PermissionPolicyIdentifiers.WriteClinicalData);
                carePlan.StatusConceptKey = StatusKeys.Cancelled;

                // Cancel all un-fulfilled parts of the care-plan
                var saveTransaction = new Bundle();
                saveTransaction.AddRange(this.CascadeCancellation(carePlan));

                saveTransaction = this.m_bundleRepository.Save(saveTransaction);
                return saveTransaction.Item.OfType<CarePlan>().FirstOrDefault();
            }
            else
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, String.Format("{0} already enrolled in {1}", patient, carePathway)));
            }
        }

        /// <summary>
        /// Cascade the cancellation
        /// </summary>
        private IEnumerable<IdentifiedData> CascadeCancellation(Act actToCancel)
        {
            actToCancel.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
            actToCancel.StatusConceptKey = StatusKeys.Cancelled;
            yield return actToCancel;
            foreach (var act in actToCancel.LoadProperty(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(r => r.LoadProperty(o => o.TargetAct)))
            {
                if (act.StatusConceptKey != StatusKeys.Completed && act.StatusConceptKey != StatusKeys.Active)
                {
                    foreach (var itm in this.CascadeCancellation(act))
                    {
                        yield return itm;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool TryGetEnrollment(Patient patient, CarePathwayDefinition carePathway, out CarePlan carePlan)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }
            else if (carePathway == null)
            {
                throw new ArgumentNullException(nameof(carePathway));
            }
            carePlan = this.m_carePlanRepository.Find(o => o.CarePathwayKey == carePathway.Key && o.Participations.Where(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget).Any(p => p.PlayerEntityKey == patient.Key) && o.StatusConceptKey == StatusKeys.Active).FirstOrDefault();
            return carePlan != null;
        }

        /// <inheritdoc/>
        public CarePlan Enroll(Patient patient, Guid carePathwayKey)
        {
            return this.Enroll(patient, this.m_carePathwayRepository.Get(carePathwayKey));
        }

        /// <inheritdoc/>
        public CarePlan UnEnroll(Patient patient, Guid carePathwayKey)
        {
            return this.UnEnroll(patient, this.m_carePathwayRepository.Get(carePathwayKey));
        }

        /// <inheritdoc/>
        public bool TryGetEnrollment(Patient patient, Guid carePathwayKey, out CarePlan carePlan)
        {
            return this.TryGetEnrollment(patient, this.m_carePathwayRepository.Get(carePathwayKey), out carePlan);
        }

        /// <inheritdoc/>
        public CarePlan RecomputeOrEnroll(Patient patient, Guid pathwayId)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            if (!this.TryGetEnrollment(patient, pathwayId, out var existingCarePlan))
            {
                return this.Enroll(patient, pathwayId);
            }
            else
            {
                var updatedCarePlan = this.CreateCarePlan(patient, this.m_carePathwayRepository.Get(pathwayId));

                var transaction = new Bundle();

                // Load any has components in the old careplan that are not fulfilled and cancel them
                transaction.AddRange(this.UpdateCarePlan(existingCarePlan, updatedCarePlan));
                return this.m_bundleRepository.Insert(transaction).Item.OfType<CarePlan>().First();
            }
        }

        /// <summary>
        /// Updates <paramref name="existingCarePlan"/> to reflect the dates and times, removed actions and added actions in <paramref name="updatedCarePlan"/>
        /// </summary>
        /// <param name="existingCarePlan">The existing care plan</param>
        /// <param name="updatedCarePlan">The updated care plan</param>
        /// <returns>Actions to update <paramref name="existingCarePlan"/> to <paramref name="updatedCarePlan"/></returns>
        private IEnumerable<IdentifiedData> UpdateCarePlan(CarePlan existingCarePlan, CarePlan updatedCarePlan)
        {
            this.m_dataCacheService.Remove(existingCarePlan);

            // Our care plans are generated with encounters so we want the contents of the encounters - rather than the encounters to reconcile
            var existingActions = existingCarePlan.LoadProperty(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).SelectMany(pe => pe.LoadProperty(o => o.TargetAct).LoadProperty(o => o.Relationships).Where(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent)).Select(r => r.LoadProperty(o => o.TargetAct));
            var updatedActions = updatedCarePlan.Relationships.Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).SelectMany(pe => pe.TargetAct.Relationships.Where(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent));

            // First we want to set the keys and remove any old data that does not appear in the new care plan 
            foreach (var itm in updatedActions)
            {
                var storedProtocols = itm.TargetAct.LoadProperty(o => o.Protocols);
                var candidate = existingActions.FirstOrDefault(p => p.ClassConceptKey == itm.TargetAct.ClassConceptKey && p.TypeConceptKey == itm.TargetAct.TypeConceptKey &&
                    p.LoadProperty(o => o.Protocols).Any(o => storedProtocols.All(q => q.ProtocolKey == o.ProtocolKey && q.Sequence == o.Sequence)));


                if (candidate == null) // No candidate - so just return
                {
                    yield return itm.TargetAct;
                    itm.TargetAct = null;
                }
                else
                {
                    this.m_dataCacheService.Remove(candidate);

                    // We want our new care plan encounter to point to the existing act object - update the stored data with new updated data
                    candidate.AddAnnotation(new ReconiliationAnnotation(itm.TargetAct.Key.Value));
                    itm.TargetActKey = itm.TargetAct.Key = candidate.Key;
                    itm.TargetAct.BatchOperation = Model.DataTypes.BatchOperationType.Update;
                    itm.TargetAct.StatusConceptKey = candidate.StatusConceptKey;
                    yield return itm.TargetAct;
                    itm.TargetAct = null;
                }
            }

            // We want to remove all actions from the existing care plan (remove them) - where there is no reconciliation
            foreach (var itm in existingActions.Where(a => !a.GetAnnotations<ReconiliationAnnotation>().Any()))
            {
                this.m_dataCacheService.Remove(itm);

                itm.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
                yield return itm;
            }

            // We want to remove all encounters since encounters are difficult to reconcile accross the care plans
            foreach (var itm in existingCarePlan.Relationships.Where(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent && o.TargetAct is PatientEncounter))
            {
                this.m_dataCacheService.Remove(itm);
                itm.TargetAct.BatchOperation = Model.DataTypes.BatchOperationType.DeletePreserveContained;
                yield return itm.TargetAct;
            }

            // We will move the new encounters up
            foreach (var itm in updatedCarePlan.Relationships.Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent))
            {
                itm.TargetAct.Key = itm.TargetActKey = itm.TargetAct.Key ?? itm.TargetActKey ?? Guid.NewGuid();
                yield return itm.TargetAct;
                itm.TargetAct = null;
            }
            // Finally - match the care plan key
            updatedCarePlan.Key = existingCarePlan.Key;
            yield return updatedCarePlan;

        }
    }
}
