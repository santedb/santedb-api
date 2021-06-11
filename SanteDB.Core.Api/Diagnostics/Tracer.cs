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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// Represents a logger
    /// </summary>
    public class Tracer
    {
        // The source of the logger
        private String m_source;

        // Writers
        private static List<KeyValuePair<TraceWriter, EventLevel>> m_writers = new List<KeyValuePair<TraceWriter, EventLevel>>();

        /// <summary>
        /// Adds a writer to the trace stack
        /// </summary>
        public static void AddWriter(TraceWriter tw, EventLevel filter)
        {
            m_writers.Add(new KeyValuePair<TraceWriter, EventLevel>(tw, filter));
        }

        /// <summary>
        /// Dispose trace writers
        /// </summary>
        public static void DisposeWriters()
        {
            foreach (var itm in m_writers)
                (itm.Key as IDisposable)?.Dispose();
            return;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SanteDB.Core.Diagnostics.Tracer"/> class.
        /// </summary>
        /// <param name="source">Source.</param>
        public Tracer(String source)
        {
            this.m_source = source;
        }

        /// <summary>
        /// Creates a logging interface for the specified source
        /// </summary>
        public static Tracer GetTracer(Type sourceType)
        {
            return new Tracer(sourceType.FullName);
        }

        /// <summary>
        /// Trace an event
        /// </summary>
        public void TraceEvent(System.Diagnostics.Tracing.EventLevel level, string format, params Object[] args)
        {
            foreach (var w in m_writers.ToArray())
            {
             //    if (level <= w.Value || w.Value == EventLevel.LogAlways)
                    w.Key.TraceEvent(level, this.m_source, format, args);
            }
        }

        /// <summary>
        /// Trace structured data into the log
        /// </summary>
        public void TraceData(EventLevel level, String message, params object[] data)
        {
            foreach (var w in m_writers.ToArray())
            {
                w.Key.TraceEventWithData(level, this.m_source, message, data);
            }
        }

        /// <summary>
        /// Trace error
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public void TraceError(String format, params Object[] args)
        {
            this.TraceEvent(EventLevel.Error, format, args);
        }

        /// <summary>
        /// Trace error
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public void TraceWarning(String format, params Object[] args)
        {
            this.TraceEvent(EventLevel.Warning, format, args);
        }

        /// <summary>
        /// Emits a warning to the trace log that an untested feature was used
        /// </summary>
        public void TraceUntestedWarning()
        {

            this.TraceEvent(EventLevel.Warning, "UNTESTED CODE WARNING ----> A PROCESS CALLED AN UNTESTED SECTION OF CODE SUBSEQUENT ERRORS MAY APPEAR IN THE LOG ----> {0}", new StackTrace(true));

        }

        /// <summary>
        /// Trace error
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public void TraceInfo(String format, params Object[] args)
        {
            this.TraceEvent(EventLevel.Informational, format, args);
        }

        /// <summary>
        /// Trace error
        /// </summary>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        public void TraceVerbose(String format, params Object[] args)
        {
            this.TraceEvent(EventLevel.Verbose, format, args);
        }
    }
}