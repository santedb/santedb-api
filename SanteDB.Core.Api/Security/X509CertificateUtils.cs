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
using SanteDB.Core.Attributes;
using System;
using System.Reflection;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Utilities for x509 certificates
    /// </summary>
    public static class X509CertificateUtils
    {

        private readonly static IPlatformSecurityProvider s_defaultProvider;

        /// <summary>
        /// Static CTOR
        /// </summary>
        static X509CertificateUtils()
        {
            // Is the entry assembly tagged with the [DefaultPlatformServiceTypeAttribute]?
            var annotatedDefault = Assembly.GetEntryAssembly()?.GetCustomAttribute<DefaultPlatformSecurityTypeAttribute>();
            if (annotatedDefault != null)
            {
                s_defaultProvider = Activator.CreateInstance(annotatedDefault.PlatformSecurityProviderType) as IPlatformSecurityProvider;
            }
            else if (Type.GetType("Mono.Runtime") != null)
            {
                s_defaultProvider = new MonoPlatformSecurityProvider();
            }
            else
            {
                s_defaultProvider = new DefaultPlatformSecurityProvider();
            }
        }

        /// <summary>
        /// Gets the default platform security provider from the running context if it is running, otherwise retrieves the 
        /// core SanteDB platform provider for the detected hosting environment
        /// </summary>
        public static IPlatformSecurityProvider GetPlatformServiceOrDefault() =>
            ApplicationServiceContext.Current?.GetService<IPlatformSecurityProvider>() ?? s_defaultProvider;
    }
}

