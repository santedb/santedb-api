/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Mail
{
    /// <summary>
    /// Represents a mailbox - a particular folder or place which holds <see cref="MailMessage"/> instances
    /// </summary>
    [XmlType(nameof(Mailbox), Namespace = "http://santedb.org/model")]
    [JsonObject(nameof(Mailbox))]
    public class Mailbox : BaseEntityData
    {

        /// <summary>
        /// The name of the inbox
        /// </summary>
        public const string INBOX_NAME = "Inbox";
        /// <summary>
        /// The name of the sent mailbox
        /// </summary>
        public const string SENT_NAME = "Sent";
        /// <summary>
        /// The name of the deleted mailbox
        /// </summary>
        public const string DELTEED_NAME = "Deleted";

        /// <summary>
        /// Gets or sets the user which owns this mailbox
        /// </summary>
        [XmlElement("owner"), JsonProperty("owner")]
        public Guid OwnerKey { get; set; }

        /// <summary>
        /// Gets or sets the user which owns the mailbox
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(OwnerKey))]
        public SecurityUser Owner { get; set; }

        /// <summary>
        /// Gets or sets the name of the mailbox
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets the messages for this mailbox
        /// </summary>
        [XmlElement("messages"), JsonProperty("messages")]
        public List<MailboxMailMessage> Messages { get; set; }


    }
}
