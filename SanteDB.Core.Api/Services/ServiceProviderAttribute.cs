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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Instance
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
        public ServiceProviderAttribute(String name, ServiceInstantiationType type = ServiceInstantiationType.Singleton, Type configurationType = null)
        {
            this.Name = name;
            this.Type = type;
            this.Configuration = configurationType;
        }

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
}
