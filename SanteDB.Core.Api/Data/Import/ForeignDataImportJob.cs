/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Jobs;
using SanteDB.Core.PubSub;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A <see cref="IJob"/> implementation that executes all <see cref="IForeignDataSubmission"/> instructions in a 
    /// state of ready.
    /// </summary>
    public class ForeignDataImportJob : IJob
    {
        /// <summary>
        /// JOB ID
        /// </summary>
        public static readonly Guid JOB_ID = Guid.Parse("2EBF8094-6628-4CEC-93B8-3D623F227722");
        private static readonly Guid REBUILD_FTI_JOB_ID = Guid.Parse("4D7A0641-762F-45BC-83AF-2001887648B1");
        private static readonly Guid REBUILD_MATL_VW_JOB_ID = Guid.Parse("B5D6A459-C0FC-4D2F-A653-733C849BEAB9");

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(ForeignDataImportJob));
        private readonly IJobManagerService m_jobManager;
        private readonly IJobStateManagerService m_jobStateManager;
        private readonly IForeignDataManagerService m_fdManager;
        private float m_fdManagerProgress = 0.0f;
        private string m_fdManagerState = String.Empty;
        private bool m_cancelRequested = false;

        /// <summary>
        /// DI constructor
        /// </summary>
        public ForeignDataImportJob(IJobStateManagerService jobStateManagerService,
            IJobManagerService jobManagerService,
            IForeignDataManagerService foreignDataManagerService)
        {
            this.m_jobManager = jobManagerService;
            this.m_jobStateManager = jobStateManagerService;
            this.m_fdManager = foreignDataManagerService;
            if (this.m_fdManager is IReportProgressChanged irpc2)
            {
                irpc2.ProgressChanged += (o, e) =>
                {
                    this.m_fdManagerProgress = e.Progress;
                    this.m_fdManagerState = e.State.ToString();
                };
            }


        }

        /// <inheritdoc/>
        public Guid Id => JOB_ID;

        /// <inheritdoc/>
        public string Name => "Foreign Data Import Background Job";

        /// <inheritdoc/>
        public string Description => "Processes staged foreign data imports";

        /// <inheritdoc/>
        public bool CanCancel => true;

        /// <inheritdoc/>
        public IDictionary<string, Type> Parameters => new Dictionary<String, Type>()
        {
            { "submissionId", typeof(String) }
        };

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_cancelRequested = true;
            this.m_jobStateManager.SetState(this, JobStateType.Cancelled);

        }

        /// <inheritdoc/>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {
                this.m_jobStateManager.SetState(this, JobStateType.Running);

                var scheduledJobs = this.m_fdManager.Find(o => o.Status == ForeignDataStatus.Scheduled).ToArray();
                var progressPerFile = 1.0f / (float)scheduledJobs.Length;
                int fileIndex = 0;
                Guid? submissionId = null;
                if (parameters.Length > 0 && parameters[0] != null)
                {
                    submissionId = Guid.Parse(parameters[0].ToString());
                }

                using (new Timer((o) => this.m_jobStateManager.SetProgress(this, this.m_fdManagerState, this.m_fdManagerProgress * progressPerFile + fileIndex * progressPerFile), null, 100, 1000))
                {
                    using (AuthenticationContext.EnterSystemContext())
                    {
                        this.m_cancelRequested = false;
                        for (fileIndex = 0; fileIndex < scheduledJobs.Length && !this.m_cancelRequested; fileIndex++)
                        {
                            if (!submissionId.HasValue || submissionId.Equals(scheduledJobs[fileIndex].Key))
                            {
                                this.m_fdManager.Execute(scheduledJobs[fileIndex].Key.Value);
                            }
                        }

                    }
                }
                if (this.m_cancelRequested)
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Cancelled);
                }
                else
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Completed);
                    this.m_jobManager.StartJob(this.m_jobManager.Jobs.First(o => o.Id == REBUILD_FTI_JOB_ID), new object[0]);
                    this.m_jobManager.StartJob(this.m_jobManager.Jobs.First(o => o.Id == REBUILD_MATL_VW_JOB_ID), new object[0]);
                }
            }
            catch (Exception ex)
            {
                this.m_tracer.TraceError("Error executing transform job: {0}", ex);
                this.m_jobStateManager.SetState(this, JobStateType.Aborted, ex.ToHumanReadableString());
            }
        }
    }
}
