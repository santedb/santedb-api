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
 * Date: 2021-11-5
 */
using SanteDB.Core.Interfaces;
using SanteDB.Core.Jobs;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core
{
    /// <summary>
    /// Extension methods for the core API functions
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create injected class
        /// </summary>
        public static object CreateInjected(this Type me)
        {
            return ApplicationServiceContext.Current.GetService<IServiceManager>().CreateInjected(me);
        }

        /// <summary>
        /// Returns true if the job schedule applies at <paramref name="refDate"/> given the <paramref name="lastRun"/>
        /// </summary>
        /// <param name="me">The job schedule to determine applicability</param>
        /// <param name="refDate">The time that the system is checking if the job execution applies</param>
        /// <param name="lastRun">The last known run time / check time of the job. Null if never run</param>
        /// <returns>True if the schedule applies</returns>
        public static bool AppliesTo(this IJobSchedule me, DateTime refDate, DateTime? lastRun)
        {
            var retVal = refDate >= me.StartTime; // The reference date is in valid bounds for start
            retVal &= !me.StopTime.HasValue || refDate < me.StopTime.Value; // The reference date is in valid bounds of stop (if specified)

            // Are there week days specified
            if (me.Type == Configuration.JobScheduleType.Interval && (!lastRun.HasValue || refDate.Subtract(lastRun.Value) > me.Interval))
            {
                return true;
            }
            else if (me.Type == Configuration.JobScheduleType.Scheduled)
            {
                if (me.Days != null && me.Days.Any())
                {
                    retVal &= me.Days.Any(r => r == refDate.DayOfWeek) &&
                        refDate.Hour >= me.StartTime.Hour &&
                        refDate.Minute >= me.StartTime.Minute &&
                        refDate.Date > me.StartTime;
                    retVal &= !lastRun.HasValue ? DateTime.Now.Hour == me.StartTime.Hour : (lastRun.Value.Date < refDate.Date); // Last run does not cover this calculation - i.e. have we not already run this repeat?
                }
                else // This is an exact time
                {
                    retVal &= refDate.Date == me.StartTime.Date &&
                        refDate.Hour >= me.StartTime.Hour &&
                        refDate.Minute >= me.StartTime.Minute &&
                        !lastRun.HasValue;
                }
            }

            return retVal;

        }

        /// <summary>
        /// Determine if the <see cref="IJobState"/> is running
        /// </summary>
        /// <param name="me">The job state structure</param>
        /// <returns>True if the status of the job state implies the job is running</returns>
        public static bool IsRunning(this IJobState me) => me.CurrentState == JobStateType.Running || me.CurrentState == JobStateType.Starting;

    }
}