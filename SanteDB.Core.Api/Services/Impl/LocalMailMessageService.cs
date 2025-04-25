/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Mail;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a <see cref="IMailMessageService"/> which uses database persistence layer 
    /// to store / retrieve mail messages within the system
    /// </summary>
    public class LocalMailMessageService : IMailMessageService, IRepositoryService<MailMessage>
    {
        private readonly ILocalizationService m_localizationService;
        private readonly IDataPersistenceService<MailMessage> m_mailMessagePersistence;
        private readonly IDataPersistenceService<Mailbox> m_mailboxPersistence;
        private readonly IDataPersistenceService<MailboxMailMessage> m_mailboxMessagePersistence;
        private readonly IDataPersistenceService<Bundle> m_bundlePersistence;

        private readonly IPolicyEnforcementService m_policyEnforcementService;
        private readonly ISecurityRepositoryService m_securityPersistence;

        /// <inheritdoc/>
        public string ServiceName => "Local Mail Message Manager";

        /// <inheritdoc/>
        public event EventHandler<MailMessageEventArgs> Sent;

        /// <summary>
        /// DI Constructor
        /// </summary>
        public LocalMailMessageService(ILocalizationService localizationService,
            IDataPersistenceService<MailMessage> mailMessagePersistence,
            IDataPersistenceService<Mailbox> mailboxPersistence,
            IDataPersistenceService<MailboxMailMessage> mailboxMessagePersistence,
            IDataPersistenceService<Bundle> bundlePersistence,
            ISecurityRepositoryService securityRepositoryService,
            IPolicyEnforcementService pepService)
        {
            this.m_localizationService = localizationService;
            this.m_mailMessagePersistence = mailMessagePersistence;
            this.m_mailboxPersistence = mailboxPersistence;
            this.m_mailboxMessagePersistence = mailboxMessagePersistence;
            this.m_bundlePersistence = bundlePersistence;

            this.m_policyEnforcementService = pepService;
            this.m_securityPersistence = securityRepositoryService;
        }

        /// <inheritdoc/>
        public Mailbox CreateMailbox(string name, Guid? ownerKey = null)
        {
            // If creating for a specific user then must have alter identity 
            if (ownerKey.HasValue)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            return this.m_mailboxPersistence.Insert(new Mailbox()
            {
                Name = name,
                OwnerKey = ownerKey ?? this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity)
            }, TransactionMode.Commit, AuthenticationContext.Current.Principal);

        }

        /// <inheritdoc/>
        public Mailbox DeleteMailbox(String mailboxName, Guid? ownerKey = null)
        {
            var currentUserKey = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            if (ownerKey.HasValue && ownerKey != currentUserKey)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }
            else
            {
                ownerKey = currentUserKey;
            }

            var mailbox = this.m_mailboxPersistence.Query(o => o.Name.ToLowerInvariant() == mailboxName.ToLowerInvariant() && o.OwnerKey == ownerKey, AuthenticationContext.Current.Principal).First();
            if (mailbox == null)
            {
                throw new KeyNotFoundException(mailboxName);
            }
            // Delete the mailbox
            return this.m_mailboxPersistence.Delete(mailbox.Key.Value, TransactionMode.Commit, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Get a mailbox by identifier
        /// </summary>
        public Mailbox GetMailbox(String mailboxName)
        {
            var mySid = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            return this.m_mailboxPersistence.Query(o => o.Name.ToLowerInvariant() == mailboxName.ToLowerInvariant() && o.OwnerKey == mySid, AuthenticationContext.Current.Principal).FirstOrDefault();
        }

        /// <inheritdoc/>
        public MailboxMailMessage DeleteMessage(String fromMailboxName, Guid messageKey)
        {
            // Delete the specified mail message key
            var mySid = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            var mailMessageBoxAssoc = this.m_mailboxMessagePersistence.Query(o => o.Key == messageKey && o.SourceEntity.Name.ToLowerInvariant() == fromMailboxName.ToLowerInvariant() && o.SourceEntity.OwnerKey == mySid, AuthenticationContext.Current.Principal).FirstOrDefault();
            if (mailMessageBoxAssoc == null)
            {
                throw new KeyNotFoundException(messageKey.ToString());
            }
            return this.m_mailboxMessagePersistence.Delete(mailMessageBoxAssoc.Key.Value, TransactionMode.Commit, AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public IQueryResultSet<Mailbox> GetMailboxes(Guid? forUserKey = null)
        {
            if (forUserKey.HasValue)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }
            var thisSid = forUserKey ?? this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            return this.m_mailboxPersistence.Query(o => o.OwnerKey == thisSid && o.ObsoletionTime == null, AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public IQueryResultSet<MailboxMailMessage> GetMessages(String mailboxName)
        {
            // Only administrators are permitted to read other people's mail
            var thisSid = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            return this.m_mailboxMessagePersistence.Query(o => o.SourceEntity.Name.ToLowerInvariant() == mailboxName.ToLowerInvariant() && o.SourceEntity.OwnerKey == thisSid, AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public MailboxMailMessage MoveMessage(Guid messageKey, String targetMailboxName, bool copy = false)
        {
            // Move a mail message to another mailbox
            var mySid = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            var sourceMessage = this.m_mailboxMessagePersistence.Query(o => o.Key == messageKey && o.SourceEntity.OwnerKey == mySid, AuthenticationContext.Current.Principal).SingleOrDefault();
            if (sourceMessage == null)
            {

                throw new KeyNotFoundException(messageKey.ToString());
            }
            var targetMailbox = this.m_mailboxPersistence.Query(o => o.Name.ToLowerInvariant() == targetMailboxName.ToLowerInvariant() && o.OwnerKey == mySid, AuthenticationContext.Current.Principal).SingleOrDefault();
            if (targetMailbox == null)
            {
                switch (targetMailboxName)
                {
                    case Mailbox.INBOX_NAME:
                    case Mailbox.DELTEED_NAME:
                    case Mailbox.SENT_NAME:
                        targetMailbox = this.m_mailboxPersistence.Insert(new Mailbox()
                        {
                            Key = Guid.NewGuid(),
                            Name = targetMailboxName,
                            OwnerKey = mySid
                        }, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                        break;
                    default:
                        throw new KeyNotFoundException(targetMailboxName);
                }
            }

            // Move or copy 
            if (copy)
            {
                return this.m_mailboxMessagePersistence.Insert(new MailboxMailMessage()
                {
                    TargetKey = sourceMessage.TargetKey,
                    SourceEntityKey = targetMailbox.Key
                }, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            }
            else
            {
                sourceMessage.SourceEntityKey = targetMailbox.Key;
                return this.m_mailboxMessagePersistence.Update(sourceMessage, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            }

        }

        /// <inheritdoc/>
        public MailMessage Send(MailMessage mail)
        {
            if (String.IsNullOrEmpty(mail.To) && mail.RcptTo?.Any() != true)
            {
                throw new InvalidOperationException(ErrorMessages.MAIL_MISISNG_TO);
            }

            try
            {
                // We want to route via the RCPT to if available 
                if (mail.RcptTo?.Any() != true)
                {
                    mail.RcptToXml = mail.To.Split(';').Distinct().Where(o => !String.IsNullOrEmpty(o)).Select(o => this.m_securityPersistence.GetUser(o).Key.Value).ToList();
                }

                var fromUser = AuthenticationContext.Current.Principal.Identity;
                mail.From = fromUser.Name;

                // Now we construct the mail message meta-data and place into the relevant inboxes
                var txBundle = new Bundle();
                mail.Key = mail.Key ?? Guid.NewGuid();
                txBundle.Add(mail);

                // Get the SENT folder for the user
                var sentMailbox = this.m_mailboxPersistence.Query(o => o.Name == Mailbox.SENT_NAME && o.Owner.UserName.ToLowerInvariant() == fromUser.Name.ToLowerInvariant(), AuthenticationContext.SystemPrincipal).FirstOrDefault();
                if (sentMailbox == null)
                {
                    sentMailbox = this.m_mailboxPersistence.Insert(new Mailbox()
                    {
                        Key = Guid.NewGuid(),
                        Name = Mailbox.SENT_NAME,
                        OwnerKey = this.m_securityPersistence.GetSid(fromUser)
                    }, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                }
                txBundle.Add(new MailboxMailMessage() { TargetKey = mail.Key.Value, SourceEntityKey = sentMailbox.Key });

                // Route the mail to inboxes of the recipients
                foreach (var itm in mail.RcptToXml)
                {
                    var inboxMailbox = this.m_mailboxPersistence.Query(o => o.Name == Mailbox.INBOX_NAME && o.OwnerKey == itm && o.ObsoletionTime == null, AuthenticationContext.SystemPrincipal).FirstOrDefault();
                    if (inboxMailbox == null)
                    {
                        inboxMailbox = this.m_mailboxPersistence.Insert(new Mailbox()
                        {
                            Key = Guid.NewGuid(),
                            Name = Mailbox.INBOX_NAME,
                            OwnerKey = itm
                        }, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    }
                    txBundle.Add(new MailboxMailMessage()
                    {
                        SourceEntityKey = inboxMailbox.Key,
                        TargetKey = mail.Key.Value,
                        MailStatusFlag = MailStatusFlags.Unread
                    });
                }

                this.m_bundlePersistence.Insert(txBundle, TransactionMode.Commit, AuthenticationContext.Current.Principal);

                this.Sent?.Invoke(this, new MailMessageEventArgs(mail));
                return mail;
            }
            catch (Exception e)
            {
                throw new DataPersistenceException(ErrorMessages.MAIL_ERROR_SENDING, e);
            }
        }

        /// <inheritdoc/>
        public MailboxMailMessage UpdateStatusFlag(Guid mailMessageKey, MailStatusFlags statusFlag)
        {
            var mailMessageItem = this.m_mailboxMessagePersistence.Query(o => o.TargetKey == mailMessageKey && o.SourceEntity.Owner.UserName.ToLowerInvariant() == AuthenticationContext.Current.Principal.Identity.Name.ToLowerInvariant(), AuthenticationContext.Current.Principal).FirstOrDefault();
            if (mailMessageItem == null)
            {
                throw new KeyNotFoundException(mailMessageKey.ToString());
            }

            mailMessageItem.MailStatusFlag = statusFlag;
            return this.m_mailboxMessagePersistence.Update(mailMessageItem, TransactionMode.Commit, AuthenticationContext.Current.Principal);

        }

        /// <inheritdoc/>
        public MailMessage Get(Guid key)
        {
            // Does the user have permission to read any mail?
            if (this.m_policyEnforcementService.SoftDemand(PermissionPolicyIdentifiers.ManageMail, AuthenticationContext.Current.Principal))
            {
                return this.m_mailMessagePersistence.Get(key, null, AuthenticationContext.Current.Principal);
            }
            else
            {
                return this.m_mailMessagePersistence.Query(o => o.Key == key && o.Mailboxes.Any(m => m.SourceEntity.Owner.UserName.ToLowerInvariant() == AuthenticationContext.Current.Principal.Identity.Name.ToLowerInvariant()) && o.ObsoletionTime == null, AuthenticationContext.Current.Principal).FirstOrDefault();
            }
        }

        /// <inheritdoc/>
        public MailMessage Get(Guid key, Guid versionKey) => this.Get(key);

        /// <inheritdoc/>
        public IQueryResultSet<MailMessage> Find(Expression<Func<MailMessage, bool>> query)
        {
            if (this.m_policyEnforcementService.SoftDemand(PermissionPolicyIdentifiers.ManageMail, AuthenticationContext.Current.Principal))
            {
                return this.m_mailMessagePersistence.Query(query, AuthenticationContext.Current.Principal);
            }
            else
            {
                Expression<Func<MailboxMailMessage, bool>> mailMessageBoxFilter = o => o.SourceEntity.Owner.UserName.ToLowerInvariant() == AuthenticationContext.Current.Principal.Identity.Name.ToLowerInvariant();
                var newQuery = Expression.Lambda<Func<MailMessage, bool>>(
                    Expression.And(query.Body, Expression.Call(
                        null,
                        (MethodInfo)typeof(Enumerable).GetGenericMethod(nameof(Enumerable.Any), new Type[] { typeof(MailboxMailMessage) }, new Type[] { typeof(IEnumerable<MailboxMailMessage>), typeof(Func<MailboxMailMessage, bool>) }),
                        Expression.MakeMemberAccess(query.Parameters[0], typeof(MailMessage).GetProperty(nameof(MailMessage.Mailboxes))),
                        Expression.Constant(mailMessageBoxFilter)
                    )), query.Parameters[0]);
                return this.m_mailMessagePersistence.Query(newQuery, AuthenticationContext.Current.Principal);
            }
        }


        /// <inheritdoc/>
        public MailMessage Insert(MailMessage data) => this.Send(data);

        /// <inheritdoc/>
        public MailMessage Save(MailMessage data)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public MailMessage Delete(Guid key)
        {
            return this.MoveMessage(key, Mailbox.DELTEED_NAME).LoadProperty(o => o.Target);
        }
    }
}
