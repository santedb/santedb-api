using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Represents a channel definition which can 
    /// </summary>
    public interface IPubSubDispatcherFactory
    {

        /// <summary>
        /// Creates a new dispatcher 
        /// </summary>
        IPubSubDispatcher CreateDispatcher(Guid channelKey, Uri endpoint, IDictionary<String, String> settings);

    }
}
