﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Interop
{

    /// <summary>
    /// The type of binding that the child object makes
    /// </summary>
    [XmlType(nameof(ChildObjectScopeBinding), Namespace = "http://santedb.org/model"), Flags]
    public enum ChildObjectScopeBinding
    {
        /// <summary>
        /// The API child object is bound to an instance of the object
        /// </summary>
        [XmlEnum("instance")]
        Instance = 0x1,
        /// <summary>
        /// The API child object is bound to the parent type
        /// </summary>
        [XmlEnum("class")]
        Class = 0x2
    }

    /// <summary>
    /// The classification of the object classification
    /// </summary>
    [XmlType(nameof(ChildObjectClassification), Namespace = "http://santedb.org/model"), Flags]
    public enum ChildObjectClassification
    {
        /// <summary>
        /// The object is a resource
        /// </summary>
        [XmlEnum("resource")]
        Resource = 0x1,
        /// <summary>
        /// The object is an operation
        /// </summary>
        [XmlEnum("operation")]
        RpcOperation = 0x2,
    }

    /// <summary>
    /// Service resource operations
    /// </summary>
    [XmlType(nameof(ResourceCapabilityType), Namespace = "http://santedb.org/model"), Flags]
    public enum ResourceCapabilityType
    {
        /// <summary>
        /// There is no capability on this resource
        /// </summary>
        [XmlEnum("none")]
        None = 0x00,
        /// <summary>
        /// The resource can accept CREATE operations
        /// </summary>
        [XmlEnum("create")]
        Create = 0x001,
        /// <summary>
        /// The resource can either CREATE or UPDATE data
        /// </summary>
        [XmlEnum("create-update")]
        CreateOrUpdate = 0x002,
        /// <summary>
        /// The resource can be updated via PUT
        /// </summary>
        [XmlEnum("update")]
        Update = 0x004,
        /// <summary>
        /// The resource can be deleted
        /// </summary>
        [XmlEnum("delete")]
        Delete = 0x008,
        /// <summary>
        /// The resource can be patched
        /// </summary>
        [XmlEnum("patch")]
        Patch = 0x010,
        /// <summary>
        /// The resource can be retrieved by ID
        /// </summary>
        [XmlEnum("get")]
        Get = 0x020,
        /// <summary>
        /// The resource can be retrieved and supports versioning
        /// </summary>
        [XmlEnum("get-version")]
        GetVersion = 0x040,
        /// <summary>
        /// The resource has the history function
        /// </summary>
        [XmlEnum("history")]
        History = 0x080,
        /// <summary>
        /// The resource can be searched
        /// </summary>
        [XmlEnum("search")]
        Search = 0x100
    }

    /// <summary>
    /// Resource capability extensions
    /// </summary>
    public static class ResourceCapabilityTypeExtensions
    {

        /// <summary>
        /// Conver the ResourceCapbility into a collection of service metadata capapbilities
        /// </summary>
        /// <param name="me">The resource capability type to convert</param>
        /// <param name="getDemandsFunc">A callback which fetches the policies the resource requires.</param>
        /// <returns>The collection of metadata capabilities to pass to the OPTIONS call</returns>
        public static IEnumerable<ServiceResourceCapability> ToResourceCapabilityStatement(this ResourceCapabilityType me, Func<ResourceCapabilityType, String[]> getDemandsFunc)
        {
            var caps = new List<ServiceResourceCapability>();
            if (me.HasFlag(ResourceCapabilityType.Create))
            {
                caps.Add(new ServiceResourceCapability(ResourceCapabilityType.Create, getDemandsFunc(ResourceCapabilityType.Create)));
            }

            if (me.HasFlag(ResourceCapabilityType.CreateOrUpdate))
            {
                caps.Add(new ServiceResourceCapability(ResourceCapabilityType.CreateOrUpdate, getDemandsFunc(ResourceCapabilityType.CreateOrUpdate)));
            }

            if (me.HasFlag(ResourceCapabilityType.Delete))
            {
                caps.Add(new ServiceResourceCapability(ResourceCapabilityType.Delete, getDemandsFunc(ResourceCapabilityType.Delete)));
            }

            if (me.HasFlag(ResourceCapabilityType.Get))
            {
                caps.Add(new ServiceResourceCapability(ResourceCapabilityType.Get, getDemandsFunc(ResourceCapabilityType.Get)));
            }

            if (me.HasFlag(ResourceCapabilityType.GetVersion))
            {
                caps.Add(new ServiceResourceCapability(ResourceCapabilityType.GetVersion, getDemandsFunc(ResourceCapabilityType.GetVersion)));
            }

            if (me.HasFlag(ResourceCapabilityType.History))
            {
                caps.Add(new ServiceResourceCapability(ResourceCapabilityType.History, getDemandsFunc(ResourceCapabilityType.History)));
            }

            if (me.HasFlag(ResourceCapabilityType.Search))
            {
                caps.Add(new ServiceResourceCapability(ResourceCapabilityType.Search, getDemandsFunc(ResourceCapabilityType.Search)));
            }

            if (me.HasFlag(ResourceCapabilityType.Update))
            {
                caps.Add(new ServiceResourceCapability(ResourceCapabilityType.Update, getDemandsFunc(ResourceCapabilityType.Update)));
            }

            return caps;
        }
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
        /// <param name="subResources">Resources which are children of this resource</param>
        public ServiceResourceOptions(string resourceName, Type resourceType, List<ServiceResourceCapability> operations, List<ChildServiceResourceOptions> subResources)
        {
            this.ResourceName = resourceName;
            this.Capabilities = operations;
            this.ResourceType = resourceType;
            this.ChildResources = subResources;
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

        /// <summary>
        /// Gets the resources which are associated with this resource (sub-resources)
        /// </summary>
        [XmlElement("children"), JsonProperty("children")]
        public List<ChildServiceResourceOptions> ChildResources { get; set; }

    }

    /// <summary>
    /// Child service resource options
    /// </summary>
    [XmlType(nameof(ChildServiceResourceOptions), Namespace = "http://santedb.org/model"), JsonObject(nameof(ChildServiceResourceOptions))]
    public class ChildServiceResourceOptions : ServiceResourceOptions
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOptions"/> class.
        /// </summary>
        public ChildServiceResourceOptions()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResourceOptions"/> class
        /// with a specific resource name, and verbs.
        /// </summary>
        /// <param name="resourceName">The name of the resource of the service resource options.</param>
        /// <param name="operations">The list of HTTP verbs of the resource option.</param>
        /// <param name="resourceType">The type of resource which options are being fetched for</param>
        /// <param name="classification">The scope of the child oject (if it applies to class or instance)</param>
        /// <param name="scope">The scope of the child resource</param>
        public ChildServiceResourceOptions(string resourceName, Type resourceType, List<ServiceResourceCapability> operations, ChildObjectScopeBinding scope, ChildObjectClassification classification)
            : base(resourceName, resourceType, operations, null)
        {
            this.Scope = scope;
            this.Classification = classification;
        }

        /// <summary>
        /// Gets or sets the scope of this object.
        /// </summary>
        [XmlAttribute("scope"), JsonProperty("scope")]
        public ChildObjectScopeBinding Scope { get; set; }

        /// <summary>
        /// Gets or sets the classification of this object
        /// </summary>
        [XmlAttribute("classification"), JsonProperty("classification")]
        public ChildObjectClassification Classification { get; set; }


    }
}
