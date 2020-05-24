/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * User: fyfej (Justin Fyfe)
 * Date: 2020-1-1
 */
using Newtonsoft.Json;
using SanteDB.Core.Model.Serialization;
using System;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents configuration for one resource
    /// </summary>
    [XmlType(nameof(ResourceMergeConfiguration), Namespace = "http://santedb.org/configuration")]
    public class ResourceMergeConfiguration
    {
        /// <summary>
        /// Serialization ctor
        /// </summary>
        public ResourceMergeConfiguration()
        {

        }

        /// <summary>
        /// MDM resource configuration
        /// </summary>
        public ResourceMergeConfiguration(Type type, String matchConfiguration, bool autoMerge)
        {
            this.ResourceTypeXml = type.GetTypeInfo().GetCustomAttribute<XmlRootAttribute>()?.ElementName;
            this.MatchConfiguration = matchConfiguration;
            this.AutoMerge = autoMerge;
        }
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
        /// Gets or sets the match configuration
        /// </summary>
        [XmlAttribute("matchConfiguration"), JsonProperty("matchConfiguration")]
        public String MatchConfiguration { get; set; }

        /// <summary>
        /// Gets the auto merge attribute
        /// </summary>
        [XmlAttribute("autoMerge"), JsonProperty("autoMerge")]
        public bool AutoMerge { get; set; }

        /// <summary>
        /// When true, preserves the original record 
        /// </summary>
        [XmlAttribute("preserveOriginal"), JsonProperty("preserveOriginal")]
        public bool PreserveOriginal { get; set; }
    }
}
