using SanteDB.Core.Model.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Represents a notification service which manages the sending of a notification
    /// </summary>
    public interface INotificationService : IServiceImplementation
    {

        /// <summary>
        /// Gets the notification relays available (SMS, EMAIL, etc.)
        /// </summary>
        IEnumerable<INotificationRelay> Relays { get; }

        /// <summary>
        /// Gets the specified notification relay
        /// </summary>
        INotificationRelay GetNotificationRelay(Uri toAddress);

        /// <summary>
        /// Gets the specified notification relay
        /// </summary>
        INotificationRelay GetNotificationRelay(String toAddress);

        /// <summary>
        /// Send the message to the specified addresses
        /// </summary>
        Guid[] Send(String[] to, String subject, String body, DateTimeOffset? scheduleDelivery = null, params NotificationAttachment[] attachments);
    }
}
