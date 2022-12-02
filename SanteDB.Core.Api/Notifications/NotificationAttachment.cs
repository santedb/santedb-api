using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Notifications
{
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
        public String Name { get; }

        /// <summary>
        /// Gets or sets the content-type
        /// </summary>
        public String ContentType { get; }

        /// <summary>
        /// Gets or sets the content
        /// </summary>
        public byte[] Content { get; }
    }
}
