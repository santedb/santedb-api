using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// A particular relay of messages (e-mail, SMS, etc.) 
    /// </summary>
    public interface INotificationRelay
    {
        /// <summary>
        /// Gets the telecommunications scheme that this relay can handle
        /// </summary>
        String Scheme { get; }

        /// <summary>
        /// Send the specified notification to the specified address
        /// </summary>
        /// <param name="toAddress">The address where the notification is to be sent</param>
        /// <param name="subject">The subject of the message</param>
        /// <param name="body">The body of the message</param>
        /// <param name="scheduleDelivery">The time when the message should be sent (for future delivery)</param>
        /// <param name="attachments">Attachment file and content</param>
        Guid Send(String[] toAddress, String subject, String body, DateTimeOffset? scheduleDelivery = null, params NotificationAttachment[] attachments);

    }

    /// <summary>
    /// Represents a notification attachment
    /// </summary>
    public class NotificationAttachment
    {

        /// <summary>
        /// Create a new attachment 
        /// </summary>
        public NotificationAttachment(String name, String contentType, byte[] content)
        {
            this.Content = content;
            this.ContentType = contentType;
            this.Name = name;
        }

        /// <summary>
        /// Create a new attachment 
        /// </summary>
        public NotificationAttachment(String name, String contentType, String content)
        {
            this.Content = Encoding.UTF8.GetBytes(content);
            this.ContentType = contentType;
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the attachment
        /// </summary>
        public String Name { get;  }

        /// <summary>
        /// Gets or sets the content-type
        /// </summary>
        public String ContentType { get;  }

        /// <summary>
        /// Gets or sets the content
        /// </summary>
        public byte[] Content { get;   }
    }
}
