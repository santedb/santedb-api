/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Interop;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Service instantiation type
    /// </summary>
    public enum ServiceInstantiationType
    {
        /// <summary>
        /// The service class is constructed once and one instance is shared in the entire application domain
        /// </summary>
        Singleton,
        /// <summary>
        /// The service class is instantiated for each call of GetService()
        /// </summary>
        PerCall
    }

    /// <summary>
    /// Identifies the manner in which a service is executed
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceProviderAttribute : Attribute
    {

        /// <summary>
        /// Identifies a service provider
        /// </summary>
        public ServiceProviderAttribute(String name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Required services must be present when the service starts up
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets the name of the service
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the configuration type
        /// </summary>
        public Type Configuration { get; set; }

        /// <summary>
        /// Service type
        /// </summary>
        public ServiceInstantiationType Type { get; set; }

        /// <summary>
        /// Gets or sets the dependencies of this service
        /// </summary>
        public Type[] Dependencies { get; set; }
    }

    /// <summary>
    /// Represents a service provider which is for an API
    /// </summary>
    public class ApiServiceProviderAttribute : ServiceProviderAttribute
    {
        /// <summary>
        /// Creates a new API service provider
        /// </summary>
        public ApiServiceProviderAttribute(string name, Type behaviorType, ServiceEndpointType serviceType) : base(name)
        {
            this.BehaviorType = behaviorType;
            this.ServiceType = serviceType;
        }

        /// <summary>
        /// Gets or sets the contract type
        /// </summary>
        public Type BehaviorType { get; }

        /// <summary>
        /// Gets the service type
        /// </summary>
        public ServiceEndpointType ServiceType { get; }
    }

}
