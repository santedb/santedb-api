using SanteDB.Core.Model;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// A service which logs and manages the logging of sending
    /// </summary>
    public interface IPubSubLogService
    {

        /// <summary>
        /// Log the dispatching of an object to the remote server
        /// </summary>
        /// <param name="subscriptionName">The subscription that the object was dispathced on</param>
        /// <param name="dispachedEntity">The entity that was dispatched</param>
        /// <param name="eventType">The type of event</param>
        /// <param name="outcome">The outcome of the dispatch</param>
        PubSubDispatchLog LogDispatch(String subscriptionName, IdentifiedData dispachedEntity, PubSubEventType eventType, OutcomeIndicator outcome);

        /// <summary>
        /// Get all the times that the object was dispatched on the specified channel
        /// </summary>
        /// <param name="subscriptionName">The name of the channel which should be checked</param>
        /// <param name="objectKey">The object key to be checked</param>
        /// <returns>An enumeration of the dispatches</returns>
        IEnumerable<PubSubDispatchLog> GetDispatches(String subscriptionName, Guid objectKey);

        /// <summary>
        /// Get the last dispatch log entry from the object on the subscription
        /// </summary>
        /// <param name="subscriptionName">The subscription name</param>
        /// <param name="objectKey">The object key</param>
        /// <returns>The last subscription</returns>
        PubSubDispatchLog GetLastDispatch(String subscriptionName, Guid objectKey);


    }
}
