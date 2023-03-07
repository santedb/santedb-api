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
