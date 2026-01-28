/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Features;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security;
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
    public class CareplanEnrollmentJob : IJob
    {

        // Job id
        public static readonly Guid JOB_ID = Guid.Parse("D720866E-EDDC-4BAF-B8E8-8DAF01CD3F1A");
        private readonly CarePathwayConfigurationSection m_configuration;
        private readonly IRepositoryService<CarePlan> m_careplanRepository;
        private readonly IJobStateManagerService m_jobStateManager;
        private readonly IRepositoryService<Patient> m_patientRepository;
        private readonly ICarePathwayDefinitionRepositoryService m_carePathwayService;
        private readonly ICarePathwayEnrollmentService m_carePathwayEnrollmentService;
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(CareplanEnrollmentJob));
        private bool m_cancel = false;

        /// <summary>
        /// Careplan enrolment job
        /// </summary>
        public CareplanEnrollmentJob(
            IConfigurationManager configurationManager,
            ICarePathwayEnrollmentService carePathwayEnrollmentService, 
            ICarePathwayDefinitionRepositoryService carePathwayDefinitionRepositoryService,
            IRepositoryService<Patient> patientRepository,
            IRepositoryService<CarePlan> careplanRepository,
            IJobStateManagerService jobStateManagerService)
        {
            this.m_configuration = configurationManager.GetSection<CarePathwayConfigurationSection>() ?? new CarePathwayConfigurationSection()
            {
                EnableAutoEnrollment = true
            };
            this.m_careplanRepository = careplanRepository;
            this.m_jobStateManager = jobStateManagerService;
            this.m_patientRepository = patientRepository;
            this.m_carePathwayService = carePathwayDefinitionRepositoryService;
            this.m_carePathwayEnrollmentService = carePathwayEnrollmentService;
        }

        /// <inheritdoc/>
        public Guid Id => JOB_ID;

        /// <inheritdoc/>
        public string Name => "Carepath Enrollment Job";

        /// <inheritdoc/>
        public string Description => "For care-pathways which are set to automatic enrolment, this job ensures that patients who are eligible are enroled";

        /// <inheritdoc/>
        public bool CanCancel => true;

        /// <inheritdoc/>
        public IDictionary<string, Type> Parameters => new Dictionary<String, Type>
        {
            { "pathwayId", typeof(String) },
            { "recomputeAll", typeof(bool) }
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
                using (AuthenticationContext.EnterSystemContext())
                {
                    this.m_cancel = false;
                    this.m_jobStateManager.SetState(this, JobStateType.Running);

                    // Enabled?
                    if (!this.m_configuration.EnableAutoEnrollment)
                    {
                        this.m_jobStateManager.SetState(this, JobStateType.Cancelled, "Configuration Prohibits Execution");
                    }
                    var pathways = new List<CarePathwayDefinition>(10);
                    if (parameters.Length == 1 && Guid.TryParse(parameters[0].ToString(), out Guid pathwayId))
                    {
                        pathways.Add(this.m_carePathwayService.Get(pathwayId));
                    }
                    else
                    {
                        pathways.AddRange(this.m_carePathwayService.Find(o => o.EnrollmentMode == CarePathwayEnrollmentMode.Automatic));
                    }

                    var resetPathway = parameters[1] is bool b && b;

                    foreach (var cp in pathways)
                    {
                        // Are we resetting?
                        if(resetPathway)
                        {
                            this.m_tracer.TraceInfo("Resetting all carelans in pathway {0} ---", cp.Mnemonic);

                            foreach (var c in this.m_careplanRepository.Find(o=>o.CarePathwayKey == cp.Key).Select(o=>o.Key).ToArray())
                            {
                                this.m_careplanRepository.Delete(c.Value);
                            }

                        }
                        this.m_tracer.TraceInfo("Performing automatic enrolment for {0} -- ", cp.Mnemonic);
                        // Fetch all patients who are not currently enrolled
                        var enrolmentCriteria = QueryExpressionParser.BuildLinqExpression<Patient>(cp.EligibilityCriteria);
                        var eligiblePatients = this.m_patientRepository.Find(enrolmentCriteria)
                            .Except(o => o.Participations.Where(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget).Any(r => (r.Act as CarePlan).CarePathwayKey == cp.Key && r.Act.StatusConceptKey == StatusKeys.Active)).ToArray();
                        var ec = eligiblePatients.Count();
                        this.m_tracer.TraceInfo("Will enroll {0} patients into {1}", ec, cp.Mnemonic);
                        var i = 0;
                        foreach (var pat in eligiblePatients)
                        {
                            if (this.m_cancel)
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
            }
            catch(Exception ex)
            {
                this.m_tracer.TraceError("Error running care path enrolment: {0}", ex);
                this.m_jobStateManager.SetState(this, JobStateType.Aborted, ex.ToHumanReadableString());
            }
        }
    }
}
