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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Mail;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a <see cref="IMailMessageService"/> which uses database persistence layer 
    /// to store / retrieve mail messages within the system
    /// </summary>
    public class LocalMailMessageService : IMailMessageService
    {
        private readonly String[] SYSTEM_MAILBOXES =
        {
            Mailbox.INBOX_NAME,
            Mailbox.DELETED_NAME,
            Mailbox.SENT_NAME
        };

        private readonly SanteDBHostType[] AUTO_CREATE_MAILBOX_HOST_TYPE =
        {
            SanteDBHostType.Server,
            SanteDBHostType.Other,
            SanteDBHostType.Test
        };

        /// <summary>
        /// Interactive mail marker
        /// </summary>
        private struct NeedsDistributionMarker { }

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(LocalMailMessageService));

        private readonly ILocalizationService m_localizationService;
        private readonly IDataPersistenceService<MailMessage> m_mailMessagePersistence;
        private readonly IDataPersistenceService<Mailbox> m_mailboxPersistence;
        private readonly IDataPersistenceService<MailboxMailMessage> m_mailboxMessagePersistence;
        private readonly IDataPersistenceService<Bundle> m_bundlePersistence;

        private readonly IPolicyEnforcementService m_policyEnforcementService;
        private readonly ISecurityRepositoryService m_securityPersistence;
        private readonly IDataPersistenceService<SecurityUser> m_userPersistence;
        private readonly IDataPersistenceService<SecurityDevice> m_devicePersistence;

        /// <inheritdoc/>
        public string ServiceName => "Local Mail Message Manager";

        /// <inheritdoc/>
        public event EventHandler<MailMessageEventArgs> Delivered;
        /// <inheritdoc/>
        public event EventHandler<MailMessageEventArgs> Updated;
        /// <inheritdoc/>
        public event EventHandler<MailMessageEventArgs> Deleted;
        /// <inheritdoc/>
        public event EventHandler<MailMessageEventArgs> Sent;
        /// <inheritdoc/>
        public event EventHandler<DataPersistedEventArgs<Mailbox>> MailboxCreated;
        /// <inheritdoc/>
        public event EventHandler<DataPersistedEventArgs<Mailbox>> MailboxDeleted;

        /// <summary>
        /// DI Constructor
        /// </summary>
        public LocalMailMessageService(ILocalizationService localizationService,
            IDataPersistenceService<MailMessage> mailMessagePersistence,
            IDataPersistenceService<Mailbox> mailboxPersistence,
            IDataPersistenceService<MailboxMailMessage> mailboxMessagePersistence,
            IDataPersistenceService<Bundle> bundlePersistence,
            IDataPersistenceService<SecurityUser> userPersistence,
            IDataPersistenceService<SecurityDevice> devicePersistence,
            ISecurityRepositoryService securityRepositoryService,
            IApplicationServiceContext serviceContext,
            IPolicyEnforcementService pepService)
        {
            this.m_localizationService = localizationService;
            this.m_mailMessagePersistence = mailMessagePersistence;
            this.m_mailboxPersistence = mailboxPersistence;
            this.m_mailboxMessagePersistence = mailboxMessagePersistence;
            this.m_bundlePersistence = bundlePersistence;
            this.m_policyEnforcementService = pepService;
            this.m_securityPersistence = securityRepositoryService;
            this.m_userPersistence = userPersistence;
            this.m_devicePersistence = devicePersistence;


            bundlePersistence.Inserting += (o, e) =>
            {
                // Add any delivery instructions if this is not part of a synchronization
                e.Data.Item.AddRange(e.Data.Item.OfType<MailMessage>().ToArray().SelectMany(m => this.DistributeMail(m, e.Data.Item.OfType<MailboxMailMessage>())));
            };
            mailMessagePersistence.Inserted += (o, e) =>
            {
                try
                {
                    var txBundle = new Bundle(this.DistributeMail(e.Data, e.Data.LoadProperty(x => x.Mailboxes)));
                    this.m_tracer.TraceInfo("Inserting routing instructions for mail {0}...", txBundle);
                    this.m_bundlePersistence.Insert(txBundle, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error distributing mail", ex);
                }
            };
        }

        /// <summary>
        /// Distribute a persisted mail message to the appropriate mailboxes
        /// </summary>
        private IEnumerable<IdentifiedData> DistributeMail(MailMessage mailMessage, IEnumerable<MailboxMailMessage> deliveredMailboxRef)
        {

            // Determine what the object in the RCPT TO is to get a list of individual objects where the mail should be routed 
            var recipients = mailMessage.RcptTo.SelectMany(o =>
            {
                switch (o)
                {
                    case SecurityDevice sd:
                        return new Guid[] { o.Key.Value };
                    case SecurityUser su:
                        return new Guid[] { o.Key.Value };
                    case SecurityRole sr:
                        // HACK: DEVICE is a special role that doesn't have "users" but should encompass all devices
                        if (sr.Name.Equals("DEVICE", StringComparison.OrdinalIgnoreCase))
                        {
                            return this.m_devicePersistence.Query(u => u.ObsoletionTime == null, AuthenticationContext.SystemPrincipal).Select(d => d.Key.Value).ToArray();
                        }
                        else
                        {
                            return this.m_userPersistence.Query(u => u.Roles.Any(r => r.Key == sr.Key), AuthenticationContext.SystemPrincipal).Select(u => u.Key.Value).ToArray();
                        }
                    default:
                        return new Guid[0];
                }
            });

            var deliveredBoxes = mailMessage.LoadProperty(o => o.Mailboxes)?.Union(deliveredMailboxRef).Select(o => o.SourceEntityKey).ToArray();
            var deliveredUsers = this.m_mailboxPersistence.Query(o => deliveredBoxes.Contains(o.Key.Value), AuthenticationContext.SystemPrincipal).Select(o => o.OwnerKey).ToArray();

            // Strip off the mailbox records and persist them in the bundle
            foreach (var itm in mailMessage.Mailboxes)
            {
                itm.BatchOperation = Model.DataTypes.BatchOperationType.InsertOrUpdate;
                yield return itm;
            }


            // Route the mail to inboxes of the recipients
            foreach (var itm in recipients.Distinct())
            {
                var existingMailRecord = this.m_mailboxMessagePersistence.Query(o => o.TargetEntityKey == mailMessage.Key && o.SourceEntity.OwnerKey == itm, AuthenticationContext.SystemPrincipal).Any();
                if (existingMailRecord)
                {
                    this.m_tracer.TraceInfo("Mail message {0} has already been delivered to {1}", mailMessage.Key, itm);
                }
                else
                {
                    this.m_tracer.TraceInfo("Will route mail message {0} to {1}", mailMessage.Key, itm);
                    var inboxMailbox = this.m_mailboxPersistence.Query(o => o.Name == Mailbox.INBOX_NAME && o.OwnerKey == itm && o.ObsoletionTime == null, AuthenticationContext.SystemPrincipal).FirstOrDefault();
                    if (inboxMailbox == null && 
                        AUTO_CREATE_MAILBOX_HOST_TYPE.Contains(ApplicationServiceContext.Current.HostType)
                    )
                    {
                        this.m_tracer.TraceInfo("Setting up Inbox for {0}", itm);
                        inboxMailbox = this.m_mailboxPersistence.Insert(new Mailbox()
                        {
                            Key = Guid.NewGuid(),
                            Name = Mailbox.INBOX_NAME,
                            OwnerKey = itm
                        }, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    }

                    if (!deliveredBoxes.Contains(inboxMailbox.Key.Value))
                    {
                        yield return new MailboxMailMessage()
                        {
                            BatchOperation = Model.DataTypes.BatchOperationType.Insert,
                            SourceEntityKey = inboxMailbox.Key,
                            TargetEntityKey = mailMessage.Key.Value,
                            MailStatusFlag = MailStatusFlags.Unread,
                            DeliveredTime = DateTimeOffset.Now
                        };
                    }
                }
            }

            mailMessage.Mailboxes?.Clear();
        }

        /// <summary>
        /// Get principal SID
        /// </summary>
        private Guid GetPrincipalSidInitialized()
        {
            var mySid = this.m_securityPersistence.GetSid(AuthenticationContext.Current.GetUserIdentity() ?? AuthenticationContext.Current.GetDeviceIdentity());
            this.InitializeMailboxes(AuthenticationContext.Current.GetUserIdentity() ?? AuthenticationContext.Current.GetDeviceIdentity());
            return mySid;
        }

        /// <summary>
        /// Initialize the system mailboxes
        /// </summary>
        public void InitializeMailboxes(IIdentity forIdentity)
        {
            if (forIdentity == null)
            {
                throw new ArgumentNullException(nameof(forIdentity));
            }
            else if (forIdentity is IApplicationIdentity)
            {
                throw new ArgumentOutOfRangeException(nameof(forIdentity));
            }


            using (AuthenticationContext.EnterSystemContext())
            {
                var mySid = this.m_securityPersistence.GetSid(forIdentity);
                if (!this.m_mailboxPersistence.Query(o => o.OwnerKey == mySid, AuthenticationContext.SystemPrincipal).Any() &&
                    AUTO_CREATE_MAILBOX_HOST_TYPE.Contains(ApplicationServiceContext.Current.HostType))
                {
                    this.m_tracer.TraceInfo("Initializing system mailboxes for SID {0}", forIdentity);
                    if (forIdentity is IDeviceIdentity)
                    {
                        this.CreateMailbox(Mailbox.INBOX_NAME, mySid);
                    }
                    else
                    {
                        SYSTEM_MAILBOXES.ForEach(m => this.CreateMailbox(m, mySid));
                        this.Send(UserMessages.WELCOME_SANTEDB, UserMessages.WELCOME_MESSAGE, MailMessageFlags.LowPriority, forIdentity.Name, mySid);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public Mailbox CreateMailbox(string name, Guid? ownerKey = null)
        {
            // If creating for a specific user then must have alter identity 
            if (ownerKey.HasValue)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            var mbx = this.m_mailboxPersistence.Insert(new Mailbox()
            {
                Name = name,
                OwnerKey = ownerKey ?? this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity)
            }, TransactionMode.Commit, AuthenticationContext.Current.Principal);

            if (!SYSTEM_MAILBOXES.Contains(name))
            {
                this.MailboxCreated?.Invoke(this, new DataPersistedEventArgs<Mailbox>(mbx, TransactionMode.Commit, AuthenticationContext.Current.Principal));
            }
            return mbx;
        }

        /// <inheritdoc/>
        public Mailbox DeleteMailbox(Guid mailboxKey)
        {
            var currentUserKey = this.GetPrincipalSidInitialized();
            var mailbox = this.m_mailboxPersistence.Get(mailboxKey, null, AuthenticationContext.Current.Principal);
            if (mailbox == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox).GetSerializationName()}/{mailboxKey}");
            }
            else if (SYSTEM_MAILBOXES.Contains(mailbox.Name))
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.OBJECT_READONLY, mailbox.Name));
            }
            else if (mailbox.OwnerKey != currentUserKey)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }
            // Delete the mailbox
            var retVal = this.m_mailboxPersistence.Delete(mailboxKey, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            this.MailboxDeleted?.Invoke(this, new DataPersistedEventArgs<Mailbox>(mailbox, TransactionMode.Commit, AuthenticationContext.Current.Principal));
            return retVal;
        }

        /// <inheritdoc/>
        public MailboxMailMessage GetMailMessage(Guid mailboxKey, Guid messageKey)
        {
            var mySid = this.GetPrincipalSidInitialized();
            var mailbox = this.m_mailboxPersistence.Get(mailboxKey, null, AuthenticationContext.Current.Principal);
            if (mailbox == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox).GetSerializationName()}/{mailboxKey}");
            }
            else if (mailbox.OwnerKey != mySid)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            using (DataPersistenceControlContext.Create(LoadMode.FullLoad))
            {
                var mailMessage = this.m_mailboxMessagePersistence.Query(o => o.SourceEntityKey == mailboxKey && o.TargetEntityKey == messageKey, AuthenticationContext.Current.Principal).FirstOrDefault();
                if (mailMessage == null)
                {
                    throw new KeyNotFoundException($"{typeof(Mailbox).GetSerializationName()}/{mailboxKey}/{typeof(MailMessage).GetSerializationName()}/{messageKey}");
                }
                return mailMessage;
            }
        }

        /// <inheritdoc/>
        public Mailbox GetMailbox(Guid mailboxKey)
        {
            var mySid = this.GetPrincipalSidInitialized();
            var retVal = this.m_mailboxPersistence.Get(mailboxKey, null, AuthenticationContext.Current.Principal);
            if (retVal == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox).GetSerializationName()}/{mailboxKey}");
            }
            else if (retVal.OwnerKey != mySid)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }
            return retVal;
        }

        /// <inheritdoc/>
        public Mailbox GetMailboxByName(String name)
        {
            var mySid = this.GetPrincipalSidInitialized();
            return this.m_mailboxPersistence.Query(o => o.Name.ToLowerInvariant() == name.ToLowerInvariant() && o.OwnerKey == mySid, AuthenticationContext.Current.Principal).FirstOrDefault();
        }

        /// <inheritdoc/>
        public MailboxMailMessage DeleteMessage(Guid fromMailbox, Guid messageKey)
        {
            // Delete the specified mail message key
            var mySid = this.GetPrincipalSidInitialized();
            var mailbox = this.m_mailboxPersistence.Get(fromMailbox, null, AuthenticationContext.Current.Principal);
            if (mailbox == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox).GetSerializationName()}/{fromMailbox}");
            }
            else if (mailbox.OwnerKey != mySid)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }


            var mailMessageBoxAssoc = this.m_mailboxMessagePersistence.Query(o => o.TargetEntityKey == messageKey && o.SourceEntityKey == fromMailbox, AuthenticationContext.Current.Principal).FirstOrDefault();
            if (mailMessageBoxAssoc == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox).GetSerializationName()}/{fromMailbox}/{typeof(MailMessage).GetSerializationName()}/{messageKey}");
            }

            // Is the mailbox the deleted mailbox?
            if (!Mailbox.DELETED_NAME.Equals(mailbox.Name, StringComparison.OrdinalIgnoreCase)) // MOVE TO DELETED
            {
                var deletedMailbox = this.m_mailboxPersistence.Query(o => o.OwnerKey == mailbox.OwnerKey && o.Name.ToLowerInvariant() == Mailbox.DELETED_NAME.ToLowerInvariant() && o.ObsoletionTime == null, AuthenticationContext.Current.Principal).FirstOrDefault();
                if (deletedMailbox == null && AUTO_CREATE_MAILBOX_HOST_TYPE.Contains(ApplicationServiceContext.Current.HostType)) // Create mailbox
                {
                    deletedMailbox = this.m_mailboxPersistence.Insert(new Mailbox()
                    {
                        OwnerKey = mailbox.OwnerKey,
                        Name = Mailbox.DELETED_NAME
                    }, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                }
                else if (deletedMailbox == null)
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, "Server Must Create DELETED mailbox"));
                }
                return this.MoveMessage(fromMailbox, messageKey, deletedMailbox.Key.Value, false);
            }
            else
            {
                var retVal = this.m_mailboxMessagePersistence.Delete(mailMessageBoxAssoc.Key.Value, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                this.Deleted?.Invoke(this, new MailMessageEventArgs(retVal));
                return retVal;
            }
        }

        /// <inheritdoc/>
        public IQueryResultSet<Mailbox> GetMailboxes(Guid? forUserKey = null)
        {
            if (forUserKey.HasValue)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }
            var thisSid = forUserKey ?? this.GetPrincipalSidInitialized();
            return this.m_mailboxPersistence.Query(o => o.OwnerKey == thisSid && o.ObsoletionTime == null, AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public IQueryResultSet<MailboxMailMessage> GetMessages(Guid mailboxKey)
        {
            // Only administrators are permitted to read other people's mail
            var thisSid = this.GetPrincipalSidInitialized();
            var mailbox = this.m_mailboxPersistence.Get(mailboxKey, null, AuthenticationContext.Current.Principal);
            if (mailbox == null)
            {
                throw new KeyNotFoundException($"${typeof(MailboxMailMessage).GetSerializationName()}/{mailboxKey}");
            }
            else if (mailbox.OwnerKey != thisSid)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            return this.m_mailboxMessagePersistence.Query(o => o.SourceEntityKey == mailboxKey && o.SourceEntity.OwnerKey == thisSid, AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public MailboxMailMessage MoveMessage(Guid sourceMailboxKey, Guid messageKey, Guid targetMailboxKey, bool copy = false)
        {
            // Move a mail message to another mailbox
            var mySid = this.GetPrincipalSidInitialized();
            var targetMailbox = this.m_mailboxPersistence.Get(targetMailboxKey, null, AuthenticationContext.Current.Principal);
            var sourceMailbox = this.m_mailboxPersistence.Get(sourceMailboxKey, null, AuthenticationContext.Current.Principal);
            if (sourceMailbox == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox)}/{sourceMailboxKey}");
            }
            else if (targetMailbox == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox)}/{targetMailboxKey}");
            }
            else if (sourceMailbox.OwnerKey != mySid || targetMailbox.OwnerKey != mySid || sourceMailbox.OwnerKey != targetMailbox.OwnerKey)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            var sourceMessage = this.m_mailboxMessagePersistence.Query(o => o.TargetEntityKey == messageKey && o.SourceEntityKey == sourceMailboxKey, AuthenticationContext.Current.Principal).SingleOrDefault();
            if (sourceMessage == null)
            {
                throw new KeyNotFoundException(messageKey.ToString());
            }

            var transaction = new Bundle();

            // Are we copying?
            if (!copy)
            {
                sourceMessage.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
                transaction.Add(sourceMessage);
            }

            transaction.Add(new MailboxMailMessage()
            {
                BatchOperation = Model.DataTypes.BatchOperationType.Insert,
                MailStatusFlag = sourceMessage.MailStatusFlag,
                SourceEntityKey = targetMailboxKey,
                TargetEntityKey = sourceMessage.TargetEntityKey,
                DeliveredTime = sourceMessage.DeliveredTime
            });

            var retVal = this.m_bundlePersistence.Insert(transaction, TransactionMode.Commit, AuthenticationContext.Current.Principal);

            this.Updated?.Invoke(this, new MailMessageEventArgs(retVal.Item.OfType<MailboxMailMessage>().ToArray()));
            return retVal.Item.OfType<MailboxMailMessage>().First(o => o.BatchOperation != Model.DataTypes.BatchOperationType.Delete);
        }

        /// <inheritdoc/>
        public MailMessage Send(MailMessage mailMessage, Guid? fromKey = null)
        {
            if (mailMessage == null)
            {
                throw new ArgumentNullException(nameof(mailMessage), ErrorMessages.ARGUMENT_NULL);
            }
            else if (!mailMessage.RcptToXml.Any())
            {
                throw new InvalidOperationException(ErrorMessages.MAIL_MISISNG_TO);
            }
            else if (fromKey.HasValue)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            try
            {

                var mySid = fromKey ?? this.GetPrincipalSidInitialized();
                mailMessage.FromKey = mySid;
                mailMessage.Key = mailMessage.Key ?? Guid.NewGuid();
                mailMessage.FromInfo = mailMessage.FromInfo ?? AuthenticationContext.Current.Principal.Identity.Name;
                mailMessage.ToInfo = mailMessage.ToInfo ?? String.Join(";", mailMessage.RcptTo.Select(o =>
                {
                    switch (o)
                    {
                        case SecurityDevice sd:
                            return sd.Name;
                        case SecurityUser su:
                            var displayName = su.LoadProperty(x => x.UserEntity)?.Names.FirstOrDefault()?.ToDisplay();
                            if (String.IsNullOrEmpty(displayName))
                            {
                                return su.UserName;
                            }
                            else
                            {
                                return $"{displayName} <{su.UserName}>";
                            }
                        case SecurityRole sr:
                            return sr.Name;
                        default:
                            return o.ToDisplay();
                    }
                }));
                // Now we construct the mail message meta-data and place into the relevant inboxes
                var txBundle = new Bundle();
                mailMessage.Key = mailMessage.Key ?? Guid.NewGuid();
                txBundle.Add(mailMessage);

                // Get the SENT folder for the user
                var sentMailbox = this.m_mailboxPersistence.Query(o => o.Name == Mailbox.SENT_NAME && o.OwnerKey == mySid, AuthenticationContext.SystemPrincipal).FirstOrDefault();
                if (sentMailbox == null)
                {
                    sentMailbox = this.m_mailboxPersistence.Insert(new Mailbox()
                    {
                        Key = Guid.NewGuid(),
                        Name = Mailbox.SENT_NAME,
                        OwnerKey = mySid
                    }, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                }
                txBundle.Add(new MailboxMailMessage() { TargetEntityKey = mailMessage.Key.Value, SourceEntityKey = sentMailbox.Key, DeliveredTime = DateTimeOffset.Now, MailStatusFlag = MailStatusFlags.Read });

                // Indicate that this needs to be distributed
                txBundle = this.m_bundlePersistence.Insert(txBundle, TransactionMode.Commit, AuthenticationContext.Current.Principal);

                if (AuthenticationContext.Current.Principal != AuthenticationContext.SystemPrincipal)
                {
                    txBundle.Item.OfType<MailboxMailMessage>().ForEach(i =>
                    {
                        if (i.SourceEntityKey == sentMailbox.Key)
                        {
                            this.Sent?.Invoke(this, new MailMessageEventArgs(i));
                        }
                        else
                        {
                            this.Delivered?.Invoke(this, new MailMessageEventArgs(i));
                        }
                    });
                }

                return mailMessage;
            }
            catch (Exception e)
            {
                throw new DataPersistenceException(ErrorMessages.MAIL_ERROR_SENDING, e);
            }
        }

        /// <inheritdoc/>
        public MailboxMailMessage UpdateStatusFlag(Guid mailboxKey, Guid messageKey, MailStatusFlags statusFlag)
        {
            var mySid = this.GetPrincipalSidInitialized();
            var mailbox = this.m_mailboxPersistence.Get(mailboxKey, null, AuthenticationContext.Current.Principal);
            if (mailbox == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox)}/{mailboxKey}");
            }
            else if (mailbox.OwnerKey != mySid)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            var mailMessageItem = this.m_mailboxMessagePersistence.Query(o => o.TargetEntityKey == messageKey && o.SourceEntityKey == mailboxKey, AuthenticationContext.Current.Principal).FirstOrDefault();
            if (mailMessageItem == null)
            {
                throw new KeyNotFoundException($"{typeof(Mailbox).GetSerializationName()}/{mailboxKey}/{typeof(MailMessage).GetSerializationName()}/{messageKey}");
            }

            mailMessageItem.MailStatusFlag = statusFlag;
            var retVal = this.m_mailboxMessagePersistence.Update(mailMessageItem, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            this.Updated?.Invoke(this, new MailMessageEventArgs(retVal));
            return retVal;
        }

        /// <inheritdoc/>
        public MailMessage Send(string subject, string body, MailMessageFlags flags, String toLine, params Guid[] recipients)
        {
            if (String.IsNullOrEmpty(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }
            else if (String.IsNullOrEmpty(body))
            {
                throw new ArgumentNullException(nameof(body));
            }
            else if (!recipients.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(recipients));
            }

            var mySid = this.GetPrincipalSidInitialized();
            return this.Send(new MailMessage(mySid, recipients, subject, body, flags)
            {
                ToInfo = toLine
            });
        }

        /// <inheritdoc/>
        public Mailbox RenameMailbox(Guid mailboxKey, string name)
        {
            var mySid = this.GetPrincipalSidInitialized();
            var mailbox = this.GetMailbox(mailboxKey);
            if (mailbox.OwnerKey != mySid)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            mailbox.Name = name;
            return this.m_mailboxPersistence.Update(mailbox, TransactionMode.Commit, AuthenticationContext.Current.Principal);
        }
    }
}
