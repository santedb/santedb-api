/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * User: fyfej
 * Date: 2023-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Audit;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Audit filter configuration
    /// </summary>
    [XmlType(nameof(AuditFilterConfiguration), Namespace = "http://santedb.org/configuration")]
    [DisplayName("Audit Filter")]
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
        public AuditFilterConfiguration(ActionType? action, EventIdentifierType? eventType, OutcomeIndicator? outcomeType, bool insertLocal, bool sendRemote)
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
        /// Identifies the resource sensitivity filtering
        /// </summary>
        [XmlAttribute("sensitivity"), JsonProperty("sensitivity"), DisplayName("Sensitivity Filter")]
        public ResourceSensitivityClassification Sensitivity { get; set; }

        /// <summary>
        /// Filter on action type
        /// </summary>
        [XmlAttribute("action"), JsonProperty("action"), DisplayName("Action Filter"), Description("Filters on action type")]
        public ActionType Action { get; set; }

        /// <summary>
        /// Filter on event
        /// </summary>
        [XmlAttribute("event"), JsonProperty("event"), DisplayName("Event Filter"), Description("Filters on event type")]
        public EventIdentifierType Event { get; set; }

        /// <summary>
        /// Filter on outcome
        /// </summary>
        [XmlAttribute("outcome"), JsonProperty("outcome"), DisplayName("Outcome Filter"), Description("Filters on outcome type")]
        public OutcomeIndicator Outcome { get; set; }

        /// <summary>
        /// True if when a filter matches the audit you want to include locally
        /// </summary>
        [XmlAttribute("insert"), JsonProperty("insert"), DisplayName("Insert to AR"), Description("When true, audits matching the filter will be stored locally in the audit repository")]
        public bool InsertLocal { get; set; }

        /// <summary>
        /// True when filter is active to shipt
        /// </summary>
        [XmlAttribute("ship"), JsonProperty("ship"), DisplayName("Send to Remote"), Description("When true, audits matching the filter will be sent upstream")]
        public bool SendRemote { get; set; }

        #region Serialization Control

        /// <summary>
        /// True if the action is specified
        /// </summary>
        [XmlIgnore, JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool ActionSpecified { get; set; }

        /// <summary>
        /// True if the event is specified
        /// </summary>
        [XmlIgnore, JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool EventSpecified { get; set; }

        /// <summary>
        /// True if the outcome is specified
        /// </summary>
        [XmlIgnore, JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool OutcomeSpecified { get; set; }

        /// <summary>
        /// True if sensitivity is specified
        /// </summary>
        [XmlIgnore, JsonIgnore, EditorBrowsable(EditorBrowsableState.Never)]
        public bool SensitivitySpecified { get; set; }
        #endregion


        /// <summary>
        /// Get the combined filter flags
        /// </summary>
        public ulong FilterFlags => (this.SensitivitySpecified ? (ulong)this.Sensitivity : 0xFF) << 32 | 
            (this.OutcomeSpecified ? (ulong)this.Outcome : 0xFF) << 24 |
            (this.ActionSpecified ? (ulong)this.Action : 0xFF) << 16 |
            (this.EventSpecified ? (ulong)this.Event : 0xFFFF);

        /// <summary>
        /// Represent the filter as a stirng
        /// </summary>
        public override string ToString() => $"SENS={(this.SensitivitySpecified ? this.Sensitivity : 0)};ACT={(this.ActionSpecified ? this.Action : 0)};EVT={(this.EventSpecified ? this.Event : 0)};OUTC={(this.OutcomeSpecified ? this.Outcome : 0)}";
    }
}