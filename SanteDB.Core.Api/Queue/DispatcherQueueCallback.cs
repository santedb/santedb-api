using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Queue
{
    /// <summary>
    /// Delegate for enqueued message callback
    /// </summary>
    public delegate void DispatcherQueueCallback(DispatcherMessageEnqueuedInfo enqueuedInfo);

    /// <summary>
    /// Represents event args related to a queue event
    /// </summary>
    public class DispatcherMessageEnqueuedInfo
    {
        /// <summary>
        /// Gets the name of the queue
        /// </summary>
        public String QueueName { get; private set; }

        /// <summary>
        /// Get the correlation token or object that was provided by the queue
        /// </summary>
        public String CorrelationId { get; private set; }

        /// <summary>
        /// Create a new persistence queue event arg instance
        /// </summary>
        /// <param name="queueName">The name of the queue</param>
        /// <param name="correlationId">The data in the queue</param>
        public DispatcherMessageEnqueuedInfo(String queueName, String correlationId)
        {
            this.CorrelationId = correlationId;
            this.QueueName = queueName;
        }
    }
}