using Newtonsoft.Json;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Audit filter configuration
    /// </summary>
    [XmlType(nameof(AuditFilterConfiguration), Namespace = "http://santedb.org/configuration")]
    public class AuditFilterConfiguration
    {

        /// <summary>
        /// Default ctor for serialization
        /// </summary>
        public AuditFilterConfiguration()
        {

        }

        /// <summary>
        /// Creates a new audit filter configuration object
        /// </summary>
        public AuditFilterConfiguration(Auditing.ActionType? action, Auditing.EventIdentifierType? eventType, Auditing.OutcomeIndicator? outcomeType, bool insertLocal, bool sendRemote)
        {
            this.Action = action.GetValueOrDefault();
            this.Event = eventType.GetValueOrDefault();
            this.Outcome = outcomeType.GetValueOrDefault();
            this.ActionSpecified = action.HasValue;
            this.EventSpecified = eventType.HasValue;
            this.OutcomeSpecified = outcomeType.HasValue;
            this.InsertLocal = insertLocal;
            this.SendRemote = sendRemote;
        }

        /// <summary>
        /// Filter on action type
        /// </summary>
        [XmlAttribute("action"), JsonProperty("action")]
        public Auditing.ActionType Action { get; set; }

        /// <summary>
        /// Filter on event
        /// </summary>
        [XmlAttribute("event"), JsonProperty("event")]
        public Auditing.EventIdentifierType Event { get; set; }

        /// <summary>
        /// Filter on outcome
        /// </summary>
        [XmlAttribute("outcome"), JsonProperty("outcome")]
        public Auditing.OutcomeIndicator Outcome { get; set; }

        /// <summary>
        /// True if when a filter matches the audit you want to include locally
        /// </summary>
        [XmlAttribute("insert"), JsonProperty("insert")]
        public bool InsertLocal { get; set; }

        /// <summary>
        /// True when filter is active to shipt
        /// </summary>
        [XmlAttribute("ship"), JsonProperty("ship")]
        public bool SendRemote { get; set; }

        #region Serialization Control
        [XmlIgnore,JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool ActionSpecified { get; set; }
        [XmlIgnore,JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool EventSpecified { get; set; }
        [XmlIgnore,JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool OutcomeSpecified { get; set; }
        #endregion

    }
}