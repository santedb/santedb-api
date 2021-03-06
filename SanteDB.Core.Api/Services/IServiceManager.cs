﻿/*
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
 * Date: 2021-2-19
 */
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Interfaces
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
        IEnumerable<TType> CreateInjectedOfAll<TType>();

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