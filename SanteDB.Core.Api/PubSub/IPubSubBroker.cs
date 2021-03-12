using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Represents a pub-sub broker
    /// </summary>
    public interface IPubSubBroker
    {

        /// <summary>
        /// Find dispatcher factory which can handle this URI
        /// </summary>
        IPubSubDispatcherFactory FindDispatcherFactory(Uri targetUri);

        /// <summary>
        /// Gets the dispatcher factory
        /// </summary>
        IPubSubDispatcherFactory GetDispatcherFactory(Type factoryType);
    }
}
