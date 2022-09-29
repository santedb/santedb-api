﻿/*
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
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using System;

namespace SanteDB.Core
{
    /// <summary>
    /// Application context
    /// </summary>
    public static class ApplicationServiceContext
    {

        // Current context
        private static IApplicationServiceContext m_current;

        /// <summary>
        /// Helper extension method for getting strongly typed service
        /// </summary>
        /// <typeparam name="T">The type of service to be retrieved</typeparam>
        /// <param name="me">The reference to the service provider</param>
        /// <returns>The fetched / registered service implementation</returns>
        public static T GetService<T>(this IServiceProvider me)
        {
            return (T)me?.GetService(typeof(T));
        }

        /// <summary>
        /// Gets or sets the current application service context
        /// </summary>
        public static IApplicationServiceContext Current
        {
            get {
                return m_current;
            }
            internal set
            {
                if(m_current != null)
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.MULTIPLE_CALLS_NOT_ALLOWED, "Initialize"));
                }
                m_current = value;
            }
        }


    }
}
