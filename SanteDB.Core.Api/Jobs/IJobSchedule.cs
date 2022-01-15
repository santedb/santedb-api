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
using System;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// Job schedule - defines the 
    /// </summary>
    public interface IJobSchedule
    {
        /// <summary>
        /// Get the interval on which the job runs
        /// </summary>
        TimeSpan? Interval { get; }

        /// <summary>
        /// Get the start or termination time
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        /// Get the stop or termination time
        /// </summary>
        DateTime? StopTime { get; }

        /// <summary>
        /// Gets the days that the schedule runs
        /// </summary>
        DayOfWeek[] Days { get; }

        /// <summary>
        /// Returns true if the job schedule applies at <paramref name="checkTime"/> given the <paramref name="lastExecutionTime"/>
        /// </summary>
        /// <param name="checkTime">The time that the system is checking if the job execution applies</param>
        /// <param name="lastExecutionTime">The last known run time / check time of the job. Null if never run</param>
        /// <returns>True if the schedule applies</returns>
        bool AppliesTo(DateTime checkTime, DateTime? lastExecutionTime);
    }
}