/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Notifications.Email
{
    /// <summary>
    /// Notification relay that sends e-mails
    /// </summary>
    public class EmailNotificationRelay : INotificationRelay
    {
        readonly Tracer m_traceSource;
        readonly EmailNotificationConfigurationSection m_configuration;
        readonly IEmailService m_emailService;

        /// <summary>
        /// DI constructor
        /// </summary>
        public EmailNotificationRelay(IConfigurationManager configurationManager, IEmailService emailService)
        {
            m_traceSource = Tracer.GetTracer(typeof(EmailNotificationRelay));
            m_configuration = configurationManager.GetSection<EmailNotificationConfigurationSection>();
            m_emailService = emailService;
        }

        // Configuration for the object


        /// <inheritdoc/>
        public IEnumerable<string> SupportedSchemes => new[] { Uri.UriSchemeMailto };

        /// <summary>
        /// Send the specified e-mail
        /// </summary>
        public Guid Send(string[] toAddress, string subject, string body, DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments)
        {
            try
            {
                var principal = AuthenticationContext.Current.Principal;

                if (principal.Identity.IsAuthenticated == false)
                {
                    principal = null;
                }

                var fromAddress = principal?.GetClaimValue(SanteDBClaimTypes.Email);
                var fromName = principal?.Identity?.Name;

                m_emailService.SendEmail(new EmailMessage
                {
                    ToAddresses = toAddress,
                    CcAddresses = ccAdmins ? m_configuration.AdministrativeContacts.Where(ac => !toAddress.Contains(ac)) : null,
                    Subject = subject,
                    Body = body,
                    Attachments = attachments.Select(att => (att.Name, att.ContentType, (object)att.Content)),
                    FromAddress = null != fromAddress ? $"{fromName} <{fromAddress}>" : null
                });

                return Guid.Empty;
            }
            catch (Exception ex)
            {
                this.m_traceSource.TraceError("Error sending notification: {0}", ex);
                throw;
            }
        }
    }
}