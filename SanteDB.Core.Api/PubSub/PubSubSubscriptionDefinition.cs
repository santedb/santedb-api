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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Identifies the type of events on the pub-sub layer
    /// </summary>
    [XmlType(nameof(PubSubEventType), Namespace = "http://santedb.org/pubsub")]
    [Flags]
    public enum PubSubEventType
    {
        /// <summary>
        /// Record is created
        /// </summary>
        [XmlEnum("create")]
        Create = 0x1,

        /// <summary>
        /// Record is updated
        /// </summary>
        [XmlEnum("update")]
        Update = 0x2,

        /// <summary>
        /// Record is deleted
        /// </summary>
        [XmlEnum("delete")]
        Delete = 0x4,

        /// <summary>
        /// Record was merged
        /// </summary>
        [XmlEnum("merge")]
        Merge = 0x8,

        /// <summary>
        /// Record was un-merged
        /// </summary>
        [XmlEnum("unmerge")]
        UnMerge = 0x10,

        /// <summary>
        /// Record was linked
        /// </summary>
        [XmlEnum("link")]
        Link = 0x20,

        /// <summary>
        /// Record was un-linked    
        /// </summary>
        [XmlEnum("unlink")]
        UnLink = 0x40
    }

    /// <summary>
    /// Represents a configuration / definition for a pub-sub subscription
    /// </summary>
    [XmlType(nameof(PubSubSubscriptionDefinition), Namespace = "http://santedb.org/pubsub")]
    [XmlRoot(nameof(PubSubSubscriptionDefinition), Namespace = "http://santedb.org/pubsub")]
    [JsonObject]
    public class PubSubSubscriptionDefinition : NonVersionedEntityData
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// True if is active
        /// </summary>
        [XmlElement("active"), JsonProperty("active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        [XmlElement("resource"), JsonProperty("resource")]
        public String ResourceTypeName { get; set; }

        /// <summary>
        /// Gets or sets the event
        /// </summary>
        [XmlElement("event"), JsonProperty("event")]
        public PubSubEventType Event { get; set; }

        /// <summary>
        /// Gets the resource type
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type ResourceType => new ModelSerializationBinder().BindToType(String.Empty, this.ResourceTypeName);

        /// <summary>
        /// Gets or sets the filters
        /// </summary>
        [XmlArray("when"), XmlArrayItem("hdsiExpression"), JsonProperty("when")]
        public List<String> Filter { get; set; }

        /// <summary>
        /// Starts when
        /// </summary>
        [XmlElement("notBefore"), JsonProperty("notBefore")]
        public DateTime? NotBefore { get; set; }

        /// <summary>
        /// Not after
        /// </summary>
        [XmlElement("notAfter"), JsonProperty("notAfter")]
        public DateTime? NotAfter { get; set; }

        /// <summary>
        /// Gets or sets the channel
        /// </summary>
        [XmlElement("channel"), JsonProperty("channel")]
        public Guid ChannelKey { get; set; }

        /// <summary>
        /// Gets the description of the subscription
        /// </summary>
        [XmlElement("description"), JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets the support contact
        /// </summary>
        [XmlElement("support"), JsonProperty("support")]
        public string SupportContact { get; set; }
    }
}