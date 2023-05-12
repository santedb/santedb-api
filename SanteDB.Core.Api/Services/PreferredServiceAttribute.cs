/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// This attribute tells the dependency injection manager that the current 
    /// implementation should be treated as the default when GetService is called
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PreferredServiceAttribute : Attribute
    {

        /// <summary>
        /// Creates a new preferred service attribute
        /// </summary>
        /// <param name="serviceType">The service type this implementation is preferred implementation for</param>
        public PreferredServiceAttribute(Type serviceType)
        {
            this.ServiceType = serviceType;
        }

        /// <summary>
        /// The type of service which this service is preferred for
        /// </summary>
        public Type ServiceType { get; set; }
    }
}
