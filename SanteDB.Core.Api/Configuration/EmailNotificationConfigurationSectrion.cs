using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Configuration for the e-mail notification
    /// </summary>
    [XmlType(nameof(EmailNotificationConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class EmailNotificationConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// SMTP Configuration settings
        /// </summary>
        [XmlElement("smtp"), JsonProperty("smtp")]
        public SmtpConfiguration Smtp { get; set; }

        /// <summary>
        /// Gets or sets the administrative contacts for this server
        /// </summary>
        [XmlArray("adminContacts"), XmlArrayItem("add"), JsonProperty("adminContacts")]
        public List<String> AdministrativeContacts { get; set; }
    }

    /// <summary>
    /// SMTP configuration
    /// </summary>
    [XmlType(nameof(SmtpConfiguration), Namespace = "http://santedb.org/configuration")]
    public class SmtpConfiguration
    {

        /// <summary>
        /// Gets the from address
        /// </summary>
        [XmlElement("from"), JsonProperty("from")]
        public string From { get; set; }

        /// <summary>
        /// Gets the password
        /// </summary>
        [XmlElement("password"), JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets the SMTP server
        /// </summary>
        [XmlElement("server"), JsonProperty("server")]
        public String Server { get; set; }

        /// <summary>
        /// Get the SSL setting
        /// </summary>
        [XmlElement("useTls"), JsonProperty("useTls")]
        public bool Ssl { get; set; }

        /// <summary>
        /// Gets the username for connecting to the server
        /// </summary>
        [XmlElement("username"), JsonProperty("username")]
        public string Username { get; set; }
    }
}
