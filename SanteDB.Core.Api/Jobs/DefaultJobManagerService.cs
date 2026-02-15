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
 * Date: 2023-6-21
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// SanteDB's default implementation of the <see cref="IJobManagerService"/>
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    [ServiceProvider("Default Job Manager", Configuration = typeof(JobConfigurationSection))]
    public class DefaultJobManagerService : IJobManagerService, IServiceFactory, IDaemonService
    {
        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultJobManagerService));

        // Thread pool
        private IThreadPoolService m_threadPool;

        // Job schedule manager
        private readonly IJobScheduleManager m_jobScheduleManager;

        // Job state manager
        private readonly IJobStateManagerService m_jobStateManager;

        // Service manager
        private readonly IServiceManager m_serviceManager;
        private readonly IConfigurationManager m_configurationManager;

        /// <summary>
        /// Create a new job manager service
        /// </summary>
        public DefaultJobManagerService(IThreadPoolService threadPool, IServiceManager serviceManager, IConfigurationManager configurationManager, IJobStateManagerService jobStateManager = null, IJobScheduleManager cronTabManager = null)
        {
            this.m_threadPool = threadPool;
            this.m_jobScheduleManager = cronTabManager ?? new XmlFileJobScheduleManager();
            this.m_jobStateManager = jobStateManager ?? new XmlFileJobStateManager();
            this.m_serviceManager = serviceManager;
            this.m_configurationManager = configurationManager;
            this.m_configuration = configurationManager.GetSection<JobConfigurationSection>();
        }

        /// <summary>
        /// Job execution
        /// </summary>
        private class JobExecutionInfo
        {
            /// <summary>
            /// Start the job
            /// </summary>
            public JobExecutionInfo(IJob job, JobStartType startType, Object[] parameters)
            {
                this.Job = job;
                this.StartType = startType;
                this.Parameters = parameters;
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
        private JobConfigurationSection m_configuration;

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

        /// <inheritdoc/>
        public IEnumerable<Type> GetAvailableJobs() =>
            this.m_serviceManager.GetAllTypes()
                .Where(t => !t.IsGenericTypeDefinition && !t.IsInterface && !t.IsAbstract && typeof(IJob).IsAssignableFrom(t));

        #region ITimerService Members

        /// <summary>
        /// Start the timer
        /// </summary>
        public bool Start()
        {
            Trace.TraceInformation("Starting timer service...");

            // Invoke the starting event handler
            this.Starting?.Invoke(this, EventArgs.Empty);

            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                if (this.m_configuration != null)
                {
                    // Load from static configuration file and from the cron-tab
                    foreach (var configuration in this.m_configuration.Jobs.Where(j => !this.m_jobs.Any(r => r.Job.GetType() == j.Type)))
                    {
                        var job = configuration.Type.CreateInjected() as IJob;
                        var ji = new JobExecutionInfo(job, configuration.StartType, configuration.Parameters);
                        this.m_tracer.TraceInfo("Adding {0} from configuration (start type of {0})", ji.Job.Name, configuration.StartType);
                        this.m_jobs.Add(ji);

                        if (configuration.Schedule?.Any() == true)
                        {
                            this.m_jobScheduleManager.Clear(job);
                            configuration.Schedule.ForEach(s => this.m_jobScheduleManager.Add(job, s));
                        }

                        if (configuration.StartType == JobStartType.Immediate)
                        {
                            this.m_threadPool.QueueUserWorkItem(this.RunJob, ji);
                        }
                    }
                }

                foreach (var job in this.m_jobs)
                {
                    // The job is marked as running - this is a problem if the host shut down improperly
                    if (this.m_jobStateManager.GetJobState(job.Job).CurrentState == JobStateType.Running)
                    {
                        this.m_jobStateManager.SetState(job.Job, JobStateType.NotRun);
                    }
                }

                // Setup timers based on the jobs
                this.m_systemTimer = new System.Timers.Timer(300_000); // timer runs every 5 minutes
                this.m_systemTimer.Elapsed += SystemJobTimer;
                this.m_systemTimer.Enabled = true;
                this.m_systemTimer.Start();
            };

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
                this.m_tracer.TraceInfo("Will run job - {0}", jinfo.Job.Id);
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
            try
            {
                // Iterate through our jobs
                foreach (var itm in this.m_jobs)
                {
                    var schedule = this.m_jobScheduleManager.Get(itm.Job);

                    // Does the job have a schedule?
                    if (schedule?.Any() != true || itm.StartType == JobStartType.Never)
                    {
                        continue;
                    }

                    // Do any of the schedules fit with the current system time?
                    var scheduleHits = schedule.Where(s => s.AppliesTo(DateTime.Now, itm.LastRun));
                    if (scheduleHits.Any() || itm.StartType == JobStartType.DelayStart && !itm.LastRun.HasValue)
                    {
                        this.m_tracer.TraceVerbose("Job {0} schedule {1} hits {2} scheduled times", itm.Job.Name, String.Join(";", schedule.Select(o => o.ToString())), string.Join(";", scheduleHits.Select(o => o.ToString())));
                        if (!this.m_jobStateManager.GetJobState(itm.Job).IsRunning())
                        {
                            this.m_tracer.TraceInfo("Starting job {0}", itm.Job.Name);
                            this.m_threadPool.QueueUserWorkItem(this.RunJob, itm);
                        }
                    }
                    else
                    {
                        this.m_tracer.TraceVerbose("Job {0} scheduled {1} not applicable", itm.Job.Name, String.Join(";", schedule.Select(o => o.ToString())));
                    }
                }
            }
            catch (Exception ex)
            {
                this.m_tracer.TraceWarning("Could not automatically run jobs : {0}", ex);
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

            this.m_systemTimer?.Dispose();
            this.m_systemTimer = null;
            foreach (var itm in this.m_jobs)
            {
                if (itm.Job is IDisposable disp)
                {
                    disp.Dispose();
                }
                if (this.m_jobStateManager.GetJobState(itm.Job).CurrentState == JobStateType.Running)
                {
                    this.m_jobStateManager.SetState(itm.Job, JobStateType.Stopped);
                }
            }

            this.Stopped?.Invoke(this, EventArgs.Empty);

            Trace.TraceInformation("Timer service stopped successfully");
            return true;
        }

        /// <inheritdoc/>
        public void AddJob(IJob jobObject, TimeSpan interval, JobStartType startType = JobStartType.Immediate)
        {
            this.AddJob(jobObject, startType);
            this.SetJobSchedule(jobObject, interval);
        }

        /// <summary>
        /// Registers the job and attempts to save the configuration
        /// </summary>
        public IJob RegisterJob(Type jobType)
        {

            if (this.IsJobRegistered(jobType))
            {
                return null; // Job is already added
            }

            var jobObject = jobType.CreateInjected() as IJob;
            var ji = new JobExecutionInfo(jobObject, JobStartType.Never, new object[0]);
            this.m_jobs.Add(ji);

            // Try to save the configuration 
            try
            {

                // Is there no configuration?
                if (this.m_configuration == null)
                {
                    this.m_configuration = new JobConfigurationSection() { Jobs = new List<JobItemConfiguration>() };
                    this.m_configurationManager.Configuration.AddSection(this.m_configuration);
                }

                if (!this.m_configuration.Jobs.Any(j => j.Type == jobType))
                {
                    this.m_configuration.Jobs.Add(new JobItemConfiguration()
                    {
                        StartType = JobStartType.Never,
                        Type = jobType
                    });
                }


                if (!this.m_configurationManager.IsReadonly)
                {
                    ApplicationServiceContext.Current.GetService<IConfigurationManager>().SaveConfiguration();
                }
                return jobObject;
            }
            catch (Exception e)
            {
                throw new Exception(ErrorMessages.WARN_CONFIGURATION_NOT_UPDATED, e);
            }
        }

        /// <inheritdoc/>
        public void AddJob(IJob jobObject, JobStartType startType = JobStartType.Immediate)
        {
            if (this.IsJobRegistered(jobObject.GetType()))
            {
                return; // Job is already added
            }

            var ji = new JobExecutionInfo(jobObject, startType, new object[0]);
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
            return this.m_jobs.Any(o => o.Job.GetType() == jobObject) || this.m_configuration?.Jobs.Any(j=>j.Type == jobObject) == true;
        }

        /// <summary>
        /// Returns true when the service is running
        /// </summary>
        public bool IsRunning
        { get { return this.m_systemTimer != null; } }

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
            // TODO: Audit
            this.m_tracer.TraceInfo("Manually starting job {0}", job.Name);
            this.m_threadPool.QueueUserWorkItem(this.RunJob,
                new JobExecutionInfo(job, JobStartType.Immediate, parameters));
        }

        /// <summary>
        /// Get the specified job instance
        /// </summary>
        public IJob GetJobInstance(Guid jobKey)
        {
            return this.m_jobs.FirstOrDefault(o => o.Job.Id == jobKey)?.Job;
        }

        /// <inheritdoc/>
        public IJob GetJobInstance(Type jobType)
        {
            return this.m_jobs.FirstOrDefault(o => o.Job.GetType() == jobType)?.Job;
        }

        /// <summary>
        /// Start a job
        /// </summary>
        public void StartJob(Type jobType, object[] parameters)
        {
            var job = this.m_jobs.FirstOrDefault(o => o.Job.GetType() == jobType);
            if (job == null)
            {
                // TODO: Audit
                this.m_tracer.TraceInfo("Manually starting job {0}", job.Job.Name);
                this.m_threadPool.QueueUserWorkItem(this.RunJob, new JobExecutionInfo(job.Job, JobStartType.Immediate, parameters));
            }
        }

        /// <summary>
        /// Sets the job's schedule
        /// </summary>
        public IJobSchedule SetJobSchedule(IJob job, DayOfWeek[] daysOfWeek, DateTime scheduleTime)
        {
            var jobInfo = this.m_jobs.FirstOrDefault(o => o.Job.Id == job.Id);
            if (jobInfo == null)
            {
                throw new KeyNotFoundException($"Job {job.Id} not registered");
            }

            this.m_tracer.TraceInfo("Set job {0} schedule to {1} @ {2}", job, daysOfWeek, scheduleTime);
            this.m_jobScheduleManager.Clear(job);
            var retVal = new JobItemSchedule()
            {
                Type = JobScheduleType.Scheduled,
                RepeatOn = daysOfWeek,
                StartDate = scheduleTime
            };
            this.m_jobScheduleManager.Add(job, retVal);
            return retVal;
        }

        /// <summary>
        /// Set the job to repeat on an interval
        /// </summary>
        /// <param name="job">The job to set repeat schedule for</param>
        /// <param name="interval">The interval to set</param>
        /// <returns>The created schedule</returns>
        public IJobSchedule SetJobSchedule(IJob job, TimeSpan interval)
        {
            var jobInfo = this.m_jobs.FirstOrDefault(o => o.Job.Id == job.Id);
            if (jobInfo == null)
            {
                throw new KeyNotFoundException($"Job {job.Id} not registered");
            }

            this.m_tracer.TraceInfo("Set job {0} schedule to repeat {1} ", job, interval);
            this.m_jobScheduleManager.Clear(job);

            var retVal = new JobItemSchedule()
            {
                Type = JobScheduleType.Interval,
                Interval = (int)interval.TotalSeconds,
                IntervalSpecified = true,
            };
            this.m_jobScheduleManager.Add(job, retVal);
            return retVal;
        }

        /// <summary>
        /// Get schedules for the specified job
        /// </summary>
        public IEnumerable<IJobSchedule> GetJobSchedules(IJob job)
        {
            var jobInfo = this.m_jobs.FirstOrDefault(o => o.Job.Id == job.Id);
            if (jobInfo == null)
            {
                throw new KeyNotFoundException($"Job {job.Id} not registered");
            }
            return this.m_jobScheduleManager.Get(job);
        }

        /// <inheritdoc/>
        public bool TryCreateService<TService>(out TService serviceInstance)
        {
            if (this.TryCreateService(typeof(TService), out object tmpService) && tmpService is TService tService)
            {
                serviceInstance = tService;
                return true;
            }
            else
            {
                serviceInstance = default(TService);
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryCreateService(Type serviceType, out object serviceInstance)
        {
            if (typeof(IJobStateManagerService).IsAssignableFrom(serviceType))
            {
                serviceInstance = this.m_serviceManager.CreateInjected<XmlFileJobStateManager>();
                return true;
            }
            else if (typeof(IJobScheduleManager).IsAssignableFrom(serviceType))
            {
                serviceInstance = this.m_serviceManager.CreateInjected<XmlFileJobScheduleManager>();
                return true;
            }
            else
            {
                serviceInstance = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public void ClearJobSchedule(IJob job)
        {
            var jobInfo = this.m_jobs.FirstOrDefault(o => o.Job.Id == job.Id);
            if (jobInfo == null)
            {
                throw new KeyNotFoundException($"Job {job.Id} not registered");
            }

            this.m_tracer.TraceInfo("Clear job {0} schedule.", job);
            this.m_jobScheduleManager.Clear(job);
        }

        #endregion ITimerService Members
    }
}