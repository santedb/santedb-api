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
using SanteDB.Core.Event;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System.Linq;

namespace SanteDB.Core.PubSub.Broker
{
    /// <summary>
    /// Chained bundle repository listener
    /// </summary>
    internal class BundleRepositoryListener : PubSubRepositoryListener<Bundle>
    {

        // Thread pool
        private IThreadPoolService m_threadPool;

        /// <summary>
        /// Bundle repository listener ctor
        /// </summary>
        public BundleRepositoryListener(IThreadPoolService threadPool, IPubSubManagerService pubSubManager, IPersistentQueueService queueService, IServiceManager serviceManager) : base(threadPool, pubSubManager, queueService, serviceManager)
        {
            this.m_threadPool = threadPool;
        }

        /// <summary>
        /// Notify inserted
        /// </summary>
        protected override void OnInserted(object sender, DataPersistedEventArgs<Bundle> evt)
        {

            this.m_threadPool.QueueUserWorkItem(e =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    foreach (var itm in e.Data.Item.Where(i => e.Data.FocalObjects.Contains(i.Key.Value)))
                    {
                        switch (itm.BatchOperation)
                        {
                            case Model.DataTypes.BatchOperationType.Auto:
                            case Model.DataTypes.BatchOperationType.InsertOrUpdate:
                            case Model.DataTypes.BatchOperationType.Insert:
                                foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, itm))
                                    dsptchr.NotifyCreated(itm);
                                break;
                            case Model.DataTypes.BatchOperationType.Update:
                                foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, itm))
                                    dsptchr.NotifyUpdated(itm);
                                break;
                            case Model.DataTypes.BatchOperationType.Obsolete:
                                foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, itm))
                                    dsptchr.NotifyObsoleted(itm);
                                break;
                        }
                    }
                }
            }, evt);
        }

        /// <summary>
        /// Notify inserted
        /// </summary>
        protected override void OnSaved(object sender, DataPersistedEventArgs<Bundle> evt)
        {
            this.m_threadPool.QueueUserWorkItem(e =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    foreach (var itm in e.Data.Item.Where(i => e.Data.FocalObjects.Contains(i.Key.Value)))
                    {
                        foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, itm))
                            dsptchr.NotifyUpdated(itm);
                    }
                }
            }, evt);
        }

        /// <summary>
        /// Notify obsoleted
        /// </summary>
        protected override void OnObsoleted(object sender, DataPersistedEventArgs<Bundle> evt)
        {
            this.m_threadPool.QueueUserWorkItem(e =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    foreach (var itm in e.Data.Item.Where(i => e.Data.FocalObjects.Contains(i.Key.Value)))
                    {
                        foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, itm))
                            dsptchr.NotifyObsoleted(itm);
                    }
                }
            }, evt);
        }
    }
}
