/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Mail;
using SanteDB.Core.Model.Query;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which handles the mailbox services.
    /// </summary>
    [System.ComponentModel.Description("Mail Repository Provider")]
    public interface IMailMessageService : IServiceImplementation
    {
        /// <summary>
        /// Fired when an mail message has been received.
        /// </summary>
        event EventHandler<MailMessageEventArgs> Sent;

        /// <summary>
        /// Send the specified mailmessage according to its sending instructions
        /// </summary>
        /// <param name="mail">The mail message to be sent</param>
        MailMessage Send(MailMessage mail);

        /// <summary>
        /// Get mailboxes for the current user
        /// </summary>
        IQueryResultSet<Mailbox> GetMailboxes(Guid? forUserKey = null);

        /// <summary>
        /// Get a specific mailbox
        /// </summary>
        /// <param name="mailboxName">The name of the mailbox</param>
        /// <returns>The mailbox</returns>
        Mailbox GetMailbox(String mailboxName);

        /// <summary>
        /// Create a new mailbox for the specified user
        /// </summary>
        /// <param name="name">The name of the mailbox to be created</param>
        /// <param name="ownerKey">The key of the user that should own the mailbox</param>
        /// <returns>The created mailbox</returns>
        Mailbox CreateMailbox(String name, Guid? ownerKey = null);

        /// <summary>
        /// Get messages from the mailbox <paramref name="mailboxName"/>
        /// </summary>
        /// <param name="mailboxName">The mailbox key to fetch messages for</param>
        /// <returns>The messages</returns>
        IQueryResultSet<MailboxMailMessage> GetMessages(String mailboxName);

        /// <summary>
        /// Move <paramref name="messageKey"/> to <paramref name="targetMailboxName"/>
        /// </summary>
        /// <param name="messageKey">The key of the message to be moved</param>
        /// <param name="targetMailboxName">The target mailbox</param>
        /// <param name="copy">True if the message should be duplicated</param>
        /// <returns>The updated mail message</returns>
        MailboxMailMessage MoveMessage(Guid messageKey, String targetMailboxName, bool copy = false);

        /// <summary>
        /// Delete the specified message
        /// </summary>
        /// <param name="fromMailboxName">The mailbox from which the message should be removed</param>
        /// <param name="messageKey">Delete the specified message key</param>
        /// <returns>The deleted message</returns>
        MailboxMailMessage DeleteMessage(String fromMailboxName, Guid messageKey);

        /// <summary>
        /// Delete mailbox from current user account
        /// </summary>
        /// <param name="fromMailboxName">The key of the mailbox to be deleted</param>
        /// <param name="ownerKey">The owner of the mailbox</param>
        /// <returns>The deleted mailbox</returns>
        Mailbox DeleteMailbox(String fromMailboxName, Guid? ownerKey = null);

        /// <summary>
        /// Update the flag for the specified mail message instance
        /// </summary>
        /// <param name="mailMessageKey">The message to be updated</param>
        /// <param name="statusFlag">The status to set</param>
        /// <returns>The updated mailbox message flag</returns>
        MailboxMailMessage UpdateStatusFlag(Guid mailMessageKey, MailStatusFlags statusFlag);
    }
}