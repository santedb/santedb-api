/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
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
 * User: JustinFyfe
 * Date: 2019-2-28
 */
using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{

    /// <summary>
    /// Diagnostics configuration
    /// </summary>
    [XmlType(nameof(DiagnosticsConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DiagnosticsConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Initializes a new instance of the diagnostics configuration section
        /// </summary>
        public DiagnosticsConfigurationSection()
        {
            this.TraceWriter = new List<TraceWriterConfiguration>();
        }

        /// <summary>
        /// Trace writers
        /// </summary>
        [XmlElement("trace"), JsonProperty("trace")]
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
        public TraceWriter TraceWriter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the source name
        /// </summary>
        [XmlAttribute("source"), JsonProperty("name")]
        public String SourceName { get; set; }

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
        public String TraceWriterClassXml
        {
            get { return this.TraceWriter.GetType().AssemblyQualifiedName; }
            set
            {
                this.TraceWriter = Activator.CreateInstance(Type.GetType(value), this.Filter, this.InitializationData) as TraceWriter;
            }
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

