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
using Newtonsoft.Json;
using System;
using System.IO;
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
