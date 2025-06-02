/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Http.Description;
using SanteDB.Core.Interop;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents a class which can resolve REST client implementations
    /// </summary>
    public interface IRestClientFactory
    {

        /// <summary>
        /// Resolve a rest client which communicates with <paramref name="serviceEndpointType"/>
        /// </summary>
        /// <param name="serviceEndpointType">The service endpoint to resolve</param>
        /// <returns>The resolved rest client</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">No configuration is avaiable for the type of client requested with <paramref name="serviceEndpointType"/>.</exception>
        IRestClient GetRestClientFor(ServiceEndpointType serviceEndpointType);

        /// <summary>
        /// Resolve a rest client which is named <paramref name="clientName"/>
        /// </summary>
        /// <param name="clientName">The service endpoint to resolve</param>
        /// <returns>The resolved rest client</returns>
        IRestClient GetRestClientFor(string clientName);

        /// <summary>
        /// Create a rest client based on the description 
        /// </summary>
        /// <param name="description">The description to create the rest client for</param>
        /// <returns>The constructed rest client</returns>
        IRestClient CreateRestClient(IRestClientDescription description);

        /// <summary>
        /// Try to get a rest client for <paramref name="serviceEndpointType"/>
        /// </summary>
        /// <param name="serviceEndpointType">The type of endpoint to retrieve a client for</param>
        /// <param name="restClient">The rest client that was retrieved</param>
        /// <returns>True if the rest client exists</returns>
        bool TryGetRestClientFor(ServiceEndpointType serviceEndpointType, out IRestClient restClient);

        /// <summary>
        /// Try to get a rest client named <paramref name="clientName"/>
        /// </summary>
        /// <param name="clientName">The name of the client for</param>
        /// <param name="restClient">The rest client that was retrieved</param>
        /// <returns>True if the rest client exists</returns>
        bool TryGetRestClientFor(string clientName, out IRestClient restClient);

    }
}
