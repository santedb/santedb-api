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
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.Data.Backup;
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
        /// Default ctor
        /// </summary>
        public XmlJobState()
        {
        }

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
    public class XmlFileJobStateManager : IJobStateManagerService, IDisposable, IProvideBackupAssets, IRestoreBackupAssets
    {

        private readonly Guid JOB_STATE_ASSET_ID = Guid.Parse("BFB822E4-633F-49CA-8459-1DDBD7C435B5");

        // Get job states of the job objects
        private readonly ConcurrentBag<XmlJobState> m_jobStates;

        // Cron tab location
        private readonly string m_jobStateLocation;

        // Serializer
        private readonly XmlSerializer m_xsz = new XmlSerializer(typeof(List<XmlJobState>));

        // Tracer for job manager
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(XmlFileJobStateManager));

        // Lock
        private object m_lock = new object();

        /// <summary>
        /// Creates a new job state manager and loads the persisted state file
        /// </summary>
        public XmlFileJobStateManager()
        {
            var assembly = Assembly.GetEntryAssembly();

            if (null == assembly) //Fixes an issue where EntryAssembly is null when called from NUnit.
            {
                assembly = Assembly.GetCallingAssembly();
            }


            if (null != assembly)
            {
                try
                {
                    var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
                    if (dataDirectory is String ddir)
                    {
                        this.m_jobStateLocation = Path.Combine(ddir, "xstate.xml");
                    }
                    else
                    {
                        this.m_jobStateLocation = Path.Combine(Path.GetDirectoryName(assembly.Location), "xstate.xml");
                    }
                }
                catch (NotSupportedException)
                {
                    this.m_jobStateLocation = Path.Combine(System.Environment.CurrentDirectory, "xstate.xml");
                }
            }
            else
            {
                this.m_jobStateLocation = Path.Combine(System.Environment.CurrentDirectory, "xstate.xml");
            }

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
        public void SetState(IJob job, JobStateType state, string statusText)
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
                        jobData.Progress = 0.0f;
                    }
                    break;
                case JobStateType.Starting:
                    jobData.LastStartTime = DateTime.Now;
                    jobData.LastStopTime = null;
                    jobData.Progress = 0.0f;
                    break;
                case JobStateType.Completed:
                    jobData.LastStopTime = DateTime.Now;
                    jobData.Progress = 1.0f;
                    break;
            }
            jobData.CurrentState = state;
            jobData.StatusText = statusText;
            this.SaveState();
        }

        /// <summary>
        /// Save the state of the jobs
        /// </summary>
        private void SaveState()
        {
            try
            {
                lock (this.m_lock)
                {
                    using (var fs = File.Create(this.m_jobStateLocation))
                    {
                        this.m_xsz.Serialize(fs, this.m_jobStates.ToList());
                    }
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


        /// <inheritdoc/>
        public Guid[] AssetClassIdentifiers => new Guid[] { JOB_STATE_ASSET_ID };

        /// <inheritdoc/>
        public IEnumerable<IBackupAsset> GetBackupAssets()
        {
            lock (this.m_lock)
            {
                yield return new FileBackupAsset(JOB_STATE_ASSET_ID, Path.GetFileName(this.m_jobStateLocation), this.m_jobStateLocation);
            }
        }

        /// <inheritdoc/>
        public bool Restore(IBackupAsset backupAsset)
        {
            if (backupAsset == null)
            {
                throw new ArgumentNullException(nameof(backupAsset));
            }
            else if (backupAsset.AssetClassId != JOB_STATE_ASSET_ID)
            {
                throw new InvalidOperationException();
            }

            lock (this.m_lock)
            {
                using (var fs = File.Create(this.m_jobStateLocation))
                {
                    using (var astr = backupAsset.Open())
                    {
                        astr.CopyTo(fs);
                    }
                    fs.Seek(0, SeekOrigin.Begin);

                    // Clear the current bag
                    while (this.m_jobStates.TryTake(out _))
                    {
                        ;
                    }

                    foreach (var itm in this.m_xsz.Deserialize(fs) as List<XmlJobState>)
                    {
                        this.m_jobStates.Add(itm);
                    }
                    return true;
                }
            }
        }
    }
}