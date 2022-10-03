/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Core.Jobs
{


    /// <summary>
    /// Job state in XML form
    /// </summary>
    [XmlType(nameof(XmlJobState), Namespace = "http://santedb.org/job/state")]
    public class XmlJobState : IJobState
    {
        /// <summary>
        /// Gets or sets the job
        /// </summary>
        [XmlIgnore]
        public IJob Job => ApplicationServiceContext.Current.GetService<IJobManagerService>().GetJobInstance(this.JobId);

        /// <summary>
        /// Gets or sets the job identifier
        /// </summary>
        [XmlAttribute("job")]
        public Guid JobId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the current status text of the job
        /// </summary>
        [XmlIgnore]
        public string StatusText { get; set; }

        /// <summary>
        /// Gets or sets the current progress
        /// </summary>
        [XmlIgnore]
        public float Progress { get; set; }

        /// <summary>
        /// Gets or sets the current state of the job
        /// </summary>
        [XmlAttribute("lastState")]
        public JobStateType CurrentState { get; set; }

        /// <summary>
        /// Gets or sets the last start time
        /// </summary>
        [XmlElement("lastStart")]
        public DateTime? LastStartTime { get; set; }

        /// <summary>
        /// Gets or sets the last stop time
        /// </summary>
        [XmlElement("lastStop")]
        public DateTime? LastStopTime { get; set; }
    }


    /// <summary>
    /// A simple job state manager class which controls job state via an XML file
    /// </summary>
    public class XmlFileJobStateManager : IJobStateManagerService, IDisposable
    {


        // Get job states of the job objects
        private readonly ConcurrentBag<XmlJobState> m_jobStates;

        // Cron tab location
        private readonly string m_jobStateLocation;

        // Serializer
        private readonly XmlSerializer m_xsz = new XmlSerializer(typeof(List<XmlJobState>));

        // Tracer for job manager
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(XmlFileJobStateManager));

        /// <summary>
        /// Creates a new job state manager and loads the persisted state file
        /// </summary>
        public XmlFileJobStateManager()
        {
            this.m_jobStateLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "xstate.xml");
            if (File.Exists(this.m_jobStateLocation))
            {
                try
                {
                    using (var fs = File.OpenRead(this.m_jobStateLocation))
                    {
                        this.m_jobStates = new ConcurrentBag<XmlJobState>(this.m_xsz.Deserialize(fs) as List<XmlJobState>);
                    }
                }
                catch
                {
                    this.m_jobStates = new ConcurrentBag<XmlJobState>();
                }
            }
            else
            {
                this.m_jobStates = new ConcurrentBag<XmlJobState>();
            }
        }

        /// <inheritdoc/>
        public IJobState GetJobState(IJob job)
        {
            var jobState = this.m_jobStates.FirstOrDefault(o => o.JobId == job.Id);
            if (jobState == null)
            {
                jobState = new XmlJobState()
                {
                    JobId = job.Id,
                    CurrentState = JobStateType.NotRun
                };
                this.m_jobStates.Add(jobState);
            }
            return jobState;
        }

        /// <inheritdoc/>
        public void SetProgress(IJob job, string statusText, float progress)
        {
            var jobData = this.m_jobStates.FirstOrDefault(o => o.JobId == job.Id);
            if (jobData == null)
            {
                this.m_jobStates.Add(new XmlJobState()
                {
                    StatusText = statusText,
                    Progress = progress,
                    JobId = job.Id,
                    CurrentState = JobStateType.Running,
                    LastStartTime = DateTime.Now
                });
            }
            else
            {
                jobData.StatusText = statusText;
                jobData.Progress = progress;
            }
        }

        /// <inheritdoc/>
        public void SetState(IJob job, JobStateType state)
        {
            var jobData = this.m_jobStates.FirstOrDefault(o => o.JobId == job.Id);
            if (jobData == null)
            {
                jobData = new XmlJobState()
                {
                    JobId = job.Id,
                    CurrentState = state,
                    LastStartTime = DateTime.Now
                };
                this.m_jobStates.Add(jobData);
            }

            // Determine state transition
            switch (state)
            {
                case JobStateType.Running:
                    if (!jobData.IsRunning())
                    {
                        jobData.LastStartTime = DateTime.Now;
                        jobData.LastStopTime = null;
                    }
                    break;
                case JobStateType.Starting:
                    jobData.LastStartTime = DateTime.Now;
                    jobData.LastStopTime = null;
                    break;
                case JobStateType.Completed:
                    jobData.LastStopTime = DateTime.Now;
                    break;
            }
            jobData.CurrentState = state;

            this.SaveState();
        }

        /// <summary>
        /// Save the state of the jobs
        /// </summary>
        private void SaveState()
        {
            try
            {
                using (var fs = File.Create(this.m_jobStateLocation))
                {
                    this.m_xsz.Serialize(fs, this.m_jobStates.ToList());
                }
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error saving job states: {0}", e);
                throw new Exception("Error persisting job status", e);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.SaveState();
        }
    }
}