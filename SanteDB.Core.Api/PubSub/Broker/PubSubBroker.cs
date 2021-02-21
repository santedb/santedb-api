/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.PubSub.Broker
{
    /// <summary>
    /// A pub-sub broker which is responsible for actually facilitating the publish/subscribe
    /// logic
    /// </summary>
    public class PubSubBroker : IDaemonService, IDisposable
    {

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

        /// <summary>
        /// Dispose of the listeners
        /// </summary>
        public void Dispose()
        {
            if(this.m_repositoryListeners != null)
            foreach (var itm in this.m_repositoryListeners)
                itm.Dispose();
            this.m_repositoryListeners = null;
        }

        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);
            // Create necessary service listener

            this.m_pubSubManager = ApplicationServiceContext.Current.GetService<IPubSubManagerService>();
            if (this.m_pubSubManager == null)
                throw new InvalidOperationException("Must have at least one IPubSubManagerService configured");
            this.m_pubSubManager.Subscribed += this.PubSubSubscribed;
            this.m_pubSubManager.UnSubscribed += this.PubSubUnSubscribed;

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
                    this.m_repositoryListeners.Add(Activator.CreateInstance(lt) as IDisposable);
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
