/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-6-21
 */
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
namespace SanteDB.Core.Data.Management
{
    /// <summary>
    /// A context which is used by the SIM management service to perform background matching and writing
    /// </summary>
    internal class BackgroundMatchContext<TEntity> : IDisposable
        where TEntity : IdentifiedData, new()
    {

        private readonly ConcurrentStack<TEntity> m_entityStack = new ConcurrentStack<TEntity>();
        private readonly ISimResourceInterceptor m_resourceInterceptor;
        private readonly IDataPersistenceService<Bundle> m_bundlePersistence;
        private readonly IThreadPoolService m_threadPool;
        private readonly ManualResetEventSlim m_processEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim m_loadEvent = new ManualResetEventSlim(true);
        private readonly int m_maxWorkers;
        private Exception m_haltException;
        private int m_loadedRecords = 0;
        private int m_availableWorkers;
        private int m_recordsProcessed = 0;
        private bool m_isRunning = true;

        /// <summary>
        /// True if the background processors should be executing
        /// </summary>
        public bool IsRunning => this.m_isRunning;

        /// <summary>
        /// Records processed
        /// </summary>
        public int RecordsProcessed => this.m_recordsProcessed;

        /// <summary>
        /// Create new background matching context
        /// </summary>
        public BackgroundMatchContext(int maxWorkers, ISimResourceInterceptor resourceInterceptor)
        {
            this.m_maxWorkers = this.m_availableWorkers = maxWorkers;
            this.m_resourceInterceptor = resourceInterceptor;
            this.m_bundlePersistence = ApplicationServiceContext.Current.GetService<IDataPersistenceService<Bundle>>();
            this.m_threadPool = ApplicationServiceContext.Current.GetService<IThreadPoolService>();
        }

        /// <summary>
        /// Halt processing 
        /// </summary>
        public void Halt(Exception e)
        {
            this.m_haltException = e;
        }

        /// <summary>
        /// Queue a loaded record
        /// </summary>
        public void QueueLoadedRecord(TEntity record)
        {
            // Main thread
            if (this.m_haltException != null)
            {
                throw this.m_haltException;
            }

            if (this.m_loadEvent.Wait(1000))
            {  // ensure that we are allowed to add or wait for avialable worker
                this.m_entityStack.Push(record);
                if (this.m_loadedRecords++ % 16 == 0)
                {
                    this.m_processEvent.Set(); // Signal the processing threads that they may process
                }
            }
        }

        /// <summary>
        /// De-queue a loaded record
        /// </summary>
        public int DeQueueLoadedRecords(TEntity[] records)
        {
            if (this.m_processEvent.Wait(1000))
            {
                var retVal = this.m_entityStack.TryPopRange(records);
                if (retVal > 0)
                {
                    if (Interlocked.Decrement(ref this.m_availableWorkers) == 0)
                    {
                        this.m_loadEvent.Reset();
                    }
                }

                return retVal;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Release a worker
        /// </summary>
        public void ReleaseWorker(int recordsProcessed)
        {
            if (Interlocked.Increment(ref this.m_availableWorkers) > 0)
            {
                this.m_loadEvent.Set(); // Allow loading of records
                this.m_processEvent.Reset();
            }
            Interlocked.Add(ref this.m_recordsProcessed, recordsProcessed);

        }

        /// <summary>
        /// Dispose of this context
        /// </summary>
        public void Dispose()
        {
            this.m_isRunning = false;
            this.WaitUntilFinished();
            this.m_loadEvent.Dispose();
            this.m_processEvent.Dispose();
        }

        /// <summary>
        /// Run the background context
        /// </summary>
        public void Start()
        {
            this.m_isRunning = true;
            for (var w = 0; w < this.m_maxWorkers; w++)
            {
                this.m_threadPool.QueueUserWorkItem(this.WorkerThreadLogic);
            }
        }

        /// <summary>
        /// Wait until all of the records are completed processing
        /// </summary>
        /// <param name="msWait">The number of milliseconds to wait</param>
        public void WaitUntilFinished()
        {
            while (this.m_availableWorkers != this.m_maxWorkers || // still someone processing
                    !this.m_entityStack.IsEmpty && this.m_haltException == null)
            {
                this.m_processEvent.Set();
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Run the matching logic for one worker thread
        /// </summary>
        private void WorkerThreadLogic(object state)
        {
            try
            {
                while (this.IsRunning)
                {
                    var records = new TEntity[16];
                    var nRecords = 0;
                    while ((nRecords = this.DeQueueLoadedRecords(records)) > 0)
                    {
                        try
                        {
                            var matches = records.Take(nRecords).SelectMany(o => this.m_resourceInterceptor.DoMatchingLogic(o));
                            this.m_bundlePersistence.Insert(new Bundle(matches), TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                        }
                        finally
                        {
                            this.ReleaseWorker(nRecords);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.Halt(e);
            }
        }
    }
}
