using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a remote endpoint resolver
    /// </summary>
    public interface IRemoteEndpointResolver : IServiceImplementation
    {

        /// <summary>
        /// Get the remote endpoint
        /// </summary>
        string GetRemoteEndpoint();

        /// <summary>
        /// Gets the remote request url
        /// </summary>
        string GetRemoteRequestUrl();
    }
}
