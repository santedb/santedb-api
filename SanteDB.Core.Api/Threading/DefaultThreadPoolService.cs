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
    /// <para>
    /// Many SanteDB jobs use threads to perform background tasks such as refreshes, matching,
    /// job execution, etc. Because we don't want uncontrolled explosion of threads, we use a thread pool 
    /// in order to control the number of active threads which are being used.
    /// </para>
    /// <para>
    /// Implementers may choose to register a <see cref="IThreadPoolService"/> which uses the .NET <see cref="ThreadPool"/> (see: <see cref="NetThreadPoolService"/>), 
    /// or they can choose to use this separate thread pool service. There are several advantages to using the SanteDB thread pool rather than the 
    /// .NET thread pool including:
    /// </para>
    /// <list type="bullet">
    ///     <item>The default .NET thread pool can bounce work between threads when the thread enters a wait state, this can cause issues with the REDIS connection multiplexer</item>
    ///     <item>SanteDB plugins may use PLINQ or other TPL libraries which require using .NET thread pool - and we don't want longer running processess consuming those threads</item>
    ///     <item>Implementers may wish to have more control over how the Thread pool uses resources</item>
    /// </list>
    /// <para>This thread pool works by spinning up a pool of threads which wait for <see cref="QueueUserWorkItem(Action{object})"/> which initiates (or queues) a request to 
    /// perform background work. When an available thread in the pool can execute the task, the task will be run and the thread will work on the next work item.</para>
    /// <para>If the thread pool needs additional threads (i.e. there are a lot of items in the backlog) it will spin up new reserved threads at a rate of 
    /// number of CPUs on the machine. This continues until the environment variable SDB_MAX_THREADS_PER_CPU is hit.</para>
    /// <para>Conversely, if the thread pool threads remain idle for too long (1 minute) they are destroyed and removed from the thread pool. This ensures over-threading
    /// is not done on the host machine.</para>
    /// </remarks>
    public class DefaultThreadPoolService : IThreadPoolService, IDisposable
    {
        // Lock
        private object s_lock = new object();

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultThreadPoolService));

        /// <summary>
        /// Maximum concurrency for the thread pool
        /// </summary>
        public const string MAX_CONCURRENCY = "SDB_THREADS_PER_CPU";

        // Number of threads to keep alive
        private readonly int m_maxConcurrencyLevel;

        // Queue of work items
        private ConcurrentQueue<WorkItem> m_queue = null;

        // Active threads
        private Thread[] m_threadPool = null;

        // True when the thread pool is being disposed
        private bool m_disposing = false;

        // The number of busy workers
        private long m_busyWorkers = 0;

        // Min pool workers
        private readonly int m_minPoolWorkers = Environment.ProcessorCount < 4 ? Environment.ProcessorCount * 2 : Environment.ProcessorCount;

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
            var envMaxThreads = Environment.GetEnvironmentVariable(MAX_CONCURRENCY);
            if (!String.IsNullOrEmpty(envMaxThreads) && int.TryParse(envMaxThreads, out var maxThreadsPerCpu))
            {
                this.m_maxConcurrencyLevel = Environment.ProcessorCount * maxThreadsPerCpu;
            }
            else
            {
                this.m_maxConcurrencyLevel = Environment.ProcessorCount * 12;
            }
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
                this.GrowPoolSize();
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
        /// Grow the pool if needed
        /// </summary>
        private void GrowPoolSize()
        {
            if (!this.m_queue.IsEmpty &&  // This method is fast
                        this.m_queue.Count > 0 && // This requires a lock so only do if not empty
                        this.m_threadPool.Length < this.m_maxConcurrencyLevel) // we have room to allocate new threads
            {
                lock (s_lock)
                {
                    if (this.m_queue.Count > this.m_threadPool.Length - this.m_busyWorkers &&
                        this.m_threadPool.Length < this.m_maxConcurrencyLevel)  // Re-check after lock taken
                    {
                        Array.Resize(ref this.m_threadPool, this.m_threadPool.Length + Environment.ProcessorCount); // allocate processor count threads
                        for (var i = 0; i < this.m_threadPool.Length; i++)
                        {
                            if (this.m_threadPool[i] == null)
                            {
                                this.m_threadPool[i] = this.CreateThreadPoolThread(i);
                                this.m_threadPool[i].Start();
                            }
                        }
                    }
                }
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
                m_threadPool = new Thread[this.m_minPoolWorkers];
                for (int i = 0; i < m_threadPool.Length; i++)
                {
                    m_threadPool[i] = this.CreateThreadPoolThread(i);
                    m_threadPool[i].Start();
                }
            }
        }

        /// <summary>
        /// Create a thread pool thread
        /// </summary>
        private Thread CreateThreadPoolThread(int threadNumber)
        {
            return new Thread(this.DispatchLoop)
            {
                Name = String.Format($"SanteDB-ThreadPoolThread-{threadNumber}"),
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
        }

        /// <summary>
        /// Dispatch loop
        /// </summary>
        private void DispatchLoop()
        {
            long lastActivityJobTime = DateTime.Now.Ticks;
            int threadPoolIndex = Array.IndexOf(this.m_threadPool, Thread.CurrentThread);

            while (!this.m_disposing)
            {
                try
                {
                    this.m_resetEvent.Wait(3000);

                    if (threadPoolIndex >= this.m_minPoolWorkers &&
                        this.m_queue.IsEmpty &&
                        DateTime.Now.Ticks - lastActivityJobTime > TimeSpan.TicksPerMinute) // shrink the pool
                    {
                        lock (s_lock)
                        {
                            threadPoolIndex = Array.IndexOf(this.m_threadPool, Thread.CurrentThread);
                            this.m_threadPool[threadPoolIndex] = this.m_threadPool[this.m_threadPool.Length - 1];
                            Array.Resize(ref this.m_threadPool, this.m_threadPool.Length - 1);
                        }
                        return;
                    }
                    else
                    {
                        while (this.m_queue.TryDequeue(out WorkItem wi))
                        {
                            try
                            {
                                lastActivityJobTime = DateTime.Now.Ticks;
                                Interlocked.Increment(ref this.m_busyWorkers);
                                wi.Callback(wi.State);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref this.m_busyWorkers);
                            }
                        }
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
            if (this.m_disposing)
            {
                throw new ObjectDisposedException(nameof(DefaultThreadPoolService));
            }
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            if (this.m_disposing)
            {
                return;
            }

            this.m_disposing = true;

            this.m_resetEvent.Set();

            if (m_threadPool != null)
            {
                for (int i = 0; i < m_threadPool.Length; i++)
                {
                    try
                    {
                        m_threadPool[i]?.Abort();
                    }
                    catch (PlatformNotSupportedException)
                    {
                        //TODO: we need to properly cancel the pool threads using a cancellationtoken.
                    }
                    m_threadPool[i] = null;
                }
            }
        }

        /// <summary>
        /// Get worker status
        /// </summary>
        public void GetWorkerStatus(out int totalWorkers, out int availableWorkers, out int waitingQueue)
        {
            totalWorkers = this.m_threadPool.Length;
            availableWorkers = totalWorkers - (int)Interlocked.Read(ref this.m_busyWorkers);
            waitingQueue = this.m_queue.Count;
        }
    }
}