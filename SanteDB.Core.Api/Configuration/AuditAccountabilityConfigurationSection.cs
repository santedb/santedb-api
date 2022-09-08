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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Confiugration related to auditing and accountability
    /// </summary>
    [XmlType(nameof(AuditAccountabilityConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class AuditAccountabilityConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// When set to true, enables complete audit trail
        /// </summary>
        [XmlAttribute("completeAuditTrail"), JsonProperty("completeAuditTrail")]
        [DisplayName("Complete Audit Trail"), Description("When set to true, instructs the SanteDB service to process all audits (include verbose audits)")]
        public bool CompleteAuditTrail { get; set; }

        /// <summary>
        /// Gets or sets filters to apply to the audit trail (i.e. ignore)
        /// </summary>
        [XmlArray("filters"), XmlArrayItem("add"), JsonProperty("filters")]
        [DisplayName("Event Filters"), Description("Sets one or more filters which control how the SanteDB audits are stored and/or shipped upstream")]
        public List<AuditFilterConfiguration> AuditFilters { get; set; }

        /// <summary>
        /// Audit source identification
        /// </summary>
        [XmlElement("auditSource"), JsonProperty("sourceIdentification")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("Source Identification"), Description("Sets the audit source identification for this node")]
        public AuditSourceConfiguration SourceInformation { get; set; }
    }

    /// <summary>
    /// Audit source configuration
    /// </summary>
    [XmlType(nameof(AuditSourceConfiguration), Namespace = "http://santedb.org/configuration")]
    public class AuditSourceConfiguration
    {

        /// <summary>
        /// Gets or sets the enterprise site
        /// </summary>
        [XmlElement("enterpriseSite"), JsonProperty("enterpriseSite")]
        [DisplayName("Enterprise Site"), Description("The name of the site (for example: GOOD HEALTH HOSPITAL MPI)")]
        public string EnterpriseSite { get; set; }

        /// <summary>
        /// The key of the device
        /// </summary>
        [XmlElement("enterpriseSiteKey"), JsonProperty("enterpriseSiteKey"), Browsable(false)]
        public Guid EnterpriseDeviceKey { get; set; }

        /// <summary>
        /// The location
        /// </summary>
        [XmlElement("siteName"), JsonProperty("siteName")]
        [DisplayName("Site Location"), Description("The location of the site (for example: WEST 4TH STREET FACILITY)")]
        public string SiteLocation { get; set; }

    }
}
