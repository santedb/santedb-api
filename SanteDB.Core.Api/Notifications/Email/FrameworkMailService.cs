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
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SanteDB.Core.Notifications.Email
{
    /// <summary>
    /// Email sender based on the Smtp client built into the .NET framework.
    /// </summary>
    public class FrameworkMailService : IEmailService
    {
        readonly EmailNotificationConfigurationSection _Configuration;
        readonly string _Host;
        readonly int _Port;
        readonly bool _UseSsl;
        readonly ICredentialsByHost _Credentials;
        readonly Tracer _Tracer;

        /// <summary>
        /// DI constructor
        /// </summary>
        public FrameworkMailService(IConfigurationManager configurationManager)
        {
            _Tracer = new Tracer(nameof(FrameworkMailService));
            _Configuration = configurationManager.GetSection<EmailNotificationConfigurationSection>();
            (_Host, _Port, _UseSsl) = ParseServerString(_Configuration?.Smtp?.Server);

            if (!string.IsNullOrEmpty(_Configuration?.Smtp?.Username))
            {
                _Credentials = new NetworkCredential(_Configuration.Smtp.Username, _Configuration.Smtp.Password);
            }
        }

        private SmtpClient OpenSmtpClient()
        {
            _Tracer.TraceVerbose("Creating SMTP Client for {0} port {1}", _Host, _Port);
            var smtpClient = new SmtpClient()
            {
                Host = _Host,
                Port = _Port,
                EnableSsl = _UseSsl || _Configuration.Smtp.Ssl,
                UseDefaultCredentials = _Credentials == null,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            smtpClient.Credentials = _Credentials;

            // Keep getting disposed object access exceptions - implemented this over in send in a sync manner until we can fix
            //smtpClient.SendCompleted += (sender, e) =>
            //{
            //    if (e.Cancelled)
            //    {
            //        _Tracer.TraceVerbose("Cancelled sending email.");
            //    }
            //    else if (e.Error != null)
            //    {
            //        _Tracer.TraceError("Exception sending email: {0}", e.Error.ToString());
            //    }
            //    else
            //    {
            //        _Tracer.TraceVerbose("Send email success.");
            //    }

            //    if (sender is IDisposable disposable)
            //    {
            //        try
            //        {
            //            disposable.Dispose();
            //        }
            //        catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
            //        {
            //            _Tracer.TraceError("Error when calling dispose on SMTP client instance: {0}", ex.ToString());
            //        }
            //    }
            //};

            return smtpClient;

        }

        /// <summary>
        /// Parse a server string in the form server.domain.com:587 into (server.domain.com, 587)
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        private static (string host, int port, bool ssl) ParseServerString(string server)
        {
            if (string.IsNullOrWhiteSpace(server))
            {
                return (null, 25, false);
            }

            var serverUri = new Uri(server);
            return (serverUri.Host, serverUri.Port, serverUri.Scheme == "smtp+tls");
        }


        /// <inheritdoc/>
        public void SendEmail(EmailMessage message)
        {
            if (null == message)
            {
                throw new ArgumentNullException(nameof(message));
            }

            using (var smtpClient = OpenSmtpClient())
            {
                using (var mm = CreateMailMessage(message))
                {
                    smtpClient.Send(mm); // -> Until the disposed object issue is fixed, (smtpClient, message, mm));
                }
            }
        }

        [DebuggerHidden]
        private void AddEmailAddress(IEnumerable<string> emailAddresses, MailAddressCollection collection)
        {
            if (null != emailAddresses)
            {
                foreach (var email in emailAddresses)
                {
                    try
                    {
                        if (email.StartsWith(Uri.UriSchemeMailto, StringComparison.OrdinalIgnoreCase))
                        {
                            collection.Add(email.Substring(Uri.UriSchemeMailto.Length + 1));
                        }
                        else
                        {
                            collection.Add(email);
                        }
                    }
                    catch (ArgumentException)
                    {
                        _Tracer.TraceWarning("Empty email address will be skipped.");
                    }
                    catch (FormatException)
                    {
                        //TODO: Sanitize and present the email address. 
                        _Tracer.TraceInfo("Invalid email address will be skipped.");
                    }
                }
            }
        }


        private MailMessage CreateMailMessage(EmailMessage message)
        {
            var mm = new MailMessage();

            mm.From = new MailAddress(message.FromAddress ?? _Configuration.Smtp.From);
            mm.ReplyToList.Add(mm.From);

            AddEmailAddress(message.ToAddresses, mm.To);
            AddEmailAddress(message.CcAddresses, mm.CC);
            AddEmailAddress(message.BccAddresses, mm.Bcc);

            mm.Subject = message.Subject;

            mm.Priority = message.HighPriority ? MailPriority.High : MailPriority.Normal;

            if (message.Body is string bodystr)
            {
                mm.Body = bodystr;
            }
            else if (message.Body is Stream stream)
            {
                using (var sr = new StreamReader(stream))
                {
                    mm.Body = sr.ReadToEnd();
                }
            }
            else if (message.Body is byte[] barr)
            {
                mm.Body = Encoding.UTF8.GetString(barr);
            }
            else
            {
                if (null != message.Body)
                {
                    throw new NotSupportedException($"Type of body is not supported. Body Type {message.Body.GetType()}");
                }
            }

            mm.IsBodyHtml = mm.Body?.IndexOf("<html", 0, Math.Min(1024, mm.Body?.Length ?? 0), StringComparison.OrdinalIgnoreCase) != -1;

            if (null != message.Attachments)
            {
                foreach (var attachment in message.Attachments)
                {
                    if (null == attachment.content)
                    {
                        continue;
                    }
                    else if (attachment.content is Stream stream)
                    {
                        mm.Attachments.Add(new Attachment(stream, attachment.name, attachment.contentType));
                    }
                    else if (attachment.content is string str)
                    {
                        mm.Attachments.Add(Attachment.CreateAttachmentFromString(str, attachment.name, Encoding.UTF8, attachment.contentType));
                    }
                    else if (attachment.content is byte[] barr)
                    {
                        mm.Attachments.Add(new Attachment(new MemoryStream(barr), attachment.name, attachment.contentType));
                    }

                }
            }

            return mm;
        }
    }
}
