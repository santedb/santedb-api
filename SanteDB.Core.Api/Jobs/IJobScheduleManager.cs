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
 * Date: 2023-3-10
 */
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// A job scheduler class
    /// </summary>
    public interface IJobScheduleManager
    {

        /// <summary>
        /// Get all registered schedules for job <paramref name="job"/>
        /// </summary>
        /// <param name="job">The id of the job to fetch schedules for</param>
        /// <returns>The registered job schedules</returns>
        IEnumerable<IJobSchedule> Get(IJob job);

        /// <summary>
        /// Clear all schedules for <paramref name="job"/>
        /// </summary>
        /// <param name="job">The job to clear schedules for</param>
        void Clear(IJob job);

        /// <summary>
        /// Add <paramref name="jobSchedule"/> to <paramref name="job"/>
        /// </summary>
        /// <param name="job">The job to add the schedule to</param>
        /// <param name="jobSchedule">The schedule to set</param>
        IJobSchedule Add(IJob job, IJobSchedule jobSchedule);

        /// <summary>
        /// Add a schedule which repeats at <paramref name="interval"/> on <paramref name="job"/>
        /// </summary>
        /// <param name="job">The job to add the schedule to</param>
        /// <param name="interval">The interval to fire the job</param>
        /// <param name="stopDate">The stop date</param>
        IJobSchedule Add(IJob job, TimeSpan interval, DateTime? stopDate = null);

        /// <summary>
        /// Add a job schedule to <paramref name="job"/> that repeats on <paramref name="repeatOn"/> starting <paramref name="startDate"/>
        /// </summary>
        /// <param name="job">The job to add the schedule to</param>
        /// <param name="repeatOn">Repeat on days of week</param>
        /// <param name="startDate">The start date and time of the schedule</param>
        /// <param name="stopDate">The date to stop the schedule</param>
        IJobSchedule Add(IJob job, DayOfWeek[] repeatOn, DateTime startDate, DateTime? stopDate = null);
    }
}
