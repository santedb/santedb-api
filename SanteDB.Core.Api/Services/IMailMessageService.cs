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
using SanteDB.Core.Event;
using SanteDB.Core.Mail;
using SanteDB.Core.Model.Query;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which handles the mailbox services.
    /// </summary>
    [System.ComponentModel.Description("Mail Repository Provider")]
    public interface IMailMessageService : IServiceImplementation
    {
        /// <summary>
        /// The message has been placed into the sent box from the Send() command - it may have not been delivered to all recipients
        /// </summary>
        event EventHandler<MailMessageEventArgs> Sent;

        /// <summary>
        /// Fired when an mail message has been received.
        /// </summary>
        event EventHandler<MailMessageEventArgs> Delivered;

        /// <summary>
        /// Message has been updated
        /// </summary>
        event EventHandler<MailMessageEventArgs> Updated;

        /// <summary>
        /// Message has been deleted
        /// </summary>
        event EventHandler<MailMessageEventArgs> Deleted;

        /// <summary>
        /// Mailbox has been created
        /// </summary>
        event EventHandler<DataPersistedEventArgs<Mailbox>> MailboxCreated;

        /// <summary>
        /// Mailbox has been deleted
        /// </summary>
        event EventHandler<DataPersistedEventArgs<Mailbox>> MailboxDeleted;

        /// <summary>
        /// Send the specified mailmessage according to its sending instructions
        /// </summary>
        /// <param name="mail">The mail message to be sent</param>
        /// <param name="fromKey">Override the "from"</param>
        MailMessage Send(MailMessage mail, Guid? fromKey = null);

        /// <summary>
        /// Send a mail message with the specified subject, body, flags and recipients
        /// </summary>
        /// <param name="subject">The subject of the message</param>
        /// <param name="body">The body of the message</param>
        /// <param name="flags">The flags of the message</param>
        /// <param name="recipients">The recipient role, user, or device</param>
        /// <param name="toLine">The informational TO line (for when the recipient cannot be identified</param>
        /// <returns>The sent mail message</returns>
        MailMessage Send(String subject, String body, MailMessageFlags flags, string toLine, params Guid[] recipients);

        /// <summary>
        /// Initialize the mailboxes for the specified security object
        /// </summary>
        /// <param name="identity">The security object</param>
        void InitializeMailboxes(IIdentity identity);

        /// <summary>
        /// Get mailboxes for the specified user or the current user
        /// </summary>
        IQueryResultSet<Mailbox> GetMailboxes(Guid? forSecuriytObjectKey = null);

        /// <summary>
        /// Get a specific mailbox
        /// </summary>
        /// <param name="mailboxKey">The name of the mailbox</param>
        /// <returns>The mailbox</returns>
        Mailbox GetMailbox(Guid mailboxKey);

        /// <summary>
        /// Get the mailbox of the current user by name
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox</param>
        /// <returns>The mailbox</returns>
        Mailbox GetMailboxByName(String mailboxName);

        /// <summary>
        /// Create a new mailbox for the specified user
        /// </summary>
        /// <param name="name">The name of the mailbox to be created</param>
        /// <param name="ownerKey">The key of the user that should own the mailbox</param>
        /// <returns>The created mailbox</returns>
        Mailbox CreateMailbox(String name, Guid? ownerKey = null);

        /// <summary>
        /// Get messages from the mailbox <paramref name="mailboxKey"/>
        /// </summary>
        /// <param name="mailboxKey">The mailbox key to fetch messages for</param>
        /// <returns>The messages</returns>
        IQueryResultSet<MailboxMailMessage> GetMessages(Guid mailboxKey);

        /// <summary>
        /// Get the specified mail message
        /// </summary>
        /// <param name="mailboxKey">The mailbox to retrieve</param>
        /// <param name="messageKey">The message to retrieve</param>
        /// <returns>The mail message</returns>
        MailboxMailMessage GetMailMessage(Guid mailboxKey, Guid messageKey);

        /// <summary>
        /// Move <paramref name="messageKey"/> to <paramref name="targetMailboxKey"/>
        /// </summary>
        /// <param name="messageKey">The key of the message to be moved</param>
        /// <param name="sourceMailbox">The source mailbox</param>
        /// <param name="targetMailboxKey">The target mailbox</param>
        /// <param name="copy">True if the message should be duplicated</param>
        /// <returns>The updated mail message</returns>
        MailboxMailMessage MoveMessage(Guid sourceMailbox, Guid messageKey, Guid targetMailboxKey, bool copy = false);

        /// <summary>
        /// Delete the specified message
        /// </summary>
        /// <param name="fromMailbox">The mailbox from which the message should be removed</param>
        /// <param name="messageKey">Delete the specified message key</param>
        /// <returns>The deleted message</returns>
        MailboxMailMessage DeleteMessage(Guid fromMailbox, Guid messageKey);

        /// <summary>
        /// Delete mailbox from current user account
        /// </summary>
        /// <param name="mailboxKey">The key of the mailbox to be deleted</param>
        /// <returns>The deleted mailbox</returns>
        Mailbox DeleteMailbox(Guid mailboxKey);

        /// <summary>
        /// Rename the mailbox <paramref name="mailboxKey"/> to <paramref name="name"/>
        /// </summary>
        /// <param name="mailboxKey">The UUID of the mailbox</param>
        /// <param name="name">The new name of the mailbox</param>
        /// <returns>The updated mailbox</returns>
        Mailbox RenameMailbox(Guid mailboxKey, String name);

        /// <summary>
        /// Update the flag for the specified mail message instance
        /// </summary>
        /// <param name="mailboxKey">The message to be updated</param>
        /// <param name="messageKey">The message key</param>
        /// <param name="statusFlag">The status to set</param>
        /// <returns>The updated mailbox message flag</returns>
        MailboxMailMessage UpdateStatusFlag(Guid mailboxKey, Guid messageKey, MailStatusFlags statusFlag);
    }
}