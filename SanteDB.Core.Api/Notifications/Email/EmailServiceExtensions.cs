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
 * User: fyfej
 * Date: 2023-6-21
 */
using System.IO;

namespace SanteDB.Core.Notifications.Email
{
    /// <summary>
    /// Extension methods on the email service
    /// </summary>
    public static class EmailServiceExtensions
    {
        /// <summary>
        /// Send an email to a single recipient with a subject and a body from a string.
        /// </summary>
        /// <param name="emailService">The email service to send the email from.</param>
        /// <param name="to">The recipient of the email.</param>
        /// <param name="subject">The subject of the email message.</param>
        /// <param name="body">The body content of the email address.</param>
        public static void SendEmail(this IEmailService emailService, string to, string subject, string body)
            => emailService.SendEmail(new EmailMessage
            {
                ToAddresses = new[] { to },
                Subject = subject,
                Body = body
            });


        /// <summary>
        /// Send an email to a single recipient with a subject and a body from a stream.
        /// </summary>
        /// <param name="emailService">The email service to send the email from.</param>
        /// <param name="to">The recipient of the email.</param>
        /// <param name="subject">The subject of the email message.</param>
        /// <param name="body">The body content of the email address.</param>
        public static void SendEmail(this IEmailService emailService, string to, string subject, Stream body)
            => emailService.SendEmail(new EmailMessage
            {
                ToAddresses = new[] { to },
                Subject = subject,
                Body = body
            });
    }
}
