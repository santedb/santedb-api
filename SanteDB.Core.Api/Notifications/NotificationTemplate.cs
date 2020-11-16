using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Represents a notification template source
    /// </summary>
    [XmlType(nameof(NotificationTemplate), Namespace = "http://santedb.org/notification")]
    [JsonObject]
    public class NotificationTemplate
    {

        // Serializer for notification
        private static XmlSerializer s_xsz = new XmlSerializer(typeof(NotificationTemplate));

        /// <summary>
        /// Gets or sets the language of the template
        /// </summary>
        [XmlAttribute("lang"), JsonProperty("lang")]
        public String Language { get; set; }

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public String Id { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        [XmlElement("subject"), JsonProperty("subject")]
        public String Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of the templates
        /// </summary>
        [XmlText, JsonProperty("text")]
        public String Body { get; set; }

        /// <summary>
        /// Load the specified object
        /// </summary>
        public static NotificationTemplate Load(Stream s)
        {
            return s_xsz.Deserialize(s) as NotificationTemplate;
        }

        /// <summary>
        /// Notification template
        /// </summary>
        public NotificationTemplate Save(Stream s)
        {
            s_xsz.Serialize(s, this);
            return this;
        }
    }
}
