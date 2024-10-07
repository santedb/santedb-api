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
using SanteDB.Core.Event;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Subscription;
using SanteDB.Core.Queue;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentDictionary<Type, List<PubSubSubscriptionDefinition>> m_subscriptionTypes;

        /// <summary>
        /// Bundle repository listener ctor
        /// </summary>
        public BundleRepositoryListener(IPubSubManagerService pubSubManager, IDispatcherQueueManagerService queueService, IServiceManager serviceManager, INotifyRepositoryService<Bundle> repositoryService) : base(pubSubManager, queueService, serviceManager, repositoryService, null)
        {
            this.m_queue = queueService;
            this.m_subscriptionTypes = new ConcurrentDictionary<Type, List<PubSubSubscriptionDefinition>>();
        }


        /// <summary>
        /// Add a subscription
        /// </summary>
        internal void AddSubscription(PubSubSubscriptionDefinition pubSubSubscriptionDefinition)
        {
            if (!this.m_subscriptionTypes.TryGetValue(pubSubSubscriptionDefinition.ResourceType, out var subs))
            {
                subs = new List<PubSubSubscriptionDefinition>();
                this.m_subscriptionTypes.TryAdd(pubSubSubscriptionDefinition.ResourceType, subs);
            }

            if (!subs.Any(s => s.Key == pubSubSubscriptionDefinition.Key || s.Name == pubSubSubscriptionDefinition.Name))
            {
                subs.Add(pubSubSubscriptionDefinition);
            }
        }

        /// <summary>
        /// Remove a subscription definition
        /// </summary>
        internal void RemoveSubscription(PubSubSubscriptionDefinition pubSubSubscriptionDefinition)
        {
            if (this.m_subscriptionTypes.TryGetValue(pubSubSubscriptionDefinition.ResourceType, out var subs))
            {
                subs.RemoveAll(o => o.Key == pubSubSubscriptionDefinition.Key || o.Name == pubSubSubscriptionDefinition.Name);
            }
        }



        /// <summary>
        /// Notify inserted
        /// </summary>
        protected override void OnInserted(object sender, DataPersistedEventArgs<Bundle> evt)
        {
            if (!this.m_subscriptionTypes.Any())
            {
                return;
            }

            foreach (var itm in evt.Data.Item.Where(i => !evt.Data.FocalObjects.Any() || evt.Data.FocalObjects.Contains(i.Key.GetValueOrDefault())))
            {

                if (this.m_subscriptionTypes.TryGetValue(itm.GetType(), out var subs))
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
                        case Model.DataTypes.BatchOperationType.Ignore:
                            return;
                    }

                    subs.FilterSubscriptionMatch(queueEntry.EventType, queueEntry.Data).ToList().ForEach(q => this.m_queue.Enqueue($"{PubSubBroker.QueueName}.{q.Name}", queueEntry));
                }
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
        protected override void OnDeleted(object sender, DataPersistedEventArgs<Bundle> evt)
        {
            this.OnInserted(sender, evt);
        }
    }
}