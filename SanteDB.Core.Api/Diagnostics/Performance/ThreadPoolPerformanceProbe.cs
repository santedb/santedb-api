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

using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SanteDB.Core.Diagnostics.Performance
{
    /// <summary>
    /// Represents a thread pool performance counter
    /// </summary>
    public class ThreadPoolPerformanceProbe : ICompositeDiagnosticsProbe
    {
        // Performance counters
        private readonly IDiagnosticsProbe[] m_performanceCounters;

        /// <summary>
        /// Generic performance counter
        /// </summary>
        private class QueueDepthProbe : DiagnosticsProbeBase<int>
        {
            private readonly IThreadPoolService m_threadPool;

            /// <summary>
            /// Non pooled workers counter
            /// </summary>
            public QueueDepthProbe(IThreadPoolService threadPool) : base("Queue Depth", "Shows the number of tasks waiting to be executed")
            {
                this.m_threadPool = threadPool;
            }

            /// <summary>
            /// Gets the identifier for the pool
            /// </summary>
            public override Guid Uuid => PerformanceConstants.ThreadPoolDepthCounter;

            /// <summary>
            /// Gets the value
            /// </summary>
            public override int Value
            {
                get
                {
                    this.m_threadPool.GetWorkerStatus(out _, out _, out int waitingInQueue);
                    return waitingInQueue;
                }
            }
        }

        /// <summary>
        /// Generic performance counter
        /// </summary>
        private class PooledWorkersProbe : DiagnosticsProbeBase<int>
        {
            private readonly IThreadPoolService m_threadPool;

            /// <summary>
            /// Non pooled workers counter
            /// </summary>
            public PooledWorkersProbe(IThreadPoolService threadPool) : base("Pool Use", "Shows the number of busy worker threads")
            {
                this.m_threadPool = threadPool;
            }

            /// <summary>
            /// Gets the identifier for the pool
            /// </summary>
            public override Guid Uuid => PerformanceConstants.ThreadPoolWorkerCounter;

            /// <summary>
            /// Gets the value
            /// </summary>
            public override int Value
            {
                get
                {
                    this.m_threadPool.GetWorkerStatus(out int totalWorkers, out int availableWorkers, out int waitingInQueue);
                    return totalWorkers - availableWorkers;
                }
            }
        }

        /// <summary>
        /// Generic performance counter
        /// </summary>
        private class PoolConcurrencyProbe : DiagnosticsProbeBase<int>
        {
            private readonly IThreadPoolService m_threadPool;

            /// <summary>
            /// Non pooled workers counter
            /// </summary>
            public PoolConcurrencyProbe(IThreadPoolService threadPool) : base("Thread pool size", "Shows the total number of threads which are allocated to the thread pool")
            {
                this.m_threadPool = threadPool;
            }

            /// <summary>
            /// Gets the identifier for the pool
            /// </summary>
            public override Guid Uuid => PerformanceConstants.ThreadPoolConcurrencyCounter;

            /// <summary>
            /// Gets the value
            /// </summary>
            public override int Value
            {
                get
                {
                    //ThreadPool.GetMaxThreads(out int workerCount, out int completionPort);
                    this.m_threadPool.GetWorkerStatus(out int workerCount, out _, out _);
                    return workerCount;
                }
            }
        }

        /// <summary>
        /// Thread pool performance probe
        /// </summary>
        public ThreadPoolPerformanceProbe(IThreadPoolService threadPoolService)
        {
            this.m_performanceCounters = new IDiagnosticsProbe[]
            {
                new PoolConcurrencyProbe(threadPoolService),
                new PooledWorkersProbe(threadPoolService),
                new QueueDepthProbe(threadPoolService)
            };
        }

        /// <summary>
        /// Get the UUID of the thread pool
        /// </summary>
        public Guid Uuid => PerformanceConstants.ThreadPoolPerformanceCounter;

        /// <summary>
        /// Gets the value of the
        /// </summary>
        public IEnumerable<IDiagnosticsProbe> Value => this.m_performanceCounters;

        /// <summary>
        /// Gets thename of hte composite
        /// </summary>
        public string Name => "Thread Pool";

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description => "The primary SanteDB thread pool performance monitor";

        /// <summary>
        /// Gets the type of the performance counter
        /// </summary>
        public Type Type => typeof(Array);

        /// <summary>
        /// Gets the value
        /// </summary>
        object IDiagnosticsProbe.Value => this.Value;
    }
}