/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;

namespace SanteDB.Core.Notifications
{
    // <summary>
    /// Represents a notification resource
    /// </summary>
    [XmlType(nameof(NotificationInstance), Namespace = "http://santedb.org/notification")]
    [XmlRoot(nameof(NotificationInstance), Namespace = "http://santedb.org/notification")]
    [JsonObject]
    public class NotificationInstance : NonVersionedEntityData
    {
        // Serializer for notification instance
        private static XmlSerializer s_xsz = new XmlSerializer(typeof(NotificationInstance));

        /// <summary>
        /// Gets or set the identifier
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the notification template key
        /// </summary>
        [XmlElement("templateId"), JsonProperty("templateId")]
        public Guid NotificationTemplateKey { get; set; }

        /// <summary>
        /// Gets or sets the notification template
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(NotificationTemplateKey))]
        public NotificationTemplate NotificationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the entity type key
        /// </summary>
        [XmlElement("entityTypeId"), JsonProperty("entityTypeId")]
        public Guid EntityTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(EntityTypeKey))]
        public Concept EntityType { get; set; }

        /// <summary>
        /// Gets or sets the mnemonic of the notification
        /// </summary>
        [XmlElement("mnemonic"), JsonProperty("mnemonic")]
        public String Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the name of the notification
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the state key of the notification
        /// </summary>
        [XmlElement("stateId"), JsonProperty("stateId")]
        public Guid StateKey { get; set; }


        /// <summary>
        /// Gets or sets the state of the notification
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(StateKey))]
        public Concept State { get; set; }

        /// <summary>
        /// Gets or sets the description of the notification
        /// </summary>
        [XmlElement("description"), JsonProperty("description")]
        public String Description { get; set; }

        /// <summary>
        /// Gets or sets the filter for the notification
        /// </summary>
        [XmlElement("filter"), JsonProperty("filter")]
        public String FilterExpression { get; set; }

        /// <summary>
        /// Gets or sets the trigger for the notification
        /// </summary>
        [XmlElement("trigger"), JsonProperty("trigger")]
        public String TriggerExpression { get; set; }

        /// <summary>
        /// Gets or sets the trigger for the notification
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public String TargetExpression { get; set; }

        /// <summary>
        /// Gets or sets the instance parameters
        /// </summary>
        [XmlElement("instanceParameters"), JsonProperty("instanceParameters")]
        public List<NotificationInstanceParameter> InstanceParameters { get; set; }

        /// <summary>
        /// Load the notification instance
        /// </summary>
        public static NotificationInstance Load(Stream s)
        {
            return s_xsz.Deserialize(s) as NotificationInstance;
        }

        /// <summary>
        /// Save the notification instance
        /// </summary>
        public NotificationInstance Save(Stream s)
        {
            s_xsz.Serialize(s, this);
            return this;
        }
    }
}
