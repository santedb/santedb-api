using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Notifications.Email
{
    /// <summary>
    /// Core service to send an email through the provider.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send an email message using the configured email service.
        /// </summary>
        /// <param name="message">The email message to send.</param>
        void SendEmail(EmailMessage message);
    }
}
