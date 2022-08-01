/*
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
 * Date: 2021-8-27
 */
using SanteDB.Core.Event;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Queue;
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
        private IDispatcherQueueManagerService m_queue;

        /// <summary>
        /// Bundle repository listener ctor
        /// </summary>
        public BundleRepositoryListener(IPubSubManagerService pubSubManager, IDispatcherQueueManagerService queueService, IServiceManager serviceManager) : base(pubSubManager, queueService, serviceManager)
        {
            this.m_queue = queueService;
        }

        /// <summary>
        /// Notify inserted
        /// </summary>
        protected override void OnInserted(object sender, DataPersistedEventArgs<Bundle> evt)
        {
            foreach (var itm in evt.Data.Item.Where(i => !evt.Data.FocalObjects.Any() || evt.Data.FocalObjects.Contains(i.Key.Value)))
            {
                PubSubNotifyQueueEntry queueEntry = null;

                switch (itm.BatchOperation)
                {
                    case Model.DataTypes.BatchOperationType.Auto:
                    case Model.DataTypes.BatchOperationType.InsertOrUpdate:
                    case Model.DataTypes.BatchOperationType.Insert:
                        queueEntry = new PubSubNotifyQueueEntry(itm.GetType(), PubSubEventType.Create, itm);
                        break;

                    case Model.DataTypes.BatchOperationType.Update:
                        queueEntry = new PubSubNotifyQueueEntry(itm.GetType(), PubSubEventType.Update, itm);
                        break;

                    case Model.DataTypes.BatchOperationType.Delete:
                        queueEntry = new PubSubNotifyQueueEntry(itm.GetType(), PubSubEventType.Delete, itm);
                        break;
                }

                this.m_queue.Enqueue(PubSubBroker.QueueName, queueEntry);
            }
        }

        /// <summary>
        /// Notify inserted
        /// </summary>
        protected override void OnSaved(object sender, DataPersistedEventArgs<Bundle> evt)
        {
            this.OnInserted(sender, evt);
        }

        /// <summary>
        /// Notify obsoleted
        /// </summary>
        protected override void OnObsoleted(object sender, DataPersistedEventArgs<Bundle> evt)
        {
            this.OnInserted(sender, evt);
        }
    }
}