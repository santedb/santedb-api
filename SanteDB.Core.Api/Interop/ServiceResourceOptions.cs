/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-6-21
 */
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

#pragma warning disable CS1591
namespace SanteDB.Core.Interop
{

    /// <summary>
    /// Service resource operations
    /// </summary>
    [XmlType(nameof(ResourceCapability), Namespace = "http://santedb.org/model"), Flags]
    public enum ResourceCapability
    {
        [XmlEnum("none")]
        None = 0x00,
        [XmlEnum("create")]
        Create = 0x001,
        [XmlEnum("create-update")]
        CreateOrUpdate = 0x002,
        [XmlEnum("update")]
        Update = 0x004,
        [XmlEnum("delete")]
        Delete = 0x008,
        [XmlEnum("patch")]
        Patch = 0x010,
        [XmlEnum("get")]
        Get = 0x020,
        [XmlEnum("get-version")]
        GetVersion = 0x040,
        [XmlEnum("history")]
        History = 0x080,
        [XmlEnum("search")]
        Search = 0x100
    }

    /// <summary>
    /// Service resource options
    /// </summary>
    [XmlType(nameof(ServiceResourceOptions), Namespace = "http://santedb.org/model"), JsonObject(nameof(ServiceResourceOptions))]
    public class ServiceResourceOptions
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOptions"/> class.
        /// </summary>
        public ServiceResourceOptions()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResourceOptions"/> class
        /// with a specific resource name, and verbs.
        /// </summary>
        /// <param name="resourceName">The name of the resource of the service resource options.</param>
        /// <param name="operations">The list of HTTP verbs of the resource option.</param>
        public ServiceResourceOptions(string resourceName, ResourceCapability operations)
        {
            this.ResourceName = resourceName;
            this.Capabilities = operations;
        }

        /// <summary>
        /// Gets the name of the resource
        /// </summary>
        [XmlAttribute("resource"), JsonProperty("resource")]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the operations supported by this resource
        /// </summary>
        [XmlAttribute("cap"), JsonProperty("cap")]
        public ResourceCapability Capabilities { get; set; }
    }
}
#pragma warning restore CS1591
