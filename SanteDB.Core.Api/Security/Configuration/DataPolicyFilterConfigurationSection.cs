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
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Security.Configuration
{
    /// <summary>
    /// Data policy filter configuration
    /// </summary>
    [XmlType(nameof(DataPolicyFilterConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DataPolicyFilterConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Gets or sets the default action
        /// </summary>
        [XmlAttribute("action"), JsonProperty("action")]
        public ResourceDataPolicyActionType DefaultAction { get; set; }

        /// <summary>
        /// Gets the list of resources
        /// </summary>
        [XmlArray("resources"), XmlArrayItem("add"), JsonProperty("resources")]
        public List<ResourceDataPolicyFilter> Resources { get; set; }

    }

    /// <summary>
    /// Resource filter policy configuration element
    /// </summary>
    [XmlType(nameof(ResourceDataPolicyFilter), Namespace = "http://santedb.org/configuration")]

    public class ResourceDataPolicyFilter : ResourceTypeReferenceConfiguration
    {

        /// <summary>
        /// Gets or sets the action
        /// </summary>
        [XmlAttribute("action")]
        public ResourceDataPolicyActionType Action { get; set; }
    }

    /// <summary>
    /// The resource data policy action
    /// </summary>
    [XmlType(nameof(ResourceDataPolicyActionType), Namespace = "http://santedb.org/configuration"), Flags]
    public enum ResourceDataPolicyActionType
    {
        /// <summary>
        /// None - Take no action
        /// </summary>
        [XmlEnum("none")]
        None = 0x0,
        /// <summary>
        /// Only audit that the resource was disclosed and allow disclosure
        /// </summary>
        [XmlEnum("audit")]
        Audit = 0x1,
        /// <summary>
        /// Disclose the record but mask the populated properties
        /// </summary>
        [XmlEnum("redact")]
        Redact = 0x2,
        /// <summary>
        /// Disclose the record type and key, but clear all data
        /// </summary>
        [XmlEnum("nullify")]
        Nullify = 0x4,
        /// <summary>
        /// Hide the record - return nothing
        /// </summary>
        [XmlEnum("hide")]
        Hide = 0x8,
        /// <summary>
        /// Generate an error condition
        /// </summary>
        [XmlEnum("error")]
        Error = 0x10,
        /// <summary>
        /// Hashes values so they can be compared with a normal value
        /// </summary>
        [XmlEnum("hash")]
        Hash = 0x20
    }
}
