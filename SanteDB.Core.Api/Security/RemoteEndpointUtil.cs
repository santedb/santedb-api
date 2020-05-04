using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Api.Security
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
                    s_instance = new RemoteEndpointUtil();
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
            if(!this.m_providers.Contains(provider))
                this.m_providers.Add(provider);
        }

        /// <summary>
        /// Scans the providers to find a remote client
        /// </summary>
        public RemoteEndpointInfo GetRemoteClient()
        {
            foreach(var itm in this.m_providers)
            {
                var retVal = itm();
                if (retVal != null)
                    return retVal;
            }
            return null;
        }
    }
}
