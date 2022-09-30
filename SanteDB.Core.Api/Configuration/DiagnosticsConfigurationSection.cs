/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{

    /// <summary>
    /// Diagnostics configuration
    /// </summary>
    [XmlType(nameof(DiagnosticsConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DiagnosticsConfigurationSection : IValidatableConfigurationSection
    {

        /// <summary>
        /// Initializes a new instance of the diagnostics configuration section
        /// </summary>
        public DiagnosticsConfigurationSection()
        {
            this.TraceWriter = new List<TraceWriterConfiguration>();
            this.Sources = new List<TraceSourceConfiguration>();
        }

        /// <summary>
        /// Gets or sets the sources to filter on
        /// </summary>
        [XmlArray("sources"), XmlArrayItem("add"), JsonProperty("sources")]
        public List<TraceSourceConfiguration> Sources { get; set; }

        /// <summary>
        /// Trace writers
        /// </summary>
        [XmlArray("writers"), XmlArrayItem("add"), JsonProperty("writers")]
        public List<TraceWriterConfiguration> TraceWriter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the default log mode
        /// </summary>
        [JsonProperty("mode"), XmlIgnore]
        public EventLevel Mode { get; set; }

        /// <summary>
        /// Validate the configuration section
        /// </summary>
        public IEnumerable<DetectedIssue> Validate()
        {
            foreach (var itm in this.TraceWriter)
            {
                if (!itm.TraceWriterClassXml.IsValid())
                {
                    // Is there a new type?
                    var tName = itm.TraceWriterClassXml.TypeXml.Split(',')[0].Split('.').Last();
                    var candidateType = AppDomain.CurrentDomain.GetAllTypes().FirstOrDefault(t => t.Name == tName);
                    if (candidateType != null)
                    {
                        yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "writerinvalid", $"Source {itm.WriterName} trace writer implementation {itm.TraceWriterClassXml.TypeXml} has moved to {candidateType.FullName}, {candidateType.Assembly.GetName().Name}", Guid.Empty);
                        itm.TraceWriter = candidateType;
                    }
                    else
                    {
                        yield return new DetectedIssue(DetectedIssuePriorityType.Error, "writerinvalid", $"Source {itm.WriterName} trace writer implementation {itm.TraceWriterClassXml.TypeXml} is invalid", Guid.Empty);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents the configuration of a single trace source
    /// </summary>
    [XmlType(nameof(TraceSourceConfiguration), Namespace = "http://santedb.org/configuration")]
    public class TraceSourceConfiguration
    {

        /// <summary>
        /// Gets the source name
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public String SourceName { get; set; }

        /// <summary>
        /// Gets or sets the filter of the trace writer
        /// </summary>
        /// <value>The filter.</value>
        [XmlAttribute("filter"), JsonProperty("filter")]
        public EventLevel Filter
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Trace writer configuration
    /// </summary>
    [XmlType(nameof(TraceWriterConfiguration), Namespace = "http://santedb.org/configuration")]
    public class TraceWriterConfiguration
    {

        /// <summary>
        /// Trace writer
        /// </summary>
        /// <value>The trace writer.</value>
        [XmlIgnore]
        public Type TraceWriter
        {
            get => this.TraceWriterClassXml.Type;
            set => this.TraceWriterClassXml = new TypeReferenceConfiguration(value);
        }

        /// <summary>
        /// Gets or sets the source name
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public String WriterName { get; set; }

        /// <summary>
        /// Gets or sets the initialization data.
        /// </summary>
        /// <value>The initialization data.</value>
        [XmlAttribute("initializationData"), JsonProperty("initializationData")]
        public String InitializationData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the writer implementation
        /// </summary>
        [XmlElement("writer")]
        public TypeReferenceConfiguration TraceWriterClassXml
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the filter of the trace writer
        /// </summary>
        /// <value>The filter.</value>
        [XmlAttribute("filter"), JsonProperty("filter")]
        public EventLevel Filter
        {
            get;
            set;
        }

    }

}

