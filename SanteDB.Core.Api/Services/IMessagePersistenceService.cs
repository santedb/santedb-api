/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-27
 */
using System;
using System.Collections.Generic;
using System.IO;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Identifies the status of a message
    /// </summary>
    public enum MessageState
    {
        /// <summary>
        /// The message has never been received by the system
        /// </summary>
        New,
        /// <summary>
        /// The message has been received by the system and is in process
        /// </summary>
        Active,
        /// <summary>
        /// The message has been received by the system and processing is complete
        /// </summary>
        Complete
    }

    /// <summary>
    /// Message information
    /// </summary>
    public class MessageInfo
    {
        /// <summary>
        /// Gets the id of the message
        /// </summary>
        public String Id { get; set; }
        /// <summary>
        /// Gets the message id that this message responds to or the response of this message.
        /// </summary>
        public String Response { get; set; }
        /// <summary>
        /// Gets the remote endpoint of the message
        /// </summary>
        public Uri Source { get; set; }
        /// <summary>
        /// Gets the local endpoint of the message
        /// </summary>
        public Uri Destination { get; set; }
        /// <summary>
        /// Gets the time the message was received
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Gets the body of the message
        /// </summary>
        public byte[] Body { get; set; }
        /// <summary>
        /// Gets or sets the state of the message
        /// </summary>
        public MessageState State { get; set; }

    }

    /// <summary>
    /// Identifies a service which maintains a log of messages received to ensure that actions are only processed once
    /// </summary>
    /// <remarks>
    /// <para>
    /// In a health context, certain types of messages which represent data triggers (such as an admit, discharge, order, etc.) may trigger 
    /// a business process which, in turn, kicks off another business process. Sometimes this process can take a long time to complete, or
    /// (due to network issues) the caller may disconnect prior to receiving a response. This service is responsible for storing that 
    /// a message has been received by the SanteDB infrastructure and is currently being (or was already) processed, and allows SanteDB
    /// to simply return the already executed message back to the caller.
    /// </para>
    /// <para>Note: This service is only currently used by the HL7v2 service</para>
    /// </remarks>
    [System.ComponentModel.Description("Exec-Once Message Persistence")]
    public interface IMessagePersistenceService : IServiceImplementation
    {

        /// <summary>
        /// Get the current state of a message processing by the unique identifier of the message
        /// </summary>
        /// <param name="messageId">The identification of the message (either WS-RM ID, HDSI ID, or HL7 MSH10)</param>
        /// <returns>The current status of the message</returns>
        MessageState GetMessageState(string messageId);

        /// <summary>
        /// Instructs the message persistence service to store an inbound message 
        /// </summary>
        /// <param name="messageId">The identifier of the message</param>
        /// <param name="message">The message body which was received (for re-processing or comparison if needed)</param>
        void PersistMessage(string messageId, Stream message);

        /// <summary>
        /// Persist metadata about the message
        /// </summary>
        /// <param name="message">The message metadata to be persisted</param>
        void PersistMessageInfo(MessageInfo message);

        /// <summary>
        /// Get the response to the supplied request message identifier
        /// </summary>
        /// <param name="messageId">The identifier of the request message</param>
        /// <returns>The stream of data which represents the response to the message</returns>
        Stream GetMessageResponseMessage(string messageId);

        /// <summary>
        /// Get a message body by message identifier
        /// </summary>
        /// <param name="messageId">the identifier of the message body to retrieve</param>
        /// <returns>The stored message</returns>
        Stream GetMessage(string messageId);

        /// <summary>
        /// Persist the result of a message request (i.e. the result of the request)
        /// </summary>
        /// <param name="response">The response message body to be persisted</param>
        /// <param name="messageId">The identifier of the result message to be stored</param>
        /// <param name="respondsToId">The identifier of the message which this message responds to</param>
        void PersistResultMessage(string messageId, string respondsToId, Stream response);

        /// <summary>
        /// Get all message ids between the specified time(s)
        /// </summary>
        /// <param name="from">The lower boundary of the time</param>
        /// <param name="to">The upper boundary of the receive time</param>
        IEnumerable<String> GetMessageIds(DateTime from, DateTime to);

        /// <summary>
        /// Get message extended attributes
        /// </summary>
        /// <param name="messageId">The message identifier to retrieve extended metadata for</param>
        MessageInfo GetMessageInfo(String messageId);

    }
}
