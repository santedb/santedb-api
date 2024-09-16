/*
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
using Newtonsoft.Json;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SanteDB.Core.Diagnostics.Tracing
{
    /// <summary>
    /// Timed Trace listener
    /// </summary>
    [DisplayName("File Trace Writer")]
    public class RolloverTextWriterTraceWriter : TraceWriter, IDisposable
    {
        // Dispatch thread
        private Thread m_dispatchThread = null;

        // True when disposing
        private bool m_disposing = false;

        // The log backlog
        private ConcurrentQueue<String> m_logBacklog = new ConcurrentQueue<string>();

        // Reset event
        private ManualResetEventSlim m_resetEvent = new ManualResetEventSlim(false);

        // File name reference
        private string m_fileName;

        private System.DateTime _currentDate;
        //System.IO.StreamWriter _traceWriter;
        //FileStream _stream;

        /// <summary>
        /// Filename
        /// </summary>
        public String FileName
        { get { return this.m_fileName; } }

        /// <summary>
        /// Rollover text writer ctor
        /// </summary>
        public RolloverTextWriterTraceWriter(EventLevel filter, string fileName, IDictionary<String, EventLevel> sources) : base(filter, fileName, sources)
        {
            // Pass in the path of the logfile (ie. C:\Logs\MyAppLog.log)
            // The logfile will actually be created with a yyyymmdd format appended to the filename
            this.m_fileName = fileName;
            if (!Path.IsPathRooted(fileName))
            {
                this.m_fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), Path.GetFileName(fileName));
            }

            // Create the directory?
            if (!Directory.Exists(Path.GetDirectoryName(this.m_fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.m_fileName));
            }
            //_stream = File.Open(generateFilename(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            //_stream.Seek(0, SeekOrigin.End);
            //_traceWriter = new StreamWriter(_stream);
            //_traceWriter.AutoFlush = true;

            // Start log dispatch
            this.m_dispatchThread = new Thread(this.LogDispatcherLoop);
            this.m_dispatchThread.IsBackground = true;
            this.m_dispatchThread.Start();

            var managerService = ApplicationServiceContext.Current?.GetService(typeof(ILogManagerService));
            if (managerService == null)
            {
                ApplicationServiceContext.Current.GetService<IServiceManager>()?.AddServiceProvider(typeof(RolloverLogManagerService));
            }

            var assemblyname = Assembly.GetEntryAssembly()?.GetName();
            this.WriteTrace(EventLevel.Informational, "Startup", "{0} Version: {1} logging at level [{2}]", assemblyname?.Name, assemblyname?.Version, filter);
        }

        /// <summary>
        /// Write a trace log
        /// </summary>
        protected override void WriteTrace(EventLevel level, string source, string format, params object[] args)
        {
            this.m_logBacklog.Enqueue(String.Format("{0}@{1} <{2}> [{3:o}]: {4}", source, Thread.CurrentThread.Name, level, DateTime.Now, String.Format(format, args)));
            //string dq = String.Format("{0}@{1} <{2}> [{3:o}]: {4}", source, Thread.CurrentThread.Name, level, DateTime.Now, String.Format(format, args));
            //using (TextWriter tw = File.AppendText(this.m_logFile))
            //    tw.WriteLine(dq); // This allows other threads to add to the write queue

            this.m_resetEvent.Set();
        }

        /// <summary>
        /// Generate the file name
        /// </summary>
        private string GenerateFilename()
        {
            _currentDate = System.DateTime.Today;
            return Path.Combine(Path.GetDirectoryName(this.m_fileName), Path.GetFileNameWithoutExtension(this.m_fileName) + "_" +
               _currentDate.ToString("yyyyMMdd") + Path.GetExtension(this.m_fileName));
        }

        /// <summary>
        /// Log dispatcher loop.
        /// </summary>
        private void LogDispatcherLoop()
        {
            while (true)
            {
                try
                {
                    if (this.m_disposing)
                    {
                        return; // shutdown dispatch
                    }

                    while (this.m_logBacklog.IsEmpty && !this.m_disposing)
                    {
                        this.m_resetEvent.Wait();
                        this.m_resetEvent.Reset();
                    }
                    if (this.m_disposing)
                    {
                        return;
                    }

                    // Use file stream
                    var fileName = this.GenerateFilename();
                    if (Directory.Exists(Path.GetDirectoryName(fileName)))
                    {
                        using (FileStream fs = File.Open(this.GenerateFilename(), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                        {
                            fs.Seek(0, SeekOrigin.End);
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                while (!this.m_logBacklog.IsEmpty)
                                {
                                    if (this.m_logBacklog.TryDequeue(out var dq))
                                    {
#if DEBUG
                                        sw.WriteLine(dq); // This allows other threads to add to the write queue
#else 
                                        var lines = dq.Split('\n', '\r').Where(o => !String.IsNullOrEmpty(o)).Take(3).ToArray(); // Take first three lines
                                        foreach (var itm in lines)
                                        {
                                            sw.WriteLine(itm); // This allows other threads to add to the write queue
                                        }
#endif
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        while (!this.m_logBacklog.IsEmpty) // exhaust the queue
                        {
                            this.m_logBacklog.TryDequeue(out string _);
                        }
                    }
                }
                catch
                {
                    ;
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
                // this.m_dispatchThread.Join(); // Abort thread
                this.m_dispatchThread = null;
            }
        }

        /// <summary>
        /// Write out data
        /// </summary>
        public override void TraceEventWithData(EventLevel level, string source, string message, object[] data)
        {
            foreach (var obj in data)
            {
                this.WriteTrace(level, source, String.Format("{0} - {1}", message, String.Join(",", obj)));
            }
        }
    }
}