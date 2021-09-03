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
using System.ComponentModel;
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
        [XmlAttribute("autoLink"), JsonProperty("autoLink"), DisplayName("Auto-Link"), Description("When set to true, instructs the resource persister to merge/link duplicates automatically")]
        public bool AutoLink { get; set; }

        /// <summary>
        /// Match configuration
        /// </summary>
        [XmlText, JsonProperty("name"), DisplayName("Match Configuration Name"), Description("The ID of the match configuration to use")]
        public String MatchConfiguration { get; set; }

        /// <summary>
        /// Represent the object as string
        /// </summary>
        public override string ToString() => $"{this.MatchConfiguration} {(this.AutoLink ? "(Auto-Link)" : "")}";
    }

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
            this.MatchConfiguration = new List<ResourceMergeMatchConfiguration>();
        }

        /// <summary>
        /// MDM resource configuration
        /// </summary>
        public ResourceMergeConfiguration(Type type)
        {
            //this.ResourceTypeXml = type.GetCustomAttribute<XmlRootAttribute>()?.ElementName;
            this.MatchConfiguration = new List<ResourceMergeMatchConfiguration>();
        }
      
        /// <summary>
        /// Gets the reosurce type
        /// </summary>
        [XmlElement("resourceType"), JsonProperty("resourceType"), DisplayName("Resource Type"), Description("Identifies the type of resource to listen for")]
        [Editor("SanteDB.Configuration.Editors.ResourceCollectionEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0")]
        public ResourceTypeReferenceConfiguration ResourceType { get; set; }

        /// <summary>
        /// For legacy purposes only
        /// </summary>
        [Browsable(false), XmlAttribute("type")]
        public String ResourceTypeXml
        {
            get => null;
            set
            {
                this.ResourceType = new ResourceTypeReferenceConfiguration() { TypeXml = value };
            }
        }

        /// <summary>
        /// Gets or sets the match configuration
        /// </summary>
        [XmlArray("matching"), XmlArrayItem("config"), JsonProperty("matching"), DisplayName("Match Configuraiton"), Description("Sets the match configurations to use")]
        public List<ResourceMergeMatchConfiguration> MatchConfiguration { get; set; }

        /// <summary>
        /// When true, preserve the original record
        /// </summary>
        [XmlAttribute("preserveOriginal"), JsonProperty("preserveOriginal"), DisplayName("Preserve Original Links"), Description("When set, instructs the resource manager to preserve original links when it re-writes them")]
        public bool PreserveOriginal { get; set; }

        /// <summary>
        /// Resource for match configuration
        /// </summary>
        /// <returns></returns>
        public override string ToString() => this.ResourceType?.TypeXml;
    }
}
