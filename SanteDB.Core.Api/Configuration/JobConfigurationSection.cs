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
using Newtonsoft.Json;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a simple job configuration
    /// </summary>
    [XmlType(nameof(JobConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class JobConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Add job
        /// </summary>
        [XmlArray("jobs"), XmlArrayItem("add"), DisplayName("Timer Jobs"), Description("Identifies jobs which should be run on a scheduled timer")]
        public List<JobItemConfiguration> Jobs { get; set; }

    }

    /// <summary>
    /// Represents the configuration of a single job
    /// </summary>
    [XmlType(nameof(JobItemConfiguration), Namespace = "http://santedb.org/configuration")]
    public class JobItemConfiguration
    {

        /// <summary>
        /// Creates a new job item configuration
        /// </summary>
        public JobItemConfiguration()
        {
            this.Schedule = new List<JobItemSchedule>();
        }

        /// <summary>
        /// The type as expressed in XML
        /// </summary>
        [XmlAttribute("type"), Browsable(false)]
        public String TypeXml { get; set; }

        /// <summary>
        /// Gets or sets the job type
        /// </summary>
        [XmlIgnore, JsonIgnore, Editor("SanteDB.Configuration.Editors.TypeSelectorEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing"),
           TypeConverter("SanteDB.Configuration.Converters.TypeDisplayConverter, SanteDB.Configuration"), BindingAttribute(typeof(IJob))]
        [DisplayName("Job Type"), Description("The type of job to run on the specified schedule")]
        public Type Type
        {
            get => !String.IsNullOrEmpty(this.TypeXml) ? Type.GetType(this.TypeXml) : null;
            set => this.TypeXml = value?.AssemblyQualifiedName;
        }

        /// <summary>
        /// Job startup type
        /// </summary>
        [XmlAttribute("startType"), JsonProperty("startType"), DisplayName("Start Type"), Description("Sets the startup process for the job")]
        public JobStartType StartType { get; set; }

        /// <summary>
        /// The schedule for this job
        /// </summary>
        [XmlArray("schedule"),
            XmlArrayItem("add"),
            JsonProperty("schedule"),
            DisplayName("Schedule"),
            Description("The schedule for this job")]
        public List<JobItemSchedule> Schedule { get; set; }

        /// <summary>
        /// Parameters for this job
        /// </summary>
        [XmlArray("parameters"),
            XmlArrayItem("int", typeof(Int32)),
            XmlArrayItem("string", typeof(string)),
            XmlArrayItem("bool", typeof(bool)),
            JsonProperty("parameters"), DisplayName("Job Parameters"), Description("If the job requires special parameters to control its execution, these are included here")]
        public object[] Parameters { get; set; }
    }

    /// <summary>
    /// Job item schedule
    /// </summary>
    [XmlType(nameof(JobItemSchedule), Namespace = "http://santedb.org/configuration")]
    public class JobItemSchedule
    {

        /// <summary>
        /// The days on which this schedule applies
        /// </summary>
        [XmlAttribute("days"), JsonProperty("days"), DisplayName("Repeat"), Description("The days of week you want this job repeating")]
        public DayOfWeek[] RepeatOn { get; set; }

        /// <summary>
        /// Gets or sets the start date
        /// </summary>
        [XmlAttribute("start"), JsonProperty("start"), DisplayName("Start On"), Description("The first time you want this job to run")]
        [Editor("SanteDB.Configuration.Editors.DateTimePickerEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Stop date
        /// </summary>
        [XmlAttribute("stop"), JsonProperty("stop"), DisplayName("Stop On"), Description("The last time you want this job to run")]
        [Editor("SanteDB.Configuration.Editors.DateTimePickerEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public DateTime StopDate { get; set; }

        /// <summary>
        /// Stop date was specified
        /// </summary>
        [DisplayName("Stop This Event"), Description("Set to true if you want the stop date to be enforced"), XmlIgnore, JsonIgnore]
        public bool StopDateSpecified { get; set; }

        /// <summary>
        /// The interval of the job
        /// </summary>
        [DisplayName("Interval"), Description("The interval you want this job to run at (if you want it run every X seconds"), XmlAttribute("interval"), JsonProperty("interval")]
        public int Interval { get; set; }

        /// <summary>
        /// The inteval was specified
        /// </summary>
        [XmlIgnore, JsonIgnore, DisplayName("Use Interval"), Description("When set to true, the job should be run on a regular interval rather than a schedule")]
        public bool IntervalSpecified { get; set; }


        /// <summary>
        /// Returns true if the schedule applies to <paramref name="refDate"/>
        /// </summary>
        internal bool AppliesTo(DateTime refDate, DateTime? lastRun)
        {
            var retVal = refDate >= this.StartDate; // The reference date is in valid bounds for start
            retVal &= !this.StopDateSpecified || refDate < this.StopDate; // The reference date is in valid bounds of stop (if specified)

            // Are there week days specified
            if (this.IntervalSpecified && (!lastRun.HasValue || refDate.Subtract(lastRun.Value).TotalMilliseconds > this.Interval))
            {
                return true;
            }
            else if (this.RepeatOn != null)
            {
                retVal &= this.RepeatOn.Any(r => r == refDate.DayOfWeek) &&
                    refDate.Hour >= this.StartDate.Hour &&
                    refDate.Minute >= this.StartDate.Minute;
                retVal &= !lastRun.HasValue || (lastRun.Value.Date <= refDate.Date); // Last run does not cover this calculation - i.e. have we not already run this repeat?
            }
            else // This is an exact time
            {
                retVal &= refDate.Date == this.StartDate.Date &&
                    refDate.Hour >= this.StartDate.Hour &&
                    refDate.Minute >= this.StartDate.Minute &&
                    !lastRun.HasValue;
            }

            return retVal;
        }

        /// <summary>
        /// Represent the schedule as string
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (this.IntervalSpecified)
            {
                sb.AppendFormat("Every {0} ms", this.Interval);
            }
            else if (this.RepeatOn?.Any() == true)
            {
                sb.AppendFormat("Every {0} at {1:HH:mm} starting {1:yyyy-MM-dd}", String.Join(",", this.RepeatOn), this.StartDate);
            }
            else
            {
                sb.AppendFormat("On {0:yyyy-MM-dd} at {0:HH:mm}", this.StartDate);
            }

            if (this.StopDateSpecified)
            {
                sb.AppendFormat(" until {0:yyyy-MM-dd} at {0:HH:mm}", this.StopDate);
            }

            return sb.ToString();
        }
    }
}
