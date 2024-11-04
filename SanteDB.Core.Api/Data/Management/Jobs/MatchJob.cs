/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SanteDB.Core.Data.Management.Jobs
{
    /// <summary>
    /// Provides a base for matching jobs
    /// </summary>
    public class MatchJob<T> : IJob
        where T : IdentifiedData, new()
    {
        // Guid
        private readonly Guid m_id;

        private bool m_cancelRequested = false;
        // Merge service
        private IRecordMergingService<T> m_mergeService;
        private readonly IJobStateManagerService m_stateManager;

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(MatchJob<T>));

        /// <summary>
        /// Create a match job
        /// </summary>
        public MatchJob(IRecordMergingService<T> recordMergingService, IJobManagerService jobManager, IJobStateManagerService stateManagerService)
        {
            this.m_id = new Guid(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(typeof(T).Name)));

            this.m_mergeService = recordMergingService;
            this.m_stateManager = stateManagerService;

            // Progress change handler
            if (this.m_mergeService is IReportProgressChanged rpt)
            {
                rpt.ProgressChanged += (o, p) =>
                {
                    this.m_stateManager.SetProgress(this, p.State.ToString(), p.Progress);
                };
            }
        }

        /// <summary>
        /// Get the identifier
        /// </summary>
        public Guid Id => this.m_id;

        /// <summary>
        /// Name of the matching job
        /// </summary>
        public string Name => $"Background Matching Job for {typeof(T).Name}";


        /// <inheritdoc/>
        public string Description => $"Starts a background process which re-processes detected duplicate SOURCE records for {typeof(T).Name}";

        /// <summary>
        /// Can cancel the job?
        /// </summary>
        public bool CanCancel => true;

        /// <summary>
        /// Gets the parameters for the job
        /// </summary>
        public IDictionary<string, Type> Parameters => new Dictionary<String, Type>()
        {
            { "clearExistingData", typeof(bool) }
        };

        /// <summary>
        /// Cancel the job
        /// </summary>
        public void Cancel()
        {
            this.m_mergeService.CancelDetectGlobalMergeCandidates();
            this.m_cancelRequested = true;
        }

        /// <summary>
        /// Run the specified job
        /// </summary>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    this.m_cancelRequested = false;
                    this.m_stateManager.SetState(this, JobStateType.Running);
                    var clear = parameters.Length > 0 ? (bool?)parameters[0] : false;
                    this.m_tracer.TraceInfo("Starting batch run of Matching ");

                    if (clear.GetValueOrDefault())
                    {
                        this.m_tracer.TraceVerbose("Batch instruction indicates clear of all links");
                        this.m_mergeService.ClearGlobalIgnoreFlags();
                        this.m_mergeService.ClearGlobalMergeCanadidates();
                    }
                    else
                    {
                        this.m_mergeService.ClearGlobalMergeCanadidates();
                    }

                    this.m_mergeService.DetectGlobalMergeCandidates();

                    if (this.m_cancelRequested)
                    {
                        this.m_stateManager.SetState(this, JobStateType.Cancelled);
                    }
                    else
                    {
                        this.m_stateManager.SetState(this, JobStateType.Completed);
                    }
                }
            }
            catch (Exception ex)
            {
                this.m_stateManager.SetState(this, JobStateType.Aborted, ex.ToHumanReadableString());
                this.m_stateManager.SetProgress(this, ex.Message, 0.0f);
                this.m_tracer.TraceError("Could not run Matching Job: {0}", ex);
            }
        }
    }
}
