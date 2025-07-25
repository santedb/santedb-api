﻿/*
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
using SanteDB.Core.Model.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Represents a notification template resource
    /// </summary>
    [XmlType(nameof(NotificationTemplate), Namespace = "http://santedb.org/notification")]
    [XmlRoot(nameof(NotificationTemplate), Namespace = "http://santedb.org/notification")]
    [JsonObject]
    public class NotificationTemplate : NonVersionedEntityData
    {

        // Serializer for notification template
        private static XmlSerializer s_xsz = new XmlSerializer(typeof(NotificationTemplate));

        /// <summary>
        /// Gets or sets the status key
        /// </summary>
        [XmlElement("status"), JsonProperty("status")]
        public Guid StatusKey { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(StatusKey))]
        public Concept Status { get; set; }

        /// <summary>
        /// Gets or sets the template mnemonic
        /// </summary>
        [XmlElement("mnemonic"), JsonProperty("mnemonic")]
        public String Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the template name
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the tags
        /// </summary>
        [XmlElement("tags"), JsonProperty("tags")]
        public String Tags { get; set; }

        /// <summary>
        /// Notification template content
        /// </summary>
        [XmlElement("contents"), JsonProperty("contents")]
        public List<NotificationTemplateContents> Contents { get; set; }

        /// <summary>
        /// Notification template parameters
        /// </summary>
        [XmlElement("parameters"), JsonProperty("parameters")]
        public List<NotificationTemplateParameter> Parameters { get; set; }

        /// <summary>
        /// Load the notification template
        /// </summary>
        public static NotificationTemplate Load(Stream s)
        {
            return s_xsz.Deserialize(s) as NotificationTemplate;
        }

        /// <summary>
        /// Save the notification template
        /// </summary>
        public NotificationTemplate Save(Stream s)
        {
            s_xsz.Serialize(s, this);
            return this;
        }
    }
}
