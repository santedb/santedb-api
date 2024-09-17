using SanteDB.Core.BusinessRules;
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
    public class DefaultCarepathEnrolmentService : ICarePathwayEnrollmentService, IDisposable
    {

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultCarepathEnrolmentService));
        private readonly CarePathwayConfigurationSection m_configuration;
        private readonly IPolicyEnforcementService m_pepService;
        private readonly INotifyRepositoryService<Patient> m_patientRepository;
        private readonly IRepositoryService<CarePlan> m_carePlanRepository;
        private readonly INotifyRepositoryService<CarePathwayDefinition> m_carePathwayRepository;
        private readonly IJobManagerService m_jobManager;
        private readonly IDecisionSupportService m_decisionSupportService;
        private readonly ConcurrentDictionary<Guid, Func<Patient, bool>> m_compiledExpressions = new ConcurrentDictionary<Guid, Func<Patient, bool>>();

        /// <summary>DI constructor</summary>
        public DefaultCarepathEnrolmentService(
            IConfigurationManager configurationManager,
            IPolicyEnforcementService policyService,
            INotifyRepositoryService<CarePathwayDefinition> carePathwayRepository,
            IJobManagerService jobManager,
            IDecisionSupportService decisionSupportService,
            IPrivacyEnforcementService privacyService,
            INotifyRepositoryService<Patient> patientRepository,
            INotifyRepositoryService<Bundle> bundleRepository,
            IRepositoryService<CarePlan> careplanRepository) 
        {
            this.m_configuration = configurationManager.GetSection<CarePathwayConfigurationSection>() ?? new CarePathwayConfigurationSection()
            {
                EnableAutoEnrollment = true
            };
            this.m_pepService = policyService;
            this.m_patientRepository = patientRepository;
            this.m_carePlanRepository = careplanRepository;
            this.m_carePathwayRepository = carePathwayRepository;
            this.m_jobManager = jobManager;
            this.m_decisionSupportService = decisionSupportService;
            // Monitor the patient registration subsystem
            this.m_patientRepository.Inserted += patientRepositoryChange;
            this.m_patientRepository.Saved += patientRepositoryChange;
            this.m_carePathwayRepository.Inserted += carePathwayRepositoryChange;
            this.m_carePathwayRepository.Saved += carePathwayRepositoryChange;
            bundleRepository.Inserting += bundleRepositoryChange;
            bundleRepository.Saving += bundleRepositoryChange;
            // Register the care planning job
            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                if (!this.m_jobManager.IsJobRegistered(typeof(CareplanEnrolmentJob)))
                {
                    this.m_jobManager.RegisterJob(typeof(CareplanEnrolmentJob));
                }
            };
        }

        private void bundleRepositoryChange(object sender, DataPersistingEventArgs<Bundle> e)
        {
            foreach (var p in e.Data.Item.OfType<Patient>().ToArray())
            {
                foreach (var cp in this.GetEligibleCarePaths(p))
                {
                    if (cp.EnrolmentMode == CarePathwayEnrolmentMode.Automatic)
                    {
                        this.m_tracer.TraceInfo("Patient {0} meets eligibility criteria for {1} - automatically enrolling", p, cp);
                        if (!e.Data.Item.OfType<CarePlan>().Any(c => c.CarePathwayKey == cp.Key))
                        {
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
            actRelationship.TargetActKey = actRelationship.TargetAct.Key = actRelationship.TargetAct.Key ?? Guid.NewGuid();
            var ta = actRelationship.TargetAct;
            actRelationship.TargetAct = null;
            if(ta.Relationships?.Any() == true)
            {
                foreach(var itm in ta.Relationships.SelectMany(o=>this.ExtractCarePlanObjects(o)))
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
            if (e.Data.EnrolmentMode == CarePathwayEnrolmentMode.Automatic && this.m_configuration.EnableAutoEnrollment)
            {
                this.m_jobManager.StartJob(typeof(CareplanEnrolmentJob), new object[] { e.Data.Key.Value });
            }
            this.m_compiledExpressions.TryRemove(e.Data.Key.Value, out _);
        }

        /// <summary>
        /// Monitor callback for registered patients - will apply automatic enrolment
        /// </summary>
        private void patientRepositoryChange(object sender, Event.DataPersistedEventArgs<Patient> e)
        {
            foreach (var cp in this.GetEligibleCarePaths(e.Data))
            {
                if (cp.EnrolmentMode == CarePathwayEnrolmentMode.Automatic)
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
            var cp = this.m_decisionSupportService.CreateCarePlan(patient, true, new Dictionary<String, object>() { { "pathway", carePathway.Mnemonic } });
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
        public IEnumerable<CarePlan> GetEnrolledCarePaths(Patient patient)
        {
            // Get the care pathways
            var cpIds = this.m_carePathwayRepository.Find(o => o.ObsoletionTime == null).Select(o => o.Key.Value).ToArray();
            return this.m_carePlanRepository.Find(o => o.Participations.Where(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget).Any(p => p.PlayerEntityKey == patient.Key) && o.StatusConceptKey == StatusKeys.Active && cpIds.Contains(o.CarePathwayKey.Value));
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
                return this.m_carePlanRepository.Save(carePlan);
            }
            else
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, String.Format("{0} already enrolled in {1}", patient, carePathway)));
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
    }
}
