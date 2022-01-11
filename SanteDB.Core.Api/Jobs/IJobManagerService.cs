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
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// Type of job startup
    /// </summary>
    [XmlType(nameof(JobStartType), Namespace = "http://santedb.org/configuration")]
    public enum JobStartType
    {
        /// <summary>
        /// Start job as soon as it is added
        /// </summary>
        [XmlEnum("immediate")]
        Immediate,
        /// <summary>
        /// Start job on a delay
        /// </summary>
        [XmlEnum("delay")]
        DelayStart,
        /// <summary>
        /// Start job on schedule only
        /// </summary>
        [XmlEnum("schedule")]
        TimerOnly,
        /// <summary>
        /// Do not start job
        /// </summary>
        [XmlEnum("never")]
        Never
    }

    /// <summary>
    /// Job manager service
    /// </summary>
    public interface IJobManagerService : IDaemonService
    {

        /// <summary>
        /// Add a job
        /// </summary>
        [Obsolete("Use AddJob(IJob, JobStartType) and then SetJobSchedule() instead", true)]
        void AddJob(IJob jobType, TimeSpan elapseTime, JobStartType startType = JobStartType.Immediate);

        /// <summary>
        /// Add a job to the execution manager
        /// </summary>
        /// <param name="jobType">The type of job to add</param>
        /// <param name="startType">The type of start the job should take</param>
        void AddJob(IJob jobType, JobStartType startType = JobStartType.Immediate);

        /// <summary>
        /// Returns true if the job is registered
        /// </summary>
        bool IsJobRegistered(Type jobType);

        /// <summary>
        /// Gets the status of all jobs
        /// </summary>
        IEnumerable<IJob> Jobs { get; }

        /// <summary>
        /// Start a job
        /// </summary>
        /// <param name="job">The job to start</param>
        /// <param name="parameters">The parameters to pass to the job</param>
        /// <returns>True if the job started successfully</returns>
        void StartJob(IJob job, object[] parameters);

        /// <summary>
        /// Start a job
        /// </summary>
        /// <param name="jobType">The job to start</param>
        /// <param name="parameters">The parameters to pass to the job</param>
        /// <returns>True if the job started successfully</returns>
        void StartJob(Type jobType, object[] parameters);

        /// <summary>
        /// Get this manager's instance of a job
        /// </summary>
        /// <param name="jobKey">The job type to fetch</param>
        IJob GetJobInstance(Guid jobKey);

        /// <summary>
        /// Get the schedule for the specified job
        /// </summary>
        IEnumerable<IJobSchedule> GetJobSchedules(IJob job);

        /// <summary>
        /// Schedule a job to start at a specific time
        /// </summary>
        IJobSchedule SetJobSchedule(IJob job, DayOfWeek[] daysOfWeek, DateTime scheduleTime);

        /// <summary>
        /// Schedule a job to repeat on an interval
        /// </summary>
        IJobSchedule SetJobSchedule(IJob job, TimeSpan intervalSpan);

    }
}
