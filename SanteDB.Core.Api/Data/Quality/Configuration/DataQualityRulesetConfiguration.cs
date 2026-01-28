/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Model;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Represents a data quality rule set
    /// </summary>
    [XmlType(nameof(DataQualityRulesetConfiguration), Namespace = "http://santedb.org/configuration")]
    [XmlRoot(nameof(DataQualityRulesetConfiguration), Namespace = "http://santedb.org/configuration")]
    public class DataQualityRulesetConfiguration : NonVersionedEntityData
    {
        private static XmlSerializer m_xsz = new XmlSerializer(typeof(DataQualityRulesetConfiguration));

        /// <summary>
        /// Gets or sets whether the rule set is enabled
        /// </summary>
        [XmlAttribute("enabled"), JsonProperty("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the rule set
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the rule set
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Adds the specified resources
        /// </summary>
        [XmlArray("resources"), XmlArrayItem("add"), JsonProperty("resources")]
        public List<DataQualityResourceConfiguration> Resources { get; set; }

        /// <summary>
        /// Save this object to <paramref name="stream"/>
        /// </summary>
        public void Save(Stream stream)
        {
            m_xsz.Serialize(stream, this);
        }

        /// <summary>
        /// Load an object from <paramref name="stream"/>
        /// </summary>
        public static DataQualityRulesetConfiguration Load(Stream stream)
        {
            return m_xsz.Deserialize(stream) as DataQualityRulesetConfiguration;
        }
    }
}