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
using SanteDB.Core.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// A job manager which uses a file-based "cron" configuration file
    /// </summary>
    public class XmlFileJobScheduleManager : IJobScheduleManager
    {

        // Serializer
        private readonly XmlSerializer m_xsz = new XmlSerializer(typeof(List<JobItemConfiguration>));

        // Lock
        private object m_lock = new object();

        // CronTab Location
        private readonly string m_cronTabLocation;

        // Job item schedules
        private List<JobItemConfiguration> m_jobSchedules = new List<JobItemConfiguration>();

        /// <summary>
        /// Initialize the job schedule manager
        /// </summary>
        public XmlFileJobScheduleManager()
        {
            this.m_cronTabLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "xcron.xml");
            if (File.Exists(this.m_cronTabLocation))
            {
                try
                {
                    using (var fs = File.OpenRead(this.m_cronTabLocation))
                    {
                        this.m_jobSchedules = this.m_xsz.Deserialize(fs) as List<JobItemConfiguration>;
                    }
                }
                catch
                {
                    this.m_jobSchedules = new List<JobItemConfiguration>();
                }
            }
            else
            {
                this.m_jobSchedules = new List<JobItemConfiguration>();
            }
        }

        /// <summary>
        /// Save the cron file
        /// </summary>
        private void SaveCron()
        {
            try
            {
                using (var fs = File.Create(this.m_cronTabLocation))
                {
                    this.m_xsz.Serialize(fs, this.m_jobSchedules);
                }
            }
            catch
            {

            }
        }

        /// <inheritdoc/>
        public void Add(IJob job, IJobSchedule jobSchedule)
        {
            lock (this.m_lock)
            {
                var scheduleReg = this.m_jobSchedules.Find(o => o.Type == job.GetType());
                if (scheduleReg == null)
                {
                    scheduleReg = new JobItemConfiguration() { Type = job.GetType(), Schedule = new List<JobItemSchedule>() };
                    this.m_jobSchedules.Add(scheduleReg);
                }
                scheduleReg.Schedule.Add(new JobItemSchedule()
                {
                    Type = jobSchedule.Type,
                    Interval = (int)jobSchedule.Interval.GetValueOrDefault().TotalSeconds,
                    IntervalSpecified = jobSchedule.Interval.HasValue,
                    RepeatOn = jobSchedule.Days,
                    StartDate = jobSchedule.StartTime,
                    StopDate = jobSchedule.StopTime.GetValueOrDefault(),
                    StopDateSpecified = jobSchedule.StopTime.HasValue
                });

                this.SaveCron();
            }
        }

        /// <summary>
        /// Clear the schedule for the specified job
        /// </summary>
        public void Clear(IJob job)
        {
            lock (this.m_lock)
            {
                var scheduleReg = this.m_jobSchedules.Find(o => o.Type == job.GetType());
                if (scheduleReg != null)
                {
                    this.m_jobSchedules.Remove(scheduleReg);
                    this.SaveCron();
                }
            }
        }

        /// <summary>
        /// Get all registered jobs
        /// </summary>
        public IEnumerable<IJobSchedule> Get(IJob job)
        {
            lock (this.m_lock)
            {
                return this.m_jobSchedules.Find(o => o.Type == job.GetType())?.Schedule;
            }
        }
    }
}