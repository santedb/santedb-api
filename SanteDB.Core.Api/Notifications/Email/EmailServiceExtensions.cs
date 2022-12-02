using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Notifications.Email
{
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
