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
using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Configuration Section for configuring the retention policies
    /// </summary>
    [XmlType(nameof(DataRetentionConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DataRetentionConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Data retention rules
        /// </summary>
        [XmlArray("rules"), XmlArrayItem("add"), JsonProperty("rules")]
        public List<DataRetentionRuleConfiguration> RetentionRules { get; set; }

    }

    /// <summary>
    /// Identifies the action to take when the retained object is set
    /// </summary>
    public enum DataRetentionActionType
    {
        /// <summary>
        /// The object should be purged (deleted from the database)
        /// </summary>
        [XmlEnum("purge")]
        Purge = 0x1,
        /// <summary>
        /// The object should be obsoleted in the persistence layer
        /// </summary>
        [XmlEnum("obsolete")]
        Obsolete = 0x2,
        /// <summary>
        /// The object should be archived using the IDataArchiveService
        /// </summary>
        [XmlEnum("archive")]
        Archive = 0x4
    }

    /// <summary>
    /// Retention rule configuration
    /// </summary>
    [XmlType(nameof(DataRetentionRuleConfiguration), Namespace = "http://santedb.org/configuration")]
    public class DataRetentionRuleConfiguration
    {

        /// <summary>
        /// Gets the name of the rule
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public String Name { get; set; }
        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public String ResourceTypeXml { get; set; }

        /// <summary>
        /// Gets the resource
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type ResourceType => new ModelSerializationBinder().BindToType(null, this.ResourceTypeXml);

        /// <summary>
        /// Gets or sets the filter expressions the rule applies (i.e. objects matching this rule will be included)
        /// </summary>
        [XmlArray("includes"), XmlArrayItem("filter"), JsonProperty("includes")]
        public List<String> IncludeExpressions { get; set; }

        /// <summary>
        /// Gets or sets the objects which are excluded.
        /// </summary>
        [XmlArray("excludes"), XmlArrayItem("filter"), JsonProperty("excludes")]
        public List<String> ExcludeExpressions { get; set; }

        /// <summary>
        /// Dictates the action
        /// </summary>
        [XmlAttribute("action"), JsonProperty("action")]
        public DataRetentionActionType Action { get; set; }

    }
}
