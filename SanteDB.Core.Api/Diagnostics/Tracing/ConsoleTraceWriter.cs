﻿/*
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
 * Date: 2021-11-17
 */
using SanteDB.Core.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SanteDB.Core.Diagnostics.Tracing
{
    /// <summary>
    /// Tracer writer that writes to the console
    /// </summary>
    [DisplayName("Console Trace Writer")]
    public class ConsoleTraceWriter : TraceWriter, IDisposable
    {
        // Dispatch thread
        private Thread m_dispatchThread = null;

        // True when disposing
        private bool m_disposing = false;

        // The log backlog
        private ConcurrentQueue<KeyValuePair<ConsoleColor, String>> m_logBacklog = new ConcurrentQueue<KeyValuePair<ConsoleColor, string>>();

        // Reset event
        private ManualResetEventSlim m_resetEvent = new ManualResetEventSlim(false);

        /// <summary>
        /// Console trace writer
        /// </summary>
        public ConsoleTraceWriter(EventLevel filter, string initializationData, IDictionary<String, EventLevel> sources) : base(filter, initializationData, sources)
        {
            // Start log dispatch
            this.m_dispatchThread = new Thread(this.LogDispatcherLoop);
            this.m_dispatchThread.IsBackground = true;
            this.m_dispatchThread.Start();
        }

        /// <summary>
        /// Write a trace
        /// </summary>
        protected override void WriteTrace(EventLevel level, string source, string format, params object[] args)
        {
            ConsoleColor color = ConsoleColor.White;
            switch (level)
            {
                case EventLevel.Verbose:
                    if (format.Contains("PERF"))
                        color = ConsoleColor.Green;
                    else
                        color = ConsoleColor.Magenta;
                    break;

                case EventLevel.Informational:
                    color = ConsoleColor.Cyan;
                    break;

                case EventLevel.Warning:
                    color = ConsoleColor.Yellow;
                    break;

                case EventLevel.Error:
                    color = ConsoleColor.Red;
                    break;

                case EventLevel.Critical:
                    color = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    break;
            }

            this.m_logBacklog.Enqueue(new KeyValuePair<ConsoleColor, String>(color, String.Format("{0:yyyy/MM/dd HH:mm:ss} [{1}] : {2} {3}: 0 : {4}", DateTime.Now, String.IsNullOrEmpty(Thread.CurrentThread.Name) ? $"@{Thread.CurrentThread.ManagedThreadId}" : Thread.CurrentThread.Name, source, level, String.Format(format, args))));
            this.m_resetEvent.Set();
        }

        private void LogDispatcherLoop()
        {
            while (true)
            {
                while (this.m_logBacklog.IsEmpty && !this.m_disposing)
                {
                    this.m_resetEvent.Wait();
                    this.m_resetEvent.Reset();
                }
                if (this.m_disposing) return;

                while (!this.m_logBacklog.IsEmpty)
                {
                    if (this.m_logBacklog.TryDequeue(out var dq))
                    {
                        if (this.m_disposing) return;
                        Console.ForegroundColor = dq.Key;
                        Console.WriteLine(dq.Value);
                        Console.ResetColor();
                    }
                }
            }
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            if (this.m_dispatchThread != null)
            {
                this.m_disposing = true;
                this.m_resetEvent.Set();
                this.m_dispatchThread = null;
            }
            Console.ResetColor();
        }

        /// <summary>
        /// Trace event
        /// </summary>
        public override void TraceEventWithData(EventLevel level, string source, string message, object[] data)
        {
            foreach (var obj in data)
            {
                this.WriteTrace(level, source, String.Format("{0} - {1}", message, JsonConvert.SerializeObject(obj)));
            }
        }
    }
}