﻿using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
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
        public Guid Send(string[] toAddress, string subject, string body, DateTimeOffset? scheduleDelivery = null, params NotificationAttachment[] attachments)
        {
            try
            {
                // Setup message
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(this.m_configuration.Smtp.From);
                toAddress.Select(o => o.Replace("mailto:", "")).ToList().ForEach(o => mailMessage.To.Add(o));
                this.m_configuration.AdministrativeContacts
                    .Where(o=>!mailMessage.To.Contains(new MailAddress(o)))
                    .ToList()
                    .ForEach(o => mailMessage.CC.Add(o));
                mailMessage.Subject = subject;
                mailMessage.Body = body;

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
