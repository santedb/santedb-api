/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a thread pool which is implemented separately from the default .net
    /// threadpool, this is to reduce the load on the .net framework thread pool
    /// </summary>
    /// <remarks>
    /// This class is a remnant / adaptation of the original thread pool service from OpenIZ because OpenIZ used PCL which 
    /// didn't have a thread pool. Additionally it provided statistics on the thread pool load, etc. This has been 
    /// refactored.
    /// </remarks>
    public class DefaultThreadPoolService : IThreadPoolService, IDisposable
    {

        // Lock
        private object s_lock = new object();

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DefaultThreadPoolService));

        // Number of threads to keep alive
        private int m_concurrencyLevel = System.Environment.ProcessorCount * 4;

        // Queue of work items
        private ConcurrentQueue<WorkItem> m_queue = null;

        // Active threads
        private Thread[] m_threadPool = null;

        // True when the thread pool is being disposed
        private bool m_disposing = false;

        // Reset event
        private ManualResetEventSlim m_resetEvent = new ManualResetEventSlim(false);

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Legacy Thread Pool Service";

        /// <summary>
        /// Creates a new instance of the wait thread pool
        /// </summary>
        public DefaultThreadPoolService()
        {
            this.EnsureStarted(); // Ensure thread pool threads are started
            this.m_queue = new ConcurrentQueue<WorkItem>();
        }

        /// <summary>
        /// Worker data structure
        /// </summary>
        private struct WorkItem
        {
            /// <summary>
            /// The callback to execute on the worker
            /// </summary>
            public Action<Object> Callback { get; set; }

            /// <summary>
            /// The state or parameter to the worker
            /// </summary>
            public object State { get; set; }

            /// <summary>
            /// The execution context
            /// </summary>
            public ExecutionContext ExecutionContext { get; set; }
        }

        /// <summary>
        /// Queue a work item to be completed
        /// </summary>
        public void QueueUserWorkItem(Action<Object> callback)
        {
            QueueUserWorkItem(callback, null);
        }


        /// <summary>
        /// Queue a user work item with the specified parameters
        /// </summary>
        public void QueueUserWorkItem<TParm>(Action<TParm> callback, TParm state)
        {
            this.QueueWorkItemInternal(callback, state);
        }

        /// <summary>
        /// Perform queue of workitem internally
        /// </summary>
        private void QueueWorkItemInternal<TParm>(Action<TParm> callback, TParm state)
        {
            ThrowIfDisposed();

            try
            {
                var wd = new WorkItem()
                {
                    Callback = (o) => callback((TParm)o),
                    State = state,
                    ExecutionContext = ExecutionContext.Capture()
                };

                m_queue.Enqueue(wd);
                this.m_resetEvent.Set();
            }
            catch (Exception e)
            {
                try
                {
                    this.m_tracer.TraceError("Error queueing work item: {0}", e);
                }
                catch { }
            }
        }

        /// <summary>
        /// Ensure the thread pool threads are started
        /// </summary>
        private void EnsureStarted()
        {
            // Load configuration
            if (this.m_threadPool == null)
            {
                this.m_concurrencyLevel = ApplicationServiceContext.Current?.GetService<IConfigurationManager>()?.GetSection<ApplicationServiceContextConfigurationSection>()?.ThreadPoolSize ?? this.m_concurrencyLevel;
                m_threadPool = new Thread[m_concurrencyLevel];
                for (int i = 0; i < m_threadPool.Length; i++)
                {
                    m_threadPool[i] = this.CreateThreadPoolThread();
                    m_threadPool[i].Start();
                }
            }
        }

        /// <summary>
        /// Create a thread pool thread
        /// </summary>
        private Thread CreateThreadPoolThread()
        {
            return new Thread(this.DispatchLoop)
            {
                Name = String.Format("SanteDB-ThreadPoolThread"),
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
        }

        /// <summary>
        /// Dispatch loop
        /// </summary>
        private void DispatchLoop()
        {
            while (!this.m_disposing)
            {
                try
                {
                    this.m_resetEvent.Wait();
                    while (this.m_queue.TryDequeue(out WorkItem wi))
                    {
                        wi.Callback(wi.State);
                    }
                    this.m_resetEvent.Reset();
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceError("Error in dispatchloop {0}", e);
                }
            }
        }

        /// <summary>
        /// Throw an exception if the object is disposed
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (this.m_disposing) throw new ObjectDisposedException(nameof(DefaultThreadPoolService));
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {

            if (this.m_disposing) return;

            this.m_disposing = true;

            this.m_resetEvent.Set();

            if (m_threadPool != null)
            {
                for (int i = 0; i < m_threadPool.Length; i++)
                {
                    if (!m_threadPool[i].Join(1000))
                        m_threadPool[i].Abort();
                    m_threadPool[i] = null;
                }
            }
        }
    }
}
