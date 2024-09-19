﻿/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace SanteDB.Core.Diagnostics.Tracing
{
    /// <summary>
    /// A trace writer that uses the System.Diagnostics.Debug class
    /// </summary>
    [DisplayName("Debug Trace Writer")]
    public class DebugDiagnosticsTraceWriter : TraceWriter
    {


        /// <summary>
        /// CTOR for diagnostics
        /// </summary>
        public DebugDiagnosticsTraceWriter(EventLevel filter, string fileName, IDictionary<String, EventLevel> sources) : base(filter, fileName, sources)
        {
        }

        /// <summary>
        /// Creates a new diagnostics trace writer
        /// </summary>
        public DebugDiagnosticsTraceWriter() : base(EventLevel.LogAlways, null, new Dictionary<String, EventLevel>())
        {
        }

        /// <summary>
        /// Write the specified trace
        /// </summary>
        protected override void WriteTrace(EventLevel level, string source, string format, params object[] args)
        {
            Debug.WriteLine($"{source} [{level}] : {String.Format(format, args)}");
        }

        /// <summary>
        /// Trace event data 
        /// </summary>
        public override void TraceEventWithData(EventLevel level, string source, string message, object[] data)
        {
        }
    }
}
