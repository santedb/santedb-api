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
using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{

    /// <summary>
    /// Resource merge match configuration
    /// </summary>
    [XmlType(nameof(ResourceMergeMatchConfiguration), Namespace = "http://santedb.org/configuration")]
    public class ResourceMergeMatchConfiguration
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public ResourceMergeMatchConfiguration()
        {
        }

        /// <summary>
        /// Create a new match configuration
        /// </summary>
        public ResourceMergeMatchConfiguration(String configurationName, bool autoMerge)
        {
            this.MatchConfiguration = configurationName;
            this.AutoLink = autoMerge;
        }

        /// <summary>
        /// Automerge
        /// </summary>
        [XmlAttribute("autoLink"), JsonProperty("autoLink")]
        public bool AutoLink { get; set; }

        /// <summary>
        /// Match configuration
        /// </summary>
        [XmlText, JsonProperty("name")]
        public String MatchConfiguration { get; set; }
    }

    /// <summary>
    /// Represents configuration for one resource
    /// </summary>
    [XmlType(nameof(ResourceMergeConfiguration), Namespace = "http://santedb.org/configuration")]
    public class ResourceMergeConfiguration : ResourceTypeReferenceConfiguration
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
        public ResourceMergeConfiguration(Type type)
        {
            this.ResourceTypeXml = type.GetCustomAttribute<XmlRootAttribute>()?.ElementName;
            this.MatchConfiguration = new List<ResourceMergeMatchConfiguration>();
        }
      
        /// <summary>
        /// Gets or sets the match configuration
        /// </summary>
        [XmlArray("matching"), XmlArrayItem("config"), JsonProperty("matching")]
        public List<ResourceMergeMatchConfiguration> MatchConfiguration { get; set; }

        /// <summary>
        /// When true, automatically perform the merge 
        /// </summary>
        [XmlAttribute("autoMerge"), JsonProperty("autoMerge")]
        public bool AutoMerge { get; set; }

        /// <summary>
        /// When true, preserve the original record
        /// </summary>
        [XmlAttribute("preserveOriginal"), JsonProperty("preserveOriginal")]
        public bool PreserveOriginal { get; set; }
    }
}
