using SanteDB.Core.Http.Description;
using SanteDB.Core.Interop;
using System;
using System.Collections.Generic;
using System.Text;

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
        IRestClient GetRestClientFor(ServiceEndpointType serviceEndpointType);

        /// <summary>
        /// Resolve a rest client which communicates with <paramref name="clientName"/>
        /// </summary>
        /// <param name="clientName">The service client name to resolve</param>
        /// <returns>The resolved rest client</returns>
        IRestClient GetRestClientFor(string clientName);

        /// <summary>
        /// Create a rest client based on the description 
        /// </summary>
        /// <param name="description">The description to create the rest client for</param>
        /// <returns>The constructed rest client</returns>
        IRestClient CreateRestClient(IRestClientDescription description);
    }
}
