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
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a generic resource type reference by simple name
    /// </summary>
    [XmlType(nameof(ResourceTypeReferenceConfiguration), Namespace = "http://santedb.org/configuration")]
    public class ResourceTypeReferenceConfiguration 
    {

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
        /// Gets as a string
        /// </summary>
        public override string ToString() => this.ResourceTypeXml;
    }
}