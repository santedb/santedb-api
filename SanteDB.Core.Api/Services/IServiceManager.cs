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
 * Date: 2022-5-30
 */
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service manager
    /// </summary>
    public interface IServiceManager
    {
        /// <summary>
        /// Add the specified service provider
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        void AddServiceProvider(Type serviceType);

        /// <summary>
        /// Add the specified service provider
        /// </summary>
        /// <param name="serviceInstance">The service instance to be added.</param>
        void AddServiceProvider(object serviceInstance);

        /// <summary>
        /// Get all services
        /// </summary>
        IEnumerable<object> GetServices();

        /// <summary>
        /// Creates all instances 
        /// </summary>
        IEnumerable<T> CreateAll<T>(params object[] parms);

        /// <summary>
        /// Removes a service provider
        /// </summary>
        void RemoveServiceProvider(Type serviceType);

        /// <summary>
        /// Gets the service contract types
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetAllTypes();

        /// <summary>
        /// Creates injected instances of all <typeparamref name="TType"/>
        /// </summary>
        IEnumerable<TType> CreateInjectedOfAll<TType>(Assembly fromAssembly = null);

        /// <summary>
        /// Create a new, injected <paramref name="type"/>
        /// </summary>
        Object CreateInjected(Type type);

        /// <summary>
        /// Create a new injected instance of <typeparamref name="TObject"/>
        /// </summary>
        TObject CreateInjected<TObject>();
    }
}