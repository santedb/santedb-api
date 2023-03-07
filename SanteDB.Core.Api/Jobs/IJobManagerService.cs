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
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Job Management Service
    /// </summary>
    /// <remarks>
    /// <para>In SanteDB, developers can create <see cref="IJob"/> implementations which represent background jobs for the system. Uses of these classes involve:</para>
    /// <list type="bullet">
    ///     <item>Performing routine maintenance tasks like compression, backup, etc.</item>
    ///     <item>Performing indexing tasks or managing long-running tasks</item>
    ///     <item>Exposing packaged batch operations to users (who can run them manually from the UI)</item>
    /// </list>
    /// <para>The job manager is the service which manages the master list of <see cref="IJob"/> instances and allows other plugins
    /// to register new jobs, start jobs, and even schedule job execution based on a schedule or interval.</para>
    /// </remarks>
    /// <seealso cref="IJob"/>
    /// <seealso cref="IJobScheduleManager"/>
    [Description("Job Management Service")]
    public interface IJobManagerService : IServiceImplementation
    {

        /// <summary>
        /// Add a job to the job manager
        /// </summary>
        [Obsolete("Use AddJob(IJob, JobStartType) and then SetJobSchedule() instead", true)]
        void AddJob(IJob jobType, TimeSpan elapseTime, JobStartType startType = JobStartType.Immediate);

        /// <summary>
        /// Add a job to the execution manager
        /// </summary>
        /// <param name="jobType">The type of job to add</param>
        /// <param name="startType">The type of start the job should take</param>
        /// <example>
        /// <code language="cs">
        /// <![CDATA[
        ///     var jobManager = ApplicationServiceContext.Current.GetService&lt;IJobManager>();
        ///     jobManager.AddJob(new HelloWorldJob(), startType = JobStartType.Never); 
        /// ]]>
        /// </code>
        /// </example>
        void AddJob(IJob jobType, JobStartType startType = JobStartType.Immediate);

        /// <summary>
        /// Returns true if the job is registered
        /// </summary>
        /// <param name="jobType">The type of job to check for</param>
        bool IsJobRegistered(Type jobType);

        /// <summary>
        /// Gets the status of all jobs
        /// </summary>
        IEnumerable<IJob> Jobs { get; }

        /// <summary>
        /// Starts the specified <paramref name="job"/>
        /// </summary>
        /// <param name="job">The job instance to start</param>
        /// <param name="parameters">The parameters to pass to the job</param>
        void StartJob(IJob job, object[] parameters);

        /// <summary>
        /// Start a job by registered type
        /// </summary>
        /// <param name="jobType">The type of job to start</param>
        /// <param name="parameters">The parameters to pass to the job</param>
        void StartJob(Type jobType, object[] parameters);

        /// <summary>
        /// Get this manager's instance of a job
        /// </summary>
        /// <param name="jobKey">The job type to fetch</param>
        /// <returns>The job instance that matches the job key</returns>
        IJob GetJobInstance(Guid jobKey);

        /// <summary>
        /// Get the schedule for the specified job
        /// </summary>
        /// <param name="job">The job to gather schedules for</param>
        /// <returns>The schedules registered for the job</returns>
        IEnumerable<IJobSchedule> GetJobSchedules(IJob job);

        /// <summary>
        /// Schedule a job to start at a specific time with a specific repetition
        /// </summary>
        /// <param name="job">The job to set the schedule for</param>
        /// <param name="daysOfWeek">The days of the week that the job should start on</param>
        /// <param name="scheduleTime">The start date of the schedule (note: if <paramref name="daysOfWeek"/> is empty then this is the only run time, otherwise the schedule starts on the <c>Date</c> in the date time at the <c>Time</c> on the date time every day of week)</param>
        /// <returns>The job schedule that has been set</returns>
        IJobSchedule SetJobSchedule(IJob job, DayOfWeek[] daysOfWeek, DateTime scheduleTime);

        /// <summary>
        /// Schedule a job to repeat on an interval
        /// </summary>
        /// <param name="job">The job which to set the interval on</param>
        /// <param name="intervalSpan">The repeat interval of the job</param>
        /// <returns>The schedule of the job</returns>
        IJobSchedule SetJobSchedule(IJob job, TimeSpan intervalSpan);
        /// <summary>
        /// Clear the schedule of a job.
        /// </summary>
        /// <param name="job">The job to clear the schedule for.</param>
        void ClearJobSchedule(IJob job);

    }
}
