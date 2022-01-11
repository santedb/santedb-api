using System;
using System.Collections.Generic;
using System.Text;

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
        /// <returns>The cleared job schedule</returns>
        IEnumerable<IJobSchedule> Clear(IJob job);

        /// <summary>
        /// Add <paramref name="jobSchedule"/> to <paramref name="job"/>
        /// </summary>
        /// <param name="job">The job to add the schedule to</param>
        /// <param name="jobSchedule">The schedule to set</param>
        void Add(IJob job, IJobSchedule jobSchedule);
    }
}
