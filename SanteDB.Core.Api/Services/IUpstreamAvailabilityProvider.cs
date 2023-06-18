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
 * Date: 2023-5-19
 */
using SanteDB.Core.Interop;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Implementers can fetch metadata about the upstream service
    /// </summary>
    public interface IUpstreamAvailabilityProvider : IServiceImplementation
    {

        /// <summary>
        /// Determines the application layer latency with the specified endpoint
        /// </summary>
        /// <param name="endpointType">The type of endpoint</param>
        /// <returns>The latency in milliseconds</returns>
        /// <remarks>This method is used to determine the amount of time the application endpoint responds to a PING request roundtrip</remarks>
        long? GetUpstreamLatency(ServiceEndpointType endpointType);

        /// <summary>
        /// Determines whether the upstream service is available.
        /// </summary>
        /// <param name="endpoint">The remote endpoint to determine available for</param>
        /// <returns>Returns true if the network is available.</returns>
        bool IsAvailable(ServiceEndpointType endpoint);

        /// <summary>
        /// Get server time drift between this machine and the server
        /// </summary>
        /// <param name="endpoint">The service endpoint to determine time drift from</param>
        /// <returns>The time drift</returns>
        TimeSpan? GetTimeDrift(ServiceEndpointType endpoint);

    }
}
