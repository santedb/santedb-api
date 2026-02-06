/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System;
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
