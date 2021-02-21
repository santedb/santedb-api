/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
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