﻿using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
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
        [XmlEnum("c")]
        Create = 0x1,
        /// <summary>
        /// Record is updated
        /// </summary>
        [XmlEnum("u")]
        Update = 0x2,
        /// <summary>
        /// Record is deleted
        /// </summary>
        [XmlEnum("d")]
        Delete = 0x4,
        /// <summary>
        /// Record was merged
        /// </summary>
        [XmlEnum("m")]
        Merge = 0x8,
        /// <summary>
        /// Record was un-merged
        /// </summary>
        [XmlEnum("u")]
        UnMerge = 0x10
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
        [XmlAttribute("name"), JsonProperty("name")]
        public String Name { get; set; }
        
        /// <summary>
        /// True if is active
        /// </summary>
        [XmlAttribute("active"), JsonProperty("active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the resource type
        /// </summary>
        [XmlAttribute("resource"), JsonProperty("resource")]
        public String ResourceTypeXml { get; set; }

        /// <summary>
        /// Gets or sets the event
        /// </summary>
        [XmlAttribute("event"),JsonProperty("event")]
        public PubSubEventType Event { get; set; }

        /// <summary>
        /// Gets the resource type
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type ResourceType => new ModelSerializationBinder().BindToType(String.Empty, this.ResourceTypeXml);

        /// <summary>
        /// Gets or sets the filters
        /// </summary>
        [XmlArray("when"), XmlArrayItem("hdsiExpression"), JsonProperty("when")]
        public List<String> Filter { get; set; }

        /// <summary>
        /// Starts when
        /// </summary>
        [XmlElement("notBefore"), JsonProperty("notBefore")]
        public DateTimeOffset? NotBefore{ get; set; }

        /// <summary>
        /// Not after
        /// </summary>
        [XmlElement("notAfter"), JsonProperty("notAfter")]
        public DateTimeOffset? NotAfter { get; set; }

        /// <summary>
        /// Gets or sets the channel
        /// </summary>
        [XmlElement("channel"), JsonProperty("channel")]
        public Guid ChannelKey { get; set; }

    }
}