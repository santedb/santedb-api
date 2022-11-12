using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Mail
{

    /// <summary>
    /// Associates a <see cref="MailMessage"/> with a <see cref="Mailbox"/>
    /// </summary>
    [XmlType(nameof(MailboxMailMessage), Namespace = "http://santedb.org/model")]
    [JsonObject(nameof(MailboxMailMessage))]
    public class MailboxMailMessage : Association<Mailbox>
    {

        /// <summary>
        /// Gets the target of the association
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public Guid TargetKey { get; set; }

        /// <summary>
        /// Gets or sets the flags
        /// </summary>
        [XmlElement("flags"), JsonProperty("flags")]
        public MailStatusFlags MailStatusFlag { get; set; }

        /// <summary>
        /// Gets the target of the relationship
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(TargetKey))]
        public MailMessage Target { get; set; }

    }
}