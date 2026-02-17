using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Audit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.PubSub
{
    /// <summary>
    /// Represents a dispatch log entry
    /// </summary>
    [XmlType(nameof(PubSubDispatchLog), Namespace = "http://santedb.org/pubsub")]
    [XmlRoot(nameof(PubSubDispatchLog), Namespace = "http://santedb.org/pubsub")]
    [JsonObject]
    public class PubSubDispatchLog : IdentifiedData
    {

        /// <inheritdoc/>
        [XmlIgnore, JsonIgnore]
        public override DateTimeOffset ModifiedOn => this.DispatchTime;
        
        /// <summary>
        /// Gets or set sthe event
        /// </summary>
        [XmlElement("event"), JsonProperty("event")]
        public PubSubEventType Event { get; set; }

        /// <summary>
        /// Gets or sets the time that the object was dispatched
        /// </summary>
        [XmlElement("dispatchTime"), JsonProperty("dispatchTime")]
        public DateTimeOffset DispatchTime { get; set; }

        /// <summary>
        /// The outcome of the dispatch
        /// </summary>
        [XmlElement("outcome"), JsonProperty("outcome")]
        public OutcomeIndicator Outcome { get; set; }

        /// <summary>
        /// Gets or sets the version sequence that was dispatched
        /// </summary>
        [XmlElement("versionSequence"), JsonProperty("versionSequence")]
        public Int64 VersionSequence { get; set; }

        /// <summary>
        /// Gets or sets the object key 
        /// </summary>
        [XmlElement("objectId"), JsonProperty("objectId")]
        public Guid ObjectKey { get; set; }
    }
}
