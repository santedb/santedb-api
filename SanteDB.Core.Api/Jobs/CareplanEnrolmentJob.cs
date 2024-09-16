using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// Careplan enrolment job
    /// </summary>
    public class CareplanEnrolmentJob : IJob
    {

        // Job id
        public static readonly Guid JOB_ID = Guid.Parse("D720866E-EDDC-4BAF-B8E8-8DAF01CD3F1A");
        private readonly IJobStateManagerService m_jobStateManager;
        private readonly IRepositoryService<Patient> m_patientRepository;
        private readonly ICarePathwayDefinitionRepositoryService m_carePathwayService;
        private readonly ICarePathwayEnrollmentService m_carePathwayEnrollmentService;
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(CareplanEnrolmentJob));
        private bool m_cancel = false;

        /// <summary>
        /// Careplan enrolment job
        /// </summary>
        public CareplanEnrolmentJob(
            ICarePathwayEnrollmentService carePathwayEnrollmentService, 
            ICarePathwayDefinitionRepositoryService carePathwayDefinitionRepositoryService,
            IRepositoryService<Patient> patientRepository,
            IJobStateManagerService jobStateManagerService)
        {
            this.m_jobStateManager = jobStateManagerService;
            this.m_patientRepository = patientRepository;
            this.m_carePathwayService = carePathwayDefinitionRepositoryService;
            this.m_carePathwayEnrollmentService = carePathwayEnrollmentService;
        }

        /// <inheritdoc/>
        public Guid Id => JOB_ID;

        /// <inheritdoc/>
        public string Name => "Carepath Enrolment Job";

        /// <inheritdoc/>
        public string Description => "For care-pathways which are set to automatic enrolment, this job ensures that patients who are eligible are enroled";

        /// <inheritdoc/>
        public bool CanCancel => true;

        /// <inheritdoc/>
        public IDictionary<string, Type> Parameters => new Dictionary<String, Type>
        {
            { "pathwayId", typeof(Guid) }
        };

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_cancel = true;
        }

        /// <inheritdoc/>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {
                this.m_cancel = false;
                this.m_jobStateManager.SetState(this, JobStateType.Running);
                var pathways = new List<CarePathwayDefinition>(10);
                if(parameters.Length == 1 && parameters[0] is Guid pathwayId)
                {
                    pathways.Add(this.m_carePathwayService.Get(pathwayId));
                }
                else
                {
                    pathways.AddRange(this.m_carePathwayService.Find(o => o.EnrolmentMode == CarePathwayEnrolmentMode.Automatic));
                }

                foreach(var cp in pathways)
                {
                    this.m_tracer.TraceInfo("Performing automatic enrolment for {0} -- ", cp.Mnemonic);
                    // Fetch all patients who are not currently enrolled
                    var enrolmentCriteria = QueryExpressionParser.BuildLinqExpression<Patient>(cp.EligibilityCriteria);
                    var eligiblePatients = this.m_patientRepository.Find(enrolmentCriteria)
                        .Except(o => o.Participations.Where(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget).Any(r => (r.Act as CarePlan).CarePathwayKey == cp.Key && r.Act.StatusConceptKey == StatusKeys.Active));
                    var ec = eligiblePatients.Count();
                    this.m_tracer.TraceInfo("Will enroll {0} patients into {1}", ec, cp.Mnemonic);
                    var i = 0;
                    foreach(var pat in eligiblePatients)
                    {
                        if(this.m_cancel)
                        {
                            this.m_jobStateManager.SetState(this, JobStateType.Cancelled);
                            return;
                        }
                        this.m_carePathwayEnrollmentService.Enroll(pat, cp);
                        this.m_jobStateManager.SetProgress(this, $"{i++} of {ec}", (float)i / (float)ec);
                    }
                }

                this.m_jobStateManager.SetState(this, JobStateType.Completed);
            }
            catch(Exception ex)
            {
                this.m_tracer.TraceError("Error running care path enrolment: {0}", ex);
                this.m_jobStateManager.SetState(this, JobStateType.Aborted, ex.ToHumanReadableString());
            }
        }
    }
}
