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
using Newtonsoft.Json;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Interfaces;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Notification parameter
    /// </summary>
    [XmlType(nameof(NotificationTemplateParameter), Namespace = "http://santedb.org/notification")]
    [XmlRoot(nameof(NotificationTemplateParameter), Namespace = "http://santedb.org/notification")]
    [JsonObject]
    public class NotificationTemplateParameter : NonVersionedEntityData, ISimpleAssociation
    {
        /// <summary>
        /// Gets or sets the notification template key
        /// </summary>
        [XmlElement("template"), JsonProperty("template")]
        public Guid NotificationTemplateKey { get; set; }

        /// <summary>
        /// Gets or sets the notification template
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(NotificationTemplateKey))]
        public NotificationTemplate NotificationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [XmlElement("description"), JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the instance parameters
        /// </summary>
        [XmlElement("instanceParameter"), JsonProperty("instanceParameter")]
        public List<NotificationInstanceParameter> InstanceParameter { get; set; }

        /// <summary>
        /// Gets the source type
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type SourceType => typeof(NotificationTemplate);

        /// <summary>
        /// Gets and sets the source entity key
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Guid? SourceEntityKey { get => NotificationTemplateKey; set => NotificationTemplateKey = (Guid)value; }

        /// <summary>
        /// gets and sets the source entity
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public object SourceEntity { get => NotificationTemplate; set => NotificationTemplate = (NotificationTemplate)value; }
    }
}