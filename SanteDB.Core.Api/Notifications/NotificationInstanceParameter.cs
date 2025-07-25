﻿/*
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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Notifications
{
    /// <summary>
    /// Notification instance parameter
    /// </summary>
    [XmlType(nameof(NotificationInstanceParameter), Namespace = "http://santedb.org/notification")]
    [XmlRoot(nameof(NotificationInstanceParameter), Namespace = "http://santedb.org/notification")]
    [JsonObject]
    public class NotificationInstanceParameter : NonVersionedEntityData, ISimpleAssociation
    {

        /// <summary>
        /// Gets or sets the notification instance key
        /// </summary>
        [XmlElement("notificationInstance"), JsonProperty("notificationInstance")]
        public Guid NotificationInstanceKey { get; set; }

        /// <summary>
        /// Gets or sets the notification instance
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(NotificationInstanceKey))]
        public NotificationInstance NotificationInstance { get; set; }

        /// <summary>
        /// Gets or sets the parameter name which this instance references in the template.
        /// </summary>
        [XmlElement("templateParameter"), JsonProperty("templateParameter")]
        public string ParameterName { get; set; }

        /*
        /// <summary>
        /// Gets or sets the template parameter key
        /// </summary>
        [XmlElement("templateParameter"), JsonProperty("templateParameter")]
        public Guid TemplateParameterKey { get; set; }

        /// <summary>
        /// Gets or sets the template parameter
        /// </summary>
        [XmlIgnore, JsonIgnore, SerializationReference(nameof(TemplateParameterKey))]
        public NotificationTemplateParameter TemplateParameter { get; set; }
        */

        /// <summary>
        /// Gets or sets the expression
        /// </summary>
        [XmlElement("expression"), JsonProperty("expression")]
        public string Expression { get; set; }

        /// <summary>
        /// Gets and sets the source type
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type SourceType => typeof(NotificationInstance);

        /// <summary>
        /// Gets and sets the source entity key
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Guid? SourceEntityKey { get => NotificationInstanceKey; set => NotificationInstanceKey = (Guid)value; }

        /// <summary>
        /// Gets and sets the source entity
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public object SourceEntity { get => NotificationInstance; set => NotificationInstance = (NotificationInstance)value; }
    }
}