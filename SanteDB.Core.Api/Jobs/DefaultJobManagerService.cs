/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */

using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// Represents the default implementation of the timer
    /// </summary>
    [ServiceProvider("Default Job Manager", Configuration = typeof(JobConfigurationSection))]
    public class DefaultJobManagerService : IJobManagerService
    {
        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DefaultJobManagerService));

        // Thread pool
        private IThreadPoolService m_threadPool;

        /// <summary>
        /// Create a new job manager service
        /// </summary>
        public DefaultJobManagerService(IThreadPoolService threadPool)
        {
            this.m_threadPool = threadPool;
        }

        /// <summary>
        /// Job execution
        /// </summary>
        private class JobExecutionInfo
        {
            /// <summary>
            /// Start the job
            /// </summary>
            public JobExecutionInfo(IJob job, JobStartType startType, TimeSpan interval, Object[] parameters)
            {
                this.Job = job;
                this.StartType = startType;
                this.Schedule = new JobItemSchedule[]
                {
                    new JobItemSchedule()
                    {
                        Interval = interval.Milliseconds,
                        IntervalSpecified = true,
                        StartDate = DateTime.Now
                    }
                };
                this.Parameters = parameters;
            }

            /// <summary>
            /// Create a new job execution schedule
            /// </summary>
            /// <param name="configuration"></param>
            public JobExecutionInfo(JobItemConfiguration configuration)
            {
                this.Job = configuration.Type.CreateInjected() as IJob;
                this.Schedule = configuration.Schedule?.ToArray();
                this.StartType = configuration.StartType;
                this.Parameters = configuration.Parameters;
            }

            /// <summary>
            /// Gets the job
            /// </summary>
            public IJob Job { get; }

            /// <summary>
            /// The last time the job was run
            /// </summary>
            public DateTime? LastRun { get; set; }

            /// <summary>
            /// The last time the job was run
            /// </summary>
            public DateTime? LastFinish { get; set; }

            /// <summary>
            /// Days that the job should be run
            /// </summary>
            public JobItemSchedule[] Schedule { get; }

            /// <summary>
            /// Parameters for this object
            /// </summary>
            public object[] Parameters { get; }

            /// <summary>
            /// The start type of the job
            /// </summary>
            public JobStartType StartType { get; }
        }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Default Job Manager";

        /// <summary>
        /// Timer configuration
        /// </summary>
        private JobConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<JobConfigurationSection>();

        /// <summary>
        /// Timer thread
        /// </summary>
        private System.Timers.Timer m_systemTimer;

        /// <summary>
        /// Timer service is starting
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Timer service is stopping
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// Timer service is started
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Timer service is stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Log of timers
        /// </summary>
        private ConcurrentBag<JobExecutionInfo> m_jobs = new ConcurrentBag<JobExecutionInfo>();

        #region ITimerService Members

        /// <summary>
        /// Start the timer
        /// </summary>
        public bool Start()
        {
            Trace.TraceInformation("Starting timer service...");

            // Invoke the starting event handler
            this.Starting?.Invoke(this, EventArgs.Empty);

            foreach (var job in this.m_configuration.Jobs)
            {
                var ji = new JobExecutionInfo(job);
                this.m_jobs.Add(ji);

                if (job.StartType == JobStartType.Immediate)
                {
                    this.m_threadPool.QueueUserWorkItem(this.RunJob, ji);
                }
            }

            // Setup timers based on the jobs
            this.m_systemTimer = new System.Timers.Timer(600000); // timer runs every 10 minutes
            this.m_systemTimer.Elapsed += SystemJobTimer;

            this.Started?.Invoke(this, EventArgs.Empty);

            Trace.TraceInformation("Timer service started successfully");
            return true;
        }

        /// <summary>
        /// Run the specified job
        /// </summary>
        /// <param name="jobMeta"></param>
        private void RunJob(object jobMeta)
        {
            var jinfo = jobMeta as JobExecutionInfo;
            jinfo.LastRun = DateTime.Now;
            try
            {
                jinfo.Job.Run(this, EventArgs.Empty, jinfo.Parameters);
            }
            catch (Exception ex)
            {
                this.m_tracer.TraceError("Error running job: {0} - {1}", jinfo.Job.Name, ex);
            }
        }

        /// <summary>
        /// Fired when the job timer expires
        /// </summary>
        private void SystemJobTimer(object sender, ElapsedEventArgs e)
        {
            // Iterate through our jobs
            foreach (var itm in this.m_jobs)
            {
                // Does the job have a schedule?
                if (itm.Schedule == null || !itm.Schedule.Any() || itm.StartType == JobStartType.Never)
                {
                    continue;
                }

                // Do any of the schedules fit with the current system time?
                var scheduleHits = itm.Schedule.Where(s => s.AppliesTo(DateTime.Now, itm.LastRun));
                if (scheduleHits.Any() || itm.StartType == JobStartType.DelayStart && !itm.LastRun.HasValue)
                {
                    this.m_tracer.TraceVerbose("Job {0} schedule {1} hits {2} scheduled times", itm.Job.Name, String.Join(";", itm.Schedule.Select(o => o.ToString())), string.Join(";", scheduleHits.Select(o => o.ToString())));
                    if (itm.Job.CurrentState != JobStateType.Running)
                    {
                        this.m_tracer.TraceInfo("Starting job {0}", itm.Job.Name);
                        this.m_threadPool.QueueUserWorkItem(this.RunJob, itm);
                    }
                }
                else
                {
                    this.m_tracer.TraceVerbose("Job {0} scheduled {1} not applicable", itm.Job.Name, String.Join(";", itm.Schedule.Select(o => o.ToString())));
                }
            }
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public bool Stop()
        {
            // Stop all timers
            Trace.TraceInformation("Stopping timer service...");
            this.Stopping?.Invoke(this, EventArgs.Empty);

            this.m_systemTimer.Dispose();
            this.m_systemTimer = null;
            foreach (var itm in this.m_jobs)
            {
                if (itm.Job is IDisposable disp)
                {
                    disp.Dispose();
                }
            }

            this.Stopped?.Invoke(this, EventArgs.Empty);

            Trace.TraceInformation("Timer service stopped successfully");
            return true;
        }

        /// <summary>
        /// Add a job
        /// </summary>
        public void AddJob(IJob jobObject, TimeSpan elapseTime, JobStartType startType = JobStartType.Immediate)
        {
            if (this.IsJobRegistered(jobObject.GetType()))
                return; // Job is already added

            var ji = new JobExecutionInfo(jobObject, startType, elapseTime, new object[0]);
            this.m_jobs.Add(ji);
            if (startType == JobStartType.Immediate)
            {
                this.m_threadPool.QueueUserWorkItem(this.RunJob, ji);
            }
        }

        /// <summary>
        /// Return true if job object is registered
        /// </summary>
        public bool IsJobRegistered(Type jobObject)
        {
            return this.m_jobs.Any(o => o.Job.GetType() == jobObject);
        }

        /// <summary>
        /// Returns true when the service is running
        /// </summary>
        public bool IsRunning { get { return this.m_systemTimer != null; } }

        /// <summary>
        /// Get the jobs
        /// </summary>
        public IEnumerable<IJob> Jobs
        {
            get
            {
                return this.m_jobs.Select(o => o.Job);
            }
        }

        /// <summary>
        /// Get the last time the job was run
        /// </summary>
        public DateTime? GetLastRuntime(IJob job)
        {
            return this.m_jobs.FirstOrDefault(o => o.Job == job)?.LastRun;
        }

        /// <summary>
        /// Start a job right now
        /// </summary>
        public void StartJob(IJob job, object[] parameters)
        {
            this.m_threadPool.QueueUserWorkItem(this.RunJob,
                new JobExecutionInfo(job, JobStartType.Immediate, TimeSpan.MinValue, parameters.Where(o => o != null).ToArray()));
        }

        /// <summary>
        /// Get the specified job instance
        /// </summary>
        public IJob GetJobInstance(Guid jobKey)
        {
            return this.m_jobs.FirstOrDefault(o => o.Job.Id == jobKey)?.Job;
        }

        /// <summary>
        /// Start a job
        /// </summary>
        public void StartJob(Type jobType, object[] parameters)
        {
            var job = this.m_jobs.FirstOrDefault(o => o.Job.GetType() == jobType);
            if (job == null)
            {
                this.m_threadPool.QueueUserWorkItem(this.RunJob, job);
            }
        }

        #endregion ITimerService Members
    }
}