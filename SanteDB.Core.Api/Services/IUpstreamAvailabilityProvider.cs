using SanteDB.Core.Interop;
using System;
using System.Collections.Generic;
using System.Text;

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
        long GetUpstreamLatency(ServiceEndpointType endpointType);

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
        TimeSpan GetTimeDrift(ServiceEndpointType endpoint);

    }
}
