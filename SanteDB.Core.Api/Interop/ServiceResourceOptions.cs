/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-8-8
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

#pragma warning disable CS1591
namespace SanteDB.Core.Interop
{

    /// <summary>
    /// Service resource operations
    /// </summary>
    [XmlType(nameof(ResourceCapabilityType), Namespace = "http://santedb.org/model"), Flags]
    public enum ResourceCapabilityType
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
    [XmlType(nameof(ServiceResourceCapability), Namespace = "http://santedb.org/model"), JsonObject(nameof(ServiceResourceCapability))]
    public class ServiceResourceCapability
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public ServiceResourceCapability()
        {

        }

        /// <summary>
        /// Creates a new resource demand
        /// </summary>
        /// <param name="capability"></param>
        /// <param name="demand"></param>
        public ServiceResourceCapability(ResourceCapabilityType capability, String[] demand)
        {
            this.Capability = capability;
            this.Demand = demand;
        }

        /// <summary>
        /// Gets or sets the capabilities
        /// </summary>
        [XmlAttribute("cap"), JsonProperty("cap")]
        public ResourceCapabilityType Capability { get; set; }

        /// <summary>
        /// Gets or sets the demand
        /// </summary>
        [XmlElement("demand"), JsonProperty("demand")]
        public String[] Demand { get; set; }
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
        /// <param name="resourceType">The type of resource which options are being fetched for</param>
        public ServiceResourceOptions(string resourceName, Type resourceType, List<ServiceResourceCapability> operations)
        {
            this.ResourceName = resourceName;
            this.Capabilities = operations;
            this.ResourceType = resourceType;
        }

        /// <summary>
        /// Gets the name of the resource
        /// </summary>
        [XmlAttribute("resource"), JsonProperty("resource")]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the operations supported by this resource
        /// </summary>
        [XmlElement("cap"), JsonProperty("cap")]
        public List<ServiceResourceCapability> Capabilities { get; set; }

        /// <summary>
        /// Gets the type of resource
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type ResourceType { get; set; }
    }
}
#pragma warning restore CS1591
