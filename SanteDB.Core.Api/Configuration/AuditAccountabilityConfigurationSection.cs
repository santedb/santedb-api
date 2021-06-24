/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool CompleteAuditTrail { get; set; }

        /// <summary>
        /// Gets or sets filters to apply to the audit trail (i.e. ignore)
        /// </summary>
        [XmlArray("filters"), XmlArrayItem("add"), JsonProperty("filters")]
        public List<AuditFilterConfiguration> AuditFilters { get; set; }

        /// <summary>
        /// Audit source identification
        /// </summary>
        [XmlElement("auditSource"), JsonProperty("sourceIdentification")]
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
        public string EnterpriseSite { get; set; }

        /// <summary>
        /// The key of the device
        /// </summary>
        [XmlElement("enterpriseSiteKey"), JsonProperty("enterpriseSiteKey")]
        public Guid EnterpriseDeviceKey { get; set; }

        /// <summary>
        /// The location
        /// </summary>
        [XmlElement("siteName"), JsonProperty("siteName")]
        public string SiteLocation { get; set; }

    }
}
