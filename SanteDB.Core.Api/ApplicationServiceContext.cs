﻿/*
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
 * Date: 2018-6-22
 */
using System;

namespace SanteDB.Core
{
    /// <summary>
    /// Application context
    /// </summary>
    public static class ApplicationServiceContext
    {

        /// <summary>
        /// Helper extension method for getting strongly typed service
        /// </summary>
        /// <typeparam name="T">The type of service to be retrieved</typeparam>
        /// <param name="me">The reference to the service provider</param>
        /// <returns>The fetched / registered service implementation</returns>
        public static T GetService<T>(this IServiceProvider me)
        {
            return (T)me.GetService(typeof(T));
        }

        /// <summary>
        /// Gets or sets the current application service context
        /// </summary>
        public static IApplicationServiceContext Current { get; set; }

        /// <summary>
        /// Type of application hosting this SanteDB
        /// </summary>
        public static SanteDBHostType HostType { get; set; }
    }
}
