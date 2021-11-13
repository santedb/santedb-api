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

using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Core.PubSub.Broker
{
    /// <summary>
    /// Represents a class which listens to a repository and notifies the various subscriptions
    /// </summary>
    public class PubSubRepositoryListener<TModel> : IDisposable where TModel : IdentifiedData
    {
        // Cached filter criteria
        private Dictionary<Guid, Func<Object, bool>> m_filterCriteria = new Dictionary<Guid, Func<Object, bool>>();

        // The repository this listener listens to
        private INotifyRepositoryService<TModel> m_repository;

        // Queue service
        private IPersistentQueueService m_queueService;

        // Service manager
        private IServiceManager m_serviceManager;

        // Merge service
        private IRecordMergingService<TModel> m_mergeService;

        // Manager
        private IPubSubManagerService m_pubSubManager;

        // Thread pool
        private IThreadPoolService m_threadPool;

        /// <summary>
        /// Constructs a new repository listener
        /// </summary>
        public PubSubRepositoryListener(IThreadPoolService threadPool, IPubSubManagerService pubSubManager, IPersistentQueueService queueService, IServiceManager serviceManager)
        {
            this.m_pubSubManager = pubSubManager;
            this.m_repository = ApplicationServiceContext.Current.GetService<INotifyRepositoryService<TModel>>();
            this.m_queueService = queueService;
            this.m_serviceManager = serviceManager;
            this.m_threadPool = threadPool;

            if (this.m_repository == null)
                throw new InvalidOperationException($"Cannot subscribe to {typeof(TModel).FullName} as this repository does not raise events");

            this.m_repository.Inserted += OnInserted;
            this.m_repository.Saved += OnSaved;
            this.m_repository.Obsoleted += OnObsoleted;

            this.m_mergeService = ApplicationServiceContext.Current.GetService<IRecordMergingService<TModel>>();
            if (this.m_mergeService != null)
            {
                this.m_mergeService.Merged += OnMerged;
                this.m_mergeService.UnMerged += OnUnmerged;
            }
        }

        /// <summary>
        /// Get all dispatchers and subscriptions
        /// </summary>
        protected IEnumerable<IPubSubDispatcher> GetDispatchers(PubSubEventType eventType, Object data)
        {
            using (AuthenticationContext.EnterSystemContext())
            {
                var resourceName = data.GetType().GetSerializationName();
                var subscriptions = this.m_pubSubManager
                        .FindSubscription(o => o.ResourceTypeXml == resourceName && o.IsActive && (o.NotBefore == null || o.NotBefore < DateTimeOffset.Now) && (o.NotAfter == null || o.NotAfter > DateTimeOffset.Now))
                        .OfType<PubSubSubscriptionDefinition>()
                        .Where(o => o.Event.HasFlag(eventType))
                        .Where(s =>
                        {
                            // Attempt to compile the filter criteria into an executable function
                            if (!this.m_filterCriteria.TryGetValue(s.Key.Value, out Func<Object, bool> fn))
                            {
                                Expression dynFn = null;
                                var parameter = Expression.Parameter(data.GetType());

                                foreach (var itm in s.Filter)
                                {
                                    var fFn = QueryExpressionParser.BuildLinqExpression(data.GetType(), NameValueCollection.ParseQueryString(itm), "p", forceLoad: true, lazyExpandVariables: true);
                                    if (dynFn is LambdaExpression le)
                                        dynFn = Expression.Lambda(
                                            Expression.And(
                                                Expression.Invoke(le, parameter),
                                                Expression.Invoke(fFn, parameter)
                                               ), parameter);
                                    else
                                        dynFn = fFn;
                                }

                                if (dynFn == null)
                                {
                                    dynFn = Expression.Lambda(Expression.Constant(true), parameter);
                                }
                                parameter = Expression.Parameter(typeof(object));
                                fn = Expression.Lambda(Expression.Invoke(dynFn, Expression.Convert(parameter, data.GetType())), parameter).Compile() as Func<Object, bool>;
                                this.m_filterCriteria.Add(s.Key.Value, fn);
                            }
                            return fn(data);
                        });

                // Now we want to filter by channel, since the channel is really what we're interested in
                foreach (var chnl in subscriptions.GroupBy(o => o.ChannelKey))
                {
                    var channelDef = this.m_pubSubManager.GetChannel(chnl.Key);
                    var factory = this.m_serviceManager.CreateInjected(channelDef.DispatcherFactoryType) as IPubSubDispatcherFactory;
                    yield return factory.CreateDispatcher(chnl.Key, new Uri(channelDef.Endpoint), channelDef.Settings.ToDictionary(o => o.Name, o => o.Value));
                }
            }
        }

        /// <summary>
        /// When unmerged
        /// </summary>
        protected virtual void OnUnmerged(object sender, Event.DataMergeEventArgs<TModel> evt)
        {
            this.m_threadPool.QueueUserWorkItem(e =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    foreach (var dsptchr in this.GetDispatchers(PubSubEventType.UnMerge, this.m_repository.Get(e.SurvivorKey)))
                        dsptchr.NotifyUnMerged(this.m_repository.Get(e.SurvivorKey), e.LinkedKeys.Select(o => this.m_repository.Get(o)).ToArray());
                }
            }, evt);
        }

        /// <summary>
        /// When merged
        /// </summary>
        protected virtual void OnMerged(object sender, Event.DataMergeEventArgs<TModel> evt)
        {
            this.m_threadPool.QueueUserWorkItem(e =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Merge, this.m_repository.Get(e.SurvivorKey)))
                        dsptchr.NotifyMerged(this.m_repository.Get(e.SurvivorKey), e.LinkedKeys.Select(o => this.m_repository.Get(o)).ToArray());
                }
            }, evt);
        }

        /// <summary>
        /// When obsoleted
        /// </summary>
        protected virtual void OnObsoleted(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.m_threadPool.QueueUserWorkItem(e =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Delete, e.Data))
                        dsptchr.NotifyObsoleted(e.Data);
                }
            }, evt);
        }

        /// <summary>
        /// When saved (updated)
        /// </summary>
        protected virtual void OnSaved(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.m_threadPool.QueueUserWorkItem(e =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Update, e.Data))
                        dsptchr.NotifyUpdated(e.Data);
                }
            }, evt);
        }

        /// <summary>
        /// When inserted
        /// </summary>
        protected virtual void OnInserted(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.m_threadPool.QueueUserWorkItem(e =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, e.Data))
                        dsptchr.NotifyCreated(e.Data);
                }
            }, evt);
        }

        /// <summary>
        /// Dispose of this
        /// </summary>
        public void Dispose()
        {
            if (this.m_repository != null)
            {
                this.m_repository.Inserted -= this.OnInserted;
                this.m_repository.Obsoleted -= this.OnObsoleted;
                this.m_repository.Saved -= this.OnSaved;
                this.m_repository = null;
            }
            if (this.m_mergeService != null)
            {
                this.m_mergeService.Merged -= this.OnMerged;
                this.m_mergeService.UnMerged -= this.OnUnmerged;
            }
        }
    }
}