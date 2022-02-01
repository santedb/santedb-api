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
 * Date: 2021-8-27
 */
using Newtonsoft.Json;
using SanteDB.Core.Model.Serialization;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a generic resource type reference by simple name
    /// </summary>
    [XmlType(nameof(ResourceTypeReferenceConfiguration), Namespace = "http://santedb.org/configuration")]
    public class ResourceTypeReferenceConfiguration
    {

        // Binder instance
        private static ModelSerializationBinder s_binder = new ModelSerializationBinder();

        /// <summary>
        /// Default ctor for serialization
        /// </summary>
        public ResourceTypeReferenceConfiguration()
        {

        }

        /// <summary>
        /// Constructor accepting the resource type
        /// </summary>
        /// <param name="resourceType">The resource type being set on this configuration object</param>
        public ResourceTypeReferenceConfiguration(Type resourceType)
        {
            s_binder.BindToName(resourceType, out var asm, out var name);
            this.TypeXml = name;
        }

        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public String TypeXml { get; set; }

        /// <summary>
        /// Gets the resource
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type Type => s_binder.BindToType(null, this.TypeXml);

        /// <summary>
        /// Gets as a string
        /// </summary>
        public override string ToString() => this.TypeXml;
    }
}