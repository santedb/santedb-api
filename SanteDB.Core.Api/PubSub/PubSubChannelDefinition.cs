using Newtonsoft.Json;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Identifies a single channel in the pub-sub manager
    /// </summary>
    [XmlType(nameof(PubSubChannelDefinition), Namespace = "http://santedb.org/pubsub")]
    [XmlRoot(nameof(PubSubChannelDefinition), Namespace = "http://santedb.org/pubsub")]
    [JsonObject]
    public class PubSubChannelDefinition : NonVersionedEntityData
    {

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets whether the channel is active
        /// </summary>
        [XmlAttribute("active"),JsonProperty("active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the endpoint
        /// </summary>
        [XmlElement("endpoint"), JsonProperty("endpoint")]
        public Uri Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the settings on this channel
        /// </summary>
        [XmlArray("settings"), XmlArrayItem("add"), JsonProperty("settings")]
        public List<PubSubChannelSetting> Settings { get; set; }

        /// <summary>
        /// Gets or sets the dispatcher type
        /// </summary>
        [XmlElement("dispatcherFactory"), JsonProperty("dispatcherFactory")]
        public String DispatcherFactoryTypeXml { get; set; }

        /// <summary>
        /// Gets the dispatcher type
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public Type DispatcherFactoryType => System.Type.GetType(this.DispatcherFactoryTypeXml);
        
    }
}
