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
using SanteDB.Core.Configuration.Http;
using SanteDB.Core.Http.Description;
using SanteDB.Core.i18n;
using SanteDB.Core.Interop;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Rest client factory
    /// </summary>
    internal class RestClientFactory : IRestClientFactory
    {
        // Get the rest client configuration section
        private readonly RestClientConfigurationSection m_configuration;

        /// <summary>
        /// DI constructor
        /// </summary>
        public RestClientFactory(IConfigurationManager configurationManager)
        {
            this.m_configuration = configurationManager.GetSection<RestClientConfigurationSection>();
            if (this.m_configuration == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, typeof(RestClientConfigurationSection)));
            }
        }

        /// <inheritdoc/>
        public IRestClient CreateRestClient(IRestClientDescription description) => Activator.CreateInstance(this.m_configuration.RestClientType.Type, description) as IRestClient;

        /// <inheritdoc/>
        public IRestClient GetRestClientFor(ServiceEndpointType serviceEndpointType) => this.GetRestClientFor(serviceEndpointType.ToString());

        /// <inheritdoc/>
        public IRestClient GetRestClientFor(String clientName)
        {
            if (!this.TryGetRestClientFor(clientName, out var retVal))
            {
                throw new KeyNotFoundException(clientName);
            }
            return retVal;
        }

        /// <inheritdoc/>
        public bool TryGetRestClientFor(ServiceEndpointType endpointType, out IRestClient restClient) => this.TryGetRestClientFor(endpointType.ToString(), out restClient);

        /// <inheritdoc/>
        public bool TryGetRestClientFor(String clientName, out IRestClient restClient)
        {
            var configuration = this.m_configuration.Client.Find(o => o.Name.Equals(clientName, StringComparison.OrdinalIgnoreCase));
            if (configuration == null)
            {
                restClient = null;
                return false;
            }
            else
            {
                configuration.ProxyAddress = this.m_configuration.ProxyAddress;
                restClient = this.CreateRestClient(configuration);
                return true;
            }
        }
    }
}
