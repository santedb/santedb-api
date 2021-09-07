/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SanteDB.Core.Notifications.Email
{
    /// <summary>
    /// Notification relay that sends e-mails
    /// </summary>
    public class EmailNotificationRelay : INotificationRelay
    {

        // Configuration for the object
        private EmailNotificationConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<EmailNotificationConfigurationSection>();

        // Get tracer
        private Tracer m_traceSource = Tracer.GetTracer(typeof(EmailNotificationRelay));

        /// <summary>
        /// The scheme of the relay
        /// </summary>
        public string Scheme => "mailto";

        /// <summary>
        /// Send the specified e-mail
        /// </summary>
        public Guid Send(string[] toAddress, string subject, string body,  DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments)
        {
            try
            {
                // Setup message
                MailMessage mailMessage = new MailMessage();
                mailMessage.Sender = new MailAddress(this.m_configuration.Smtp.From);
                toAddress.Select(o => o.Replace("mailto:", "")).ToList().ForEach(o => mailMessage.To.Add(o));

                if(ccAdmins)
                    this.m_configuration.AdministrativeContacts
                        .Where(o=>!mailMessage.To.Contains(new MailAddress(o)))
                        .ToList()
                        .ForEach(o => mailMessage.CC.Add(o));
                mailMessage.Subject = subject;
                mailMessage.Body = body;

                // Attempt to get the security e-mail settings
                var obo = AuthenticationContext.Current.Principal.GetClaimValue(SanteDBClaimTypes.Email);
                if (!String.IsNullOrEmpty(obo))
                {
                    mailMessage.From = new MailAddress(obo, AuthenticationContext.Current.Principal.Identity.Name);
                    mailMessage.ReplyToList.Add(mailMessage.From);
                }
                else
                    mailMessage.From = new MailAddress(this.m_configuration.Smtp.From, AuthenticationContext.Current.Principal.Identity.Name);
                if (body.Contains("<html"))
                    mailMessage.IsBodyHtml = true;
                // Add attachments
                if (attachments != null)
                    foreach (var itm in attachments)
                        mailMessage.Attachments.Add(new Attachment(new MemoryStream(itm.Content), itm.Name, itm.ContentType));

                var serverData = new Uri(this.m_configuration.Smtp.Server);
                SmtpClient smtpClient = new SmtpClient(serverData.Host, serverData.Port);
                smtpClient.UseDefaultCredentials = String.IsNullOrEmpty(this.m_configuration.Smtp.Username);
                smtpClient.EnableSsl = this.m_configuration.Smtp.Ssl;
                if (!(smtpClient.UseDefaultCredentials))
                    smtpClient.Credentials = new NetworkCredential(this.m_configuration.Smtp.Username, this.m_configuration.Smtp.Password);
                smtpClient.SendCompleted += (o, e) =>
                {
                    if (e.Error != null)
                        this.m_traceSource.TraceError("Error sending message to {0} - {1}", mailMessage.To, e.Error.Message);
                    else this.m_traceSource.TraceInfo("Successfully sent message to {0}", mailMessage.To);
                    (o as IDisposable).Dispose();
                };
                this.m_traceSource.TraceInfo("Sending notification email message to {0}", mailMessage.To);
                smtpClient.Send(mailMessage);
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
