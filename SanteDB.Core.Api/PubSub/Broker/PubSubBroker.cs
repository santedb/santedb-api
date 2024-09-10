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
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.Data.Quality;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Parameters;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Queue;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;

namespace SanteDB.Core.PubSub.Broker
{
    /// <summary>
    /// An implementation of the <see cref="IDaemonService"/> which is responsible for monitoring
    /// the <see cref="IPubSubManagerService"/> for new subscription events
    /// </summary>
    /// <remarks>
    /// <para>The Pub/Sub broker is the central daemon which is responsible for coordinating outbound notifications based on 
    /// filters established by subscribers. The broker:</para>
    /// <list type="number">
    ///     <item>Listens to the <c>Subscribe</c> event on the <see cref="IPubSubManagerService"/> and creates a callback for the data event</item>
    ///     <item>Enqueues any new data to the <see cref="IDispatcherQueueManagerService"/> for sending to outbound recipients</item>
    ///     <item>When a <see cref="IDispatcherQueueManagerService"/> message is enqueued, loads the appropriate <see cref="IPubSubDispatcherFactory"/> and publishes the notification</item>
    /// </list>
    /// </remarks>
    [Description("Publish Subscribe Broker")]
    public class PubSubBroker : IDaemonService, IDisposable
    {
        /// <summary>
        /// Queue name
        /// </summary>
        public const string QueueName = "sys.pubsub";

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(PubSubBroker));

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
        private IDispatcherQueueManagerService m_queueService;

        /// <summary>
        /// Create a new pub-sub broker
        /// </summary>
        public PubSubBroker(IServiceManager serviceManager, IDispatcherQueueManagerService queueService, IPubSubManagerService pubSubManager)
        {
            this.m_serviceManager = serviceManager;
            this.m_queueService = queueService;
            this.m_pubSubManager = pubSubManager;
            // Create necessary service listener
            this.m_pubSubManager.Subscribed += this.PubSubSubscribe;
            this.m_pubSubManager.UnSubscribed += this.PubSubUnSubscribed;
            this.m_pubSubManager.Activated += (o, e) =>
            {
                this.PubSubSubscribe(o, e);
            };

        }

        /// <summary>
        /// Notification has been queued
        /// </summary>
        private void NotificationQueued(DispatcherMessageEnqueuedInfo e)
        {
            if (e.QueueName.StartsWith(QueueName))
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    Object queueObject = null;
                    while ((queueObject = this.m_queueService.Dequeue(e.QueueName)) is DispatcherQueueEntry dq && dq.Body is PubSubNotifyQueueEntry evtData)
                    {
                        try
                        {
                            var dsptchr = this.GetDispatcher(e.QueueName);
                            try
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
                                            if (evtData.Data is ParameterCollection pc && pc.TryGet("survivor", out IdentifiedData survivor) && pc.TryGet("linkedDuplicates", out Bundle duplicates))
                                            {
                                                dsptchr.NotifyMerged(survivor, duplicates.Item);
                                            }
                                            break;
                                        }
                                    case PubSubEventType.UnMerge:
                                        {
                                            if (evtData.Data is ParameterCollection pc && pc.TryGet("survivor", out IdentifiedData survivor) && pc.TryGet("linkedDuplicates", out Bundle duplicates))
                                            {
                                                dsptchr.NotifyUnMerged(survivor, duplicates.Item);
                                            }
                                            break;
                                        }
                                    case PubSubEventType.Link:
                                        {
                                            if (evtData.Data is ParameterCollection pc && pc.TryGet("holder", out IdentifiedData holder) && pc.TryGet("target", out IdentifiedData target))
                                            {
                                                dsptchr.NotifyLinked(holder, target);
                                            }
                                            break;
                                        }
                                    case PubSubEventType.UnLink:
                                        {
                                            if (evtData.Data is ParameterCollection pc && pc.TryGet("holder", out IdentifiedData holder) && pc.TryGet("target", out IdentifiedData target))
                                            {
                                                dsptchr.NotifyUnlinked(holder, target);
                                            }
                                            break;
                                        }
                                }
                            }
                            finally
                            {
                                if (dsptchr is IDisposable disp)
                                {
                                    disp.Dispose();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.m_tracer.TraceError("Error dispatching notification from PubSub broker: {0}", ex);
                            this.m_queueService.Enqueue($"{e.QueueName}.dead", evtData);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all dispatchers and subscriptions
        /// </summary>
        protected IPubSubDispatcher GetDispatcher(String originQueueName)
        {
           
            var subName = originQueueName.Substring(QueueName.Length + 1);

            using (AuthenticationContext.EnterSystemContext())
            {
                var subscription = this.m_pubSubManager.GetSubscriptionByName(subName);
                var channelDef = this.m_pubSubManager.GetChannel(subscription.ChannelKey);
                var factory = DispatcherFactoryUtil.FindDispatcherFactoryById(channelDef.DispatcherFactoryId);
                return factory.CreateDispatcher(subscription.ChannelKey, new Uri(channelDef.Endpoint), channelDef.Settings.ToDictionary(o => o.Name, o => o.Value));
            }
        }

        /// <summary>
        /// Dispose of the listeners
        /// </summary>
        public void Dispose()
        {
            if (this.m_repositoryListeners != null)
            {
                foreach (var itm in this.m_repositoryListeners)
                {
                    itm.Dispose();
                }
            }

            this.m_repositoryListeners = null;
            this.m_queueService.UnSubscribe(QueueName, this.NotificationQueued);
        }

        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);
            this.m_repositoryListeners = new List<IDisposable>();

            using (AuthenticationContext.EnterSystemContext())
            {
                try
                {
                    var bundleListener = this.m_serviceManager.CreateInjected<BundleRepositoryListener>();
                    this.m_repositoryListeners.Add(bundleListener);

                    // Hook up the listeners for existing
                    foreach (var psd in this.m_pubSubManager.FindSubscription(x => x.IsActive == true))
                    {
                        this.PubSubSubscribe(this, new Event.DataPersistedEventArgs<PubSubSubscriptionDefinition>(psd, TransactionMode.Commit, AuthenticationContext.SystemPrincipal));
                    }
                }
                catch (Exception ex)
                {
                    this.m_tracer.TraceWarning("Cannot wire up subscription broker - {0}", ex);
                }
            }

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
                var resourceXml = e.Data.ResourceType.GetSerializationName();
                if (!this.m_pubSubManager.FindSubscription(o => o.ResourceTypeName == resourceXml).Any())
                {
                    var lt = typeof(PubSubRepositoryListener<>).MakeGenericType(e.Data.ResourceType);
                    var listener = this.m_repositoryListeners.FirstOrDefault(o => lt.Equals(o.GetType()));
                    listener.Dispose();
                    this.m_repositoryListeners.Remove(listener);
                }
            }
        }

        /// <summary>
        /// Pub-sub definition has been subscribed
        /// </summary>
        private void PubSubSubscribe(object sender, Event.DataPersistedEventArgs<PubSubSubscriptionDefinition> e)
        {
            lock (this.m_lock)
            {
                var lt = typeof(PubSubRepositoryListener<>).MakeGenericType(e.Data.ResourceType);

                var queueName = $"{QueueName}.{e.Data.Name}";
                this.m_queueService.Open(queueName);
                this.m_queueService.SubscribeTo(queueName, this.NotificationQueued);
                this.m_queueService.Open($"{queueName}.dead");

                this.m_repositoryListeners.OfType<BundleRepositoryListener>().First().AddSubscription(e.Data);
                if (!this.m_repositoryListeners.Any(o => o.GetType().Equals(lt)))
                {
                    this.m_repositoryListeners.Add(this.m_serviceManager.CreateInjected(lt) as IDisposable);
                }
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