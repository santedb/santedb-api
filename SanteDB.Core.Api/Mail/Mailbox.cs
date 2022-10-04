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
