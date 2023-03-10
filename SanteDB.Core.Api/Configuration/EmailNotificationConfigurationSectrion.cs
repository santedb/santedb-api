/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
