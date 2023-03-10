/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// Because we're using PCL we have to wrap the TraceWriter interface
    /// </summary>
    public abstract class TraceWriter
    {
        // Configuration
        private ConcurrentDictionary<string, EventLevel> m_sourceFilters;

        // Filter
        private EventLevel m_filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceWriter"/> class.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="initializationData">The initialization data.</param>
        /// <param name="configuration">The configuration to set for this object</param>
        public TraceWriter(EventLevel filter, String initializationData, IDictionary<String, EventLevel> configuration)
        {
            this.m_filter = filter;
            if (configuration != null)
            {
                this.m_sourceFilters = new ConcurrentDictionary<string, EventLevel>(configuration);
            }
            else
            {
                this.m_sourceFilters = new ConcurrentDictionary<string, EventLevel>();
            }
        }

        /// <summary>
        /// Trace information
        /// </summary>
        public void TraceInfo(String source, String format, params Object[] args)
        {
            this.TraceEvent(EventLevel.Informational, source, format, args);
        }

        /// <summary>
        /// Trace an event to the writer
        /// </summary>
        public virtual void TraceEvent(EventLevel level, String source, String format, params Object[] args)
        {
            try
            {
                var sourceConfig = this.m_filter;
                if (!this.m_sourceFilters.TryGetValue(source, out sourceConfig))
                {
                    var key = this.m_sourceFilters.FirstOrDefault(o => source.StartsWith(o.Key));

                    if (String.IsNullOrEmpty(key.Key))
                    {
                        sourceConfig = this.m_filter;
                    }
                    else
                    {
                        sourceConfig = key.Value;
                    }
                    this.m_sourceFilters.TryAdd(source, sourceConfig);
                }

                if (this.m_filter == EventLevel.LogAlways)
                {
                    this.WriteTrace(level, source, format, args);
                }
                else if (this.m_filter >= level &&
                    (sourceConfig >= level ||
                    sourceConfig == EventLevel.LogAlways))
                {
                    this.WriteTrace(level, source, format, args);
                }
            }
            catch { }
        }

        /// <summary>
        /// Write data to the event
        /// </summary>
        protected abstract void WriteTrace(EventLevel level, String source, String format, params Object[] args);

        /// <summary>
        /// Trace an event with data to the log
        /// </summary>
        public abstract void TraceEventWithData(EventLevel level, string source, string message, object[] data);

        /// <summary>
        /// Trace an error
        /// </summary>
        public void TraceError(String source, String format, params Object[] args)
        {
            this.TraceEvent(EventLevel.Error, source, format, args);
        }

        /// <summary>
        /// Trace warning
        /// </summary>
        public void TraceWarning(String source, String format, params Object[] args)
        {
            this.TraceEvent(EventLevel.Warning, source, format, args);
        }
    }
}