using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// A single pub-sub channel setting
    /// </summary>
    [XmlType(nameof(PubSubChannelSetting), Namespace = "http://santedb.org/pubsub")]
    [JsonObject]
    public class PubSubChannelSetting
    {
        /// <summary>
        /// Gets the name of the setting
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        [XmlText, JsonProperty("value")]
        public string Value { get; set; }


    }
}