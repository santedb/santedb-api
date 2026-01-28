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
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Queue
{
    /// <summary>
    /// Queue entry
    /// </summary>
    [XmlType(nameof(DispatcherQueueEntry), Namespace = "http://santedb.org/queue")]
    [XmlRoot(nameof(DispatcherQueueEntry), Namespace = "http://santedb.org/queue")]
    public class DispatcherQueueEntry
    {
        /// <summary>
        /// Serialization ctor
        /// </summary>
        public DispatcherQueueEntry()
        {
        }

        /// <summary>
        /// Get the queue entry
        /// </summary>
        /// <param name="correlationId">The id of the queue entry</param>
        /// <param name="sourceQueue">The source queue where this entry came from</param>
        /// <param name="timestamp">The time the entry was created</param>
        /// <param name="label">A descriptive label for the object</param>
        /// <param name="body">The body information</param>
        public DispatcherQueueEntry(String correlationId, String sourceQueue, DateTime timestamp, String label, object body)
        {
            this.CorrelationId = correlationId;
            this.SourceQueue = sourceQueue;
            this.Timestamp = timestamp;
            this.Body = body;
            this.Label = label;
        }

        /// <summary>
        /// Gets or sets the payload type
        /// </summary>
        [XmlElement("payloadType"), JsonProperty("payloadType")]
        public String Label { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier
        /// </summary>
        [XmlElement("correlationId"), JsonProperty("correlationId")]
        public String CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the source queue
        /// </summary>
        [XmlElement("sourceQueue"), JsonProperty("sourceQueue")]
        public String SourceQueue { get; set; }

        /// <summary>
        /// Gets the timestamp
        /// </summary>
        [XmlElement("timestamp"), JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets the body of this object
        /// </summary>
        [XmlElement("body"), JsonProperty("body")]
        public Object Body { get; set; }
    }
}