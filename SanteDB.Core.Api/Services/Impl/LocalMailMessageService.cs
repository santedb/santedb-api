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
using System.Text;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a <see cref="IMailMessageService"/> which uses database persistence layer 
    /// to store / retrieve mail messages within the system
    /// </summary>
    public class LocalMailMessageService : IMailMessageService
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
            if(ownerKey.HasValue)
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
        public Mailbox DeleteMailbox(Guid mailboxKey)
        {
            var mailbox = this.m_mailboxPersistence.Get(mailboxKey, null, AuthenticationContext.Current.Principal);
            var currentUserKey = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            if(mailbox.OwnerKey != currentUserKey)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            // Delete the mailbox
            return this.m_mailboxPersistence.Delete(mailboxKey, TransactionMode.Commit, AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public MailboxMailMessage DeleteMessage(Guid fromMailboxKey, Guid messageKey)
        {
            // Delete the specified mail message key
            var mySid = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            var mailMessageBoxAssoc = this.m_mailboxMessagePersistence.Query(o => o.TargetKey == messageKey && o.SourceEntityKey == fromMailboxKey && o.SourceEntity.OwnerKey == mySid, AuthenticationContext.Current.Principal).FirstOrDefault();
            if(mailMessageBoxAssoc == null)
            {
                throw new KeyNotFoundException(messageKey.ToString());
            }

            return this.m_mailboxMessagePersistence.Delete(mailMessageBoxAssoc.Key.Value, TransactionMode.Commit, AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public IQueryResultSet<Mailbox> GetMailboxes(Guid? forUserKey = null)
        {
            if(forUserKey.HasValue)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }
            var thisSid = forUserKey ?? this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            return this.m_mailboxPersistence.Query(o => o.OwnerKey == thisSid && o.ObsoletionTime == null, AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public IQueryResultSet<MailMessage> GetMessages(Guid mailboxKey)
        {
            // Only administrators are permitted to read other people's mail
            if(this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity) != 
                this.m_mailboxPersistence.Get(mailboxKey, null, AuthenticationContext.Current.Principal).OwnerKey)
            {
                this.m_policyEnforcementService.Demand(PermissionPolicyIdentifiers.ManageMail);
            }

            return this.m_mailMessagePersistence.Query(o => o.Mailboxes.Any(b => b.SourceEntityKey == mailboxKey), AuthenticationContext.Current.Principal);
        }

        /// <inheritdoc/>
        public MailboxMailMessage MoveMessage(Guid fromMailboxKey, Guid messageKey, Guid targetMailboxKey, bool copy = false)
        {
            // Move a mail message to another mailbox
            var mySid = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            var sourceMessage = this.m_mailboxMessagePersistence.Query(o => o.TargetKey == messageKey && o.SourceEntity.OwnerKey == mySid && o.SourceEntityKey == fromMailboxKey, AuthenticationContext.Current.Principal).FirstOrDefault();
            if(sourceMessage == null)
            {
                throw new KeyNotFoundException(messageKey.ToString());
            }
            var sourceMailbox = sourceMessage.LoadProperty(o => o.SourceEntity);

            var targetMailbox = this.m_mailboxPersistence.Get(targetMailboxKey, null, AuthenticationContext.Current.Principal);
            if(targetMailbox == null)
            {
                throw new KeyNotFoundException(targetMailboxKey.ToString());
            }

            // Ensure the mailboxes are the same owner
            if(sourceMailbox.OwnerKey != targetMailbox.OwnerKey)
            {
                throw new InvalidOperationException(ErrorMessages.MAIL_CANNOT_MOVE_OWNERS);
            }

            // Move or copy 
            if (copy)
            {
                return this.m_mailboxMessagePersistence.Insert(new MailboxMailMessage()
                {
                    TargetKey = messageKey,
                    SourceEntityKey = targetMailboxKey
                }, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            }
            else
            {
                sourceMessage.SourceEntityKey = targetMailboxKey;
                return this.m_mailboxMessagePersistence.Update(sourceMessage, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            }

        }

        /// <inheritdoc/>
        public MailMessage Send(MailMessage mail)
        {
            if(String.IsNullOrEmpty(mail.To) && mail.RcptTo?.Any() != true)
            {
                throw new InvalidOperationException(ErrorMessages.MAIL_MISISNG_TO);
            }

            try
            {
                // We want to route via the RCPT to if available 
                if(mail.RcptTo?.Any() != true)
                {
                    mail.RcptToXml = mail.To.Split(';').Distinct().Where(o => !String.IsNullOrEmpty(o)).Select(o => this.m_securityPersistence.GetUser(o).Key.Value).ToList();
                }

                var fromUser = this.m_securityPersistence.GetUser(AuthenticationContext.Current.Principal.Identity);
                mail.From = fromUser.UserName;

                // Now we construct the mail message meta-data and place into the relevant inboxes
                var txBundle = new Bundle();
                mail.Key = mail.Key ?? Guid.NewGuid();
                txBundle.Add(mail);

                // Get the SENT folder for the user
                var sentMailbox = this.m_mailboxPersistence.Query(o => o.Name == Mailbox.SENT_NAME && o.OwnerKey == fromUser.Key, AuthenticationContext.SystemPrincipal).FirstOrDefault();
                if(sentMailbox == null)
                {
                    sentMailbox = this.m_mailboxPersistence.Insert(new Mailbox()
                    {
                        Key = Guid.NewGuid(),
                        Name = Mailbox.SENT_NAME,
                        OwnerKey = fromUser.Key.Value
                    }, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                }
                txBundle.Add(new MailboxMailMessage() { TargetKey = mail.Key.Value, SourceEntityKey = sentMailbox.Key });

                // Route the mail to inboxes of the recipients
                foreach(var itm in mail.RcptToXml)
                {
                    var inboxMailbox = this.m_mailboxPersistence.Query(o => o.Name == Mailbox.INBOX_NAME && o.OwnerKey == itm && o.ObsoletionTime == null, AuthenticationContext.SystemPrincipal).FirstOrDefault();
                    if(inboxMailbox == null)
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
                        Flags = MailStatusFlags.Unread
                    });
                }

                this.m_bundlePersistence.Insert(txBundle, TransactionMode.Commit, AuthenticationContext.Current.Principal);

                this.Sent?.Invoke(this, new MailMessageEventArgs(mail));
                return mail;
            }
            catch(Exception e)
            {
                throw new DataPersistenceException(ErrorMessages.MAIL_ERROR_SENDING, e);
            }
        }

        /// <inheritdoc/>
        public MailboxMailMessage UpdateFlag(Guid mailMessageKey, MailStatusFlags statusFlag)
        {
            var mySid = this.m_securityPersistence.GetSid(AuthenticationContext.Current.Principal.Identity);
            var mailMessageItem = this.m_mailboxMessagePersistence.Query(o => o.TargetKey == mailMessageKey && o.SourceEntity.OwnerKey == mySid, AuthenticationContext.Current.Principal).FirstOrDefault();
            if(mailMessageItem == null)
            {
                throw new KeyNotFoundException(mailMessageKey.ToString());
            }

            mailMessageItem.Flags = statusFlag;
            return this.m_mailboxMessagePersistence.Update(mailMessageItem, TransactionMode.Commit, AuthenticationContext.Current.Principal);

        }
    }
}
