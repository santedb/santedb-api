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

using SanteDB.Core.Diagnostics;
using System;
using System.Threading;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a thread pool which is implemented to wrap .NET thread pool
    /// </summary>
    /// <remarks>
    /// This class is a remnant / adaptation of the original thread pool service from OpenIZ because OpenIZ used PCL which
    /// didn't have a thread pool. Additionally it provided statistics on the thread pool load, etc. This has been
    /// refactored.
    /// </remarks>
    public class NetThreadPoolService : IThreadPoolService
    {
        /// <summary>
        /// Get the service name
        /// </summary>
        public String ServiceName => ".NET Thread Pool Integration";

        // Tracer for thread pool
        private Tracer m_tracer = Tracer.GetTracer(typeof(DefaultThreadPoolService));

        /// <summary>
        /// Errored workers
        /// </summary>
        private int m_activeWorkers = 0;

        // Dispatched workers
        private int m_dispatchedWorkers = 0;

        /// <summary>
        /// Queue user work item to the .NET ThreadPool
        /// </summary>
        /// <param name="action"></param>
        public void QueueUserWorkItem(Action<object> action)
        {
            this.QueueUserWorkItem(action, null);
        }

        /// <summary>
        /// Queue worker
        /// </summary>
        public void QueueUserWorkItem<TParm>(Action<TParm> action, TParm parm)
        {
            Interlocked.Increment(ref this.m_dispatchedWorkers);
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    Interlocked.Decrement(ref this.m_dispatchedWorkers);
                    Interlocked.Increment(ref this.m_activeWorkers);
                    action((TParm)o);
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceError("Unhandled ThreadPool Worker Error:  {0}", e);
                }
                finally
                {
                    Interlocked.Decrement(ref this.m_activeWorkers);
                }
            }, parm);
        }

        /// <summary>
        /// Get worker status
        /// </summary>
        public void GetWorkerStatus(out int totalWorkers, out int availableWorkers, out int waitingInQueue)
        {
            ThreadPool.GetMaxThreads(out totalWorkers, out _);
            ThreadPool.GetAvailableThreads(out availableWorkers, out _);
            availableWorkers = totalWorkers - this.m_activeWorkers;
            waitingInQueue = this.m_dispatchedWorkers - this.m_activeWorkers;
        }
    }
}