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
    public class NotificationInstanceParameter
    {

        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the notification
        /// </summary>
        [XmlElement("notification"), JsonProperty("notification")]
        public Guid NotificationInstance { get; set; }

        /// <summary>
        /// Gets or sets the parameter
        /// </summary>
        [XmlElement("parameter"), JsonProperty("parameter")]
        public Guid TemplateParameter { get; set; }

        /// <summary>
        /// Gets or sets the expression
        /// </summary>
        [XmlElement("expression"), JsonProperty("expression")]
        public string Expression { get; set; }

    }
}