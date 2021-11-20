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
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Parameters;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Queue;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Core.PubSub.Broker
{
    /// <summary>
    /// A pub-sub broker which is responsible for actually facilitating the publish/subscribe
    /// logic
    /// </summary>
    public class PubSubBroker : IDaemonService, IDisposable
    {
        /// <summary>
        /// Queue name
        /// </summary>
        public const string QueueName = "sys.pubsub";

        // Cached filter criteria
        private ConcurrentDictionary<Guid, Func<Object, bool>> m_filterCriteria = new ConcurrentDictionary<Guid, Func<Object, bool>>();

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(PubSubBroker));

        // Lock
        private object m_lock = new object();

        // Pub sub manager
        private IPubSubManagerService m_pubSubManager;

        /// <summary>
        /// Repository listeners
        /// </summary>
        private List<IDisposable> m_repositoryListeners = null;

        /// <summary>
        /// True if the broker is running
        /// </summary>
        public bool IsRunning => this.m_repositoryListeners?.Count > 0;

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "PubSub Notification Broker";

        /// <summary>
        /// The service is starting
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// The service is started
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// The service is stopping
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// The service is stopped
        /// </summary>
        public event EventHandler Stopped;

        // Service manager
        private IServiceManager m_serviceManager;

        // Queue service
        private IDispatcherQueueManagerService m_queue;

        /// <summary>
        /// Create a new pub-sub broker
        /// </summary>
        public PubSubBroker(IServiceManager serviceManager, IDispatcherQueueManagerService queueService, IPubSubManagerService pubSubManager)
        {
            this.m_serviceManager = serviceManager;
            this.m_queue = queueService;
            this.m_pubSubManager = pubSubManager;
            // Create necessary service listener
            this.m_pubSubManager.Subscribed += this.PubSubSubscribed;
            this.m_pubSubManager.UnSubscribed += this.PubSubUnSubscribed;

            queueService.Open(QueueName);
            queueService.SubscribeTo(QueueName, this.NotificationQueued);
            queueService.Open($"{QueueName}.dead");
        }

        /// <summary>
        /// Notification has been queued
        /// </summary>
        private void NotificationQueued(DispatcherMessageEnqueuedInfo e)
        {
            if (e.QueueName == QueueName)
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    Object queueObject = null;
                    while ((queueObject = this.m_queue.Dequeue(QueueName)) is DispatcherQueueEntry dq && dq.Body is PubSubNotifyQueueEntry evtData)
                    {
                        try
                        {
                            foreach (var dsptchr in this.GetDispatchers(evtData.EventType, evtData.Data))
                            {
                                switch (evtData.EventType)
                                {
                                    case PubSubEventType.Create:
                                        dsptchr.NotifyCreated(evtData.Data as IdentifiedData);
                                        break;

                                    case PubSubEventType.Delete:
                                        dsptchr.NotifyObsoleted(evtData.Data as IdentifiedData);
                                        break;

                                    case PubSubEventType.Update:
                                        dsptchr.NotifyUpdated(evtData.Data as IdentifiedData);
                                        break;

                                    case PubSubEventType.Merge:
                                        {
                                            if (evtData.Data is ParameterCollection pc && pc.TryGet("survivor", out IdentifiedData survivor) && pc.TryGet("linkedDuplicates", out IEnumerable<IdentifiedData> duplicates))
                                            {
                                                dsptchr.NotifyMerged(survivor, duplicates);
                                            }
                                            break;
                                        }
                                    case PubSubEventType.UnMerge:
                                        {
                                            if (evtData.Data is ParameterCollection pc && pc.TryGet("survivor", out IdentifiedData survivor) && pc.TryGet("linkedDuplicates", out IEnumerable<IdentifiedData> duplicates))
                                            {
                                                dsptchr.NotifyUnMerged(survivor, duplicates);
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.m_tracer.TraceError("Error dispatching notification from PubSub broker: {0}", ex);
                            this.m_queue.Enqueue($"{QueueName}.dead", evtData);
                        }
                    }
                }
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
                                this.m_filterCriteria.TryAdd(s.Key.Value, fn);
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
        /// Dispose of the listeners
        /// </summary>
        public void Dispose()
        {
            if (this.m_repositoryListeners != null)
                foreach (var itm in this.m_repositoryListeners)
                    itm.Dispose();
            this.m_repositoryListeners = null;
            this.m_queue.UnSubscribe(QueueName, this.NotificationQueued);
        }

        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);
            this.m_repositoryListeners = new List<IDisposable>();

            ApplicationServiceContext.Current.Started += (o, e) =>
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    try
                    {
                        // Hook up the listeners for existing
                        foreach (var psd in this.m_pubSubManager.FindSubscription(x => x.IsActive == true))
                        {
                            this.PubSubSubscribed(this, new Event.DataPersistedEventArgs<PubSubSubscriptionDefinition>(psd, TransactionMode.Commit, AuthenticationContext.SystemPrincipal));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.m_tracer.TraceWarning("Cannot wire up subscription broker - {0}", ex);
                    }
                }
            };
            this.m_repositoryListeners.Add(this.m_serviceManager.CreateInjected<BundleRepositoryListener>());

            this.Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Fired when the pub-sub manager has indicated a subscription has been removed
        /// </summary>
        private void PubSubUnSubscribed(object sender, Event.DataPersistedEventArgs<PubSubSubscriptionDefinition> e)
        {
            lock (this.m_lock)
            {
                // If there are no further types subscribed then remove the listener
                if (this.m_pubSubManager.FindSubscription(o => o.ResourceType == e.Data.ResourceType).Count() == 0)
                {
                    var lt = typeof(PubSubRepositoryListener<>).MakeGenericType(e.Data.ResourceType);
                    var listener = this.m_repositoryListeners.FirstOrDefault(o => lt.GetType().Equals(o.GetType()));
                    listener.Dispose();
                    this.m_repositoryListeners.Remove(listener);
                }
            }
        }

        /// <summary>
        /// Pub-sub definition has been subscribed
        /// </summary>
        private void PubSubSubscribed(object sender, Event.DataPersistedEventArgs<PubSubSubscriptionDefinition> e)
        {
            lock (this.m_lock)
            {
                var lt = typeof(PubSubRepositoryListener<>).MakeGenericType(e.Data.ResourceType);
                if (!this.m_repositoryListeners.Any(o => o.GetType().Equals(lt)))
                    this.m_repositoryListeners.Add(this.m_serviceManager.CreateInjected(lt) as IDisposable);
            }
        }

        /// <summary>
        /// Stop the service
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            // Call dispose which will clean up the listeners
            this.Dispose();
            this.m_repositoryListeners = null;
            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}