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
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Represents a notification template source
    /// </summary>
    [XmlType(nameof(NotificationTemplate), Namespace = "http://santedb.org/notification")]
    [XmlRoot(nameof(NotificationTemplate), Namespace = "http://santedb.org/notification")]
    [JsonObject]
    public class NotificationTemplate : NonVersionedEntityData
    {

        // Serializer for notification
        private static XmlSerializer s_xsz = new XmlSerializer(typeof(NotificationTemplate));

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public String Id { get; set; }

        /// <summary>
        /// Notification template content
        /// </summary>
        [XmlElement("content"), JsonProperty("content")]
        public List<NotificationTemplateContents> Contents { get; set; }

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
