/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Queue
{
    /// <summary>
    /// A service which is responsible for storing messages in a reliable place for dispatching
    /// </summary>
    /// <remarks>
    /// Whenever SanteDB sends messages to another system using messaging, there is no guarantee that the remote endpoint will be available,
    /// or accessable, etc. Therefore all messages which are sent (dispatched) to a remote system are first queued using this wrapper service.
    /// <para>The purpose of the dispatcher queue is the store the outbound message locally in some persistent place, then to notify any
    /// listeners that a new message is ready for sending. If the message cannot be sent, then the sending service should place
    /// the message back onto the queue or onto a dedicated deadletter queue.</para>
    /// </remarks>
    public interface IDispatcherQueueManagerService : IServiceImplementation
    {
        /// <summary>
        /// Opens the specified queue name and enables subscriptions
        /// </summary>
        void Open(String queueName);


        /// <summary>
        /// Subscribes to <paramref name="queueName"/> using <paramref name="callback"/>
        /// </summary>
        void SubscribeTo(String queueName, DispatcherQueueCallback callback);

        /// <summary>
        /// Remove the callback registration
        /// </summary>
        void UnSubscribe(String queueName, DispatcherQueueCallback callback);

        /// <summary>
        /// Enqueue the specified data to the persistent queue
        /// </summary>
        void Enqueue(String queueName, Object data);

        /// <summary>
        /// Dequeues the last added item from the persistent queue
        /// </summary>
        DispatcherQueueEntry Dequeue(String queueName);

        /// <summary>
        /// De-queue a specific message
        /// </summary>
        DispatcherQueueEntry DequeueById(String queueName, String correlationId);

        /// <summary>
        /// Purge the queue
        /// </summary>
        void Purge(String queueName);

        /// <summary>
        /// Move an entry from one queue to another
        /// </summary>
        DispatcherQueueEntry Move(DispatcherQueueEntry entry, string toQueue);

        /// <summary>
        /// Get the specified queue entry
        /// </summary>
        DispatcherQueueEntry GetQueueEntry(string queueName, string correlationId);

        /// <summary>
        /// Gets the queues for this system
        /// </summary>
        IEnumerable<DispatcherQueueInfo> GetQueues();

        /// <summary>
        /// Get all queue entries
        /// </summary>
        IEnumerable<DispatcherQueueEntry> GetQueueEntries(String queueName);
    }
}