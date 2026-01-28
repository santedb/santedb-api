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
using System;

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