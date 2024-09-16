using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Configuration for the care pathway configuration section
    /// </summary>
    [XmlType(nameof(CarePathwayConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class CarePathwayConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// When true enables automatic enrolment
        /// </summary>
        [XmlAttribute("autoEnrollment"), JsonProperty("autoEnrollment")]
        [DisplayName("Enabled Auto Enrolment")]
        [Description("When enabled, care pathways with automatic enrolment will be automatically enroll patients matching the eligibility criteria")]
        public bool EnableAutoEnrollment { get; set; }
    }
}
