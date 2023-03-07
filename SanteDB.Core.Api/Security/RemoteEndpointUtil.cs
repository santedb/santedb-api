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
using System.Net;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents information about a remote client
    /// </summary>
    public class RemoteEndpointInfo
    {
        /// <summary>
        /// Gets or sets the correlation token
        /// </summary>
        public String CorrelationToken { get; set; }

        /// <summary>
        /// Gets or sets the remote address
        /// </summary>
        public String RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets the original request url
        /// </summary>
        public String OriginalRequestUrl { get; set; }

        /// <summary>
        /// Forwarding information
        /// </summary>
        public string ForwardInformation { get; set; }

    }

    /// <summary>
    /// Represents a resolver service which can get the current request endpoint
    /// </summary>
    public class RemoteEndpointUtil
    {
        // Singleton instance
        private static RemoteEndpointUtil s_instance;

        // Providers
        private List<Func<RemoteEndpointInfo>> m_providers = new List<Func<RemoteEndpointInfo>>();

        /// <summary>
        /// Gets the singleton
        /// </summary>
        public static RemoteEndpointUtil Current
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new RemoteEndpointUtil();
                }

                return s_instance;
            }
        }

        /// <summary>
        /// Singleton
        /// </summary>
        private RemoteEndpointUtil()
        {
        }

        /// <summary>
        /// Adds a provider to this service which, when the function returns a value, indicates the channel is being used
        /// </summary>
        public void AddEndpointProvider(Func<RemoteEndpointInfo> provider)
        {
            if (!this.m_providers.Contains(provider))
            {
                this.m_providers.Add(provider);
            }
        }

        /// <summary>
        /// Scans the providers to find a remote client
        /// </summary>
        public RemoteEndpointInfo GetRemoteClient()
        {
            foreach (var itm in this.m_providers)
            {
                var retVal = itm();
                if (retVal != null)
                {
                    return retVal;
                }
            }
            return null;
        }

    }
}