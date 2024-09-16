using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
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
        private readonly INotifyRepositoryService<Patient> m_patientRepository;
        private readonly IRepositoryService<CarePlan> m_carePlanRepository;
        private readonly INotifyRepositoryService<CarePathwayDefinition> m_carePathwayRepository;
        private readonly IJobManagerService m_jobManager;
        private readonly IDecisionSupportService m_decisionSupportService;
        private readonly ConcurrentDictionary<Guid, Func<Patient, bool>> m_compiledExpressions = new ConcurrentDictionary<Guid, Func<Patient, bool>>();

        /// <summary>DI constructor</summary>
        public DefaultCarepathEnrolmentService(
            IPolicyEnforcementService policyService,
            INotifyRepositoryService<CarePathwayDefinition> carePathwayRepository,
            IJobManagerService jobManager,
            IDecisionSupportService decisionSupportService,
            IPrivacyEnforcementService privacyService,
            INotifyRepositoryService<Patient> patientRepository,
            IRepositoryService<CarePlan> careplanRepository) 
        {
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
        }

        /// <inheritdoc/>
        public string ServiceName => "Default Care Pathway Management Service";

        /// <summary>
        /// Refresh for automatic enrolment
        /// </summary>
        private void carePathwayRepositoryChange(object sender, Event.DataPersistedEventArgs<CarePathwayDefinition> e)
        {
            if (e.Data.EnrolmentMode == CarePathwayEnrolmentMode.Automatic)
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
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }
            else if (carePathway == null)
            {
                throw new ArgumentNullException(nameof(carePathway));
            }
            else if (this.IsEnrolled(patient, carePathway))
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, String.Format("{0} already enrolled in {1}", patient, carePathway)));
            }
            else if (!this.ValidateEligibilityInternal(patient, carePathway))
            {
                throw new DetectedIssueException(BusinessRules.DetectedIssuePriorityType.Error, "carepath.enroll.eligibility", String.Format("Patient {0} is ineligible to be enrolled in {1}", patient, carePathway), DetectedIssueKeys.SafetyConcernIssue, null);
            }

            var cp = this.m_decisionSupportService.CreateCarePlan(patient, true, new Dictionary<String, object>() { { "pathway", carePathway.Mnemonic } });
            cp.StatusConceptKey = StatusKeys.Active;
            return this.m_carePlanRepository.Insert(cp);
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
            else if (!this.IsEnrolled(patient, carePathway))
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, String.Format("{0} already enrolled in {1}", patient, carePathway)));
            }

            var cp = this.m_carePlanRepository.Find(o => o.CarePathwayKey == carePathway.Key && o.Participations.Where(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget).Any(p => p.PlayerEntityKey == patient.Key) && o.StatusConceptKey == StatusKeys.Active).FirstOrDefault();
            cp.StatusConceptKey = StatusKeys.Cancelled;
            return this.m_carePlanRepository.Save(cp);
        }

        /// <inheritdoc/>
        public bool IsEnrolled(Patient patient, CarePathwayDefinition carePathway)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }
            else if (carePathway == null)
            {
                throw new ArgumentNullException(nameof(carePathway));
            }
            return this.m_carePlanRepository.Find(o => o.CarePathwayKey == carePathway.Key && o.Participations.Where(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget).Any(p => p.PlayerEntityKey == patient.Key) && o.StatusConceptKey == StatusKeys.Active).Any();
        }
    }
}
