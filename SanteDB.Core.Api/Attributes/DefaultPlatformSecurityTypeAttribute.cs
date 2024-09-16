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
 */
using SanteDB.Core.Security;
using System;

namespace SanteDB.Core.Attributes
{
    /// <summary>
    /// Instructs the <see cref="X509CertificateUtils"/> class to use a default implementation (overridding the 
    /// detected <see cref="MonoPlatformSecurityProvider"/> or <see cref="DefaultPlatformSecurityProvider"/>)
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DefaultPlatformSecurityTypeAttribute : Attribute
    {

        /// <summary>
        /// Platform security implementation ctor
        /// </summary>
        /// <param name="platformSecurityImplementation"></param>
        public DefaultPlatformSecurityTypeAttribute(Type platformSecurityImplementation)
        {
            this.PlatformSecurityProviderType = platformSecurityImplementation;
        }

        /// <summary>
        /// Gets the platform security provider type
        /// </summary>
        public Type PlatformSecurityProviderType { get; set; }
    }
}
