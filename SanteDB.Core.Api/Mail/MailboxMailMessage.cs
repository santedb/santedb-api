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
using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Mail
{

    /// <summary>
    /// Associates a <see cref="MailMessage"/> with a <see cref="Mailbox"/>
    /// </summary>
    [XmlRoot(nameof(MailboxMailMessage), Namespace = "http://santedb.org/model")]
    [XmlType(nameof(MailboxMailMessage), Namespace = "http://santedb.org/model")]
    [JsonObject(nameof(MailboxMailMessage))]
    public class MailboxMailMessage : Association<Mailbox>, ISimpleTargetedAssociation
    {

        /// <summary>
        /// Gets the target of the association
        /// </summary>
        [XmlElement("message"), JsonProperty("message")]
        public Guid? TargetEntityKey { get; set; }

        /// <summary>
        /// Gets or sets the flags
        /// </summary>
        [XmlElement("flags"), JsonProperty("flags")]
        public MailStatusFlags MailStatusFlag { get; set; }

        /// <summary>
        /// Date/time of the alert
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(DeliveredTimeXml))]
        public DateTimeOffset DeliveredTime { get; set; }

        /// <summary>
        /// Gets or sets the time of the alert.
        /// </summary>
        [JsonProperty("time"), XmlElement("time")]
        public String DeliveredTimeXml
        {
            get { return this.DeliveredTime.ToString("o"); }
            set
            {
                this.DeliveredTime = DateTime.Parse(value);
            }
        }

        /// <summary>
        /// Target key
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(TargetEntityKey))]
        public MailMessage TargetEntity { get; set; }

        /// <inheritdoc/>
        [XmlIgnore, JsonIgnore]
        object ISimpleTargetedAssociation.TargetEntity { 
            get => this.TargetEntity; 
            set => this.TargetEntity = value as MailMessage; 
        }
    }
}