/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using System;
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
        /// <param name="ccAdmins">When true, administrators should be notified as well</param>
        Guid Send(String[] toAddress, String subject, String body, DateTimeOffset? scheduleDelivery = null, bool ccAdmins = false, params NotificationAttachment[] attachments);

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
