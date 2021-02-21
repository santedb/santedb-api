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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using SanteDB.Core.Model.Query;
using System.Collections.Concurrent;

namespace SanteDB.Core.PubSub.Broker
{
    /// <summary>
    /// Represents a class which listens to a repository and notifies the various subscriptions
    /// </summary>
    public class PubSubRepositoryListener<TModel> : IDisposable where TModel : IdentifiedData
    {

        // Cached filter criteria
        private Dictionary<Guid, Func<TModel, bool>> m_filterCriteria = new Dictionary<Guid, Func<TModel, bool>>();

        // The repository this listener listens to
        private INotifyRepositoryService<TModel> m_repository;

        // Merge service
        private IRecordMergingService<TModel> m_mergeService;

        // Manager
        private IPubSubManagerService m_pubSubManager;

        /// <summary>
        /// Constructs a new repository listener
        /// </summary>
        public PubSubRepositoryListener()
        {
            this.m_pubSubManager = ApplicationServiceContext.Current.GetService<IPubSubManagerService>();
            this.m_repository = ApplicationServiceContext.Current.GetService<INotifyRepositoryService<TModel>>();
            if (this.m_repository == null)
                throw new InvalidOperationException($"Cannot subscribe to {typeof(TModel).FullName} as this repository does not raise events");

            this.m_repository.Inserted += OnInserted;
            this.m_repository.Saved += OnSaved;
            this.m_repository.Obsoleted += OnObsoleted;

            this.m_mergeService = ApplicationServiceContext.Current.GetService<IRecordMergingService<TModel>>();
            if(this.m_mergeService != null)
            {
                this.m_mergeService.Merged += OnMerged;
                this.m_mergeService.UnMerged += OnUnmerged;
            }
        }

        /// <summary>
        /// Get all dispatchers and subscriptions
        /// </summary>
        private IEnumerable<IPubSubDispatcher> GetDispatchers(PubSubEventType eventType, TModel data)
        {
            var resourceName = data.GetType().GetSerializationName();
            var subscriptions = this.m_pubSubManager
                    .FindSubscription(o => o.ResourceTypeXml == resourceName && o.IsActive && (o.NotBefore == null || o.NotBefore < DateTimeOffset.Now) && (o.NotAfter == null || o.NotAfter > DateTimeOffset.Now))
                    .Where(o => o.Event.HasFlag(eventType))
                    .Where(s =>
                    {
                        if (!this.m_filterCriteria.TryGetValue(s.Key.Value, out Func<TModel, bool> fn))
                        {
                            Expression dynFn = null;
                            var parameter = Expression.Parameter(typeof(TModel));

                            foreach(var itm in s.Filter)
                            {
                                var fFn = QueryExpressionParser.BuildLinqExpression<TModel>(itm);
                                if (dynFn is LambdaExpression le)
                                    dynFn = Expression.Lambda(
                                        Expression.And(
                                            Expression.Invoke(le, parameter),
                                            Expression.Invoke(fFn, parameter)
                                           ), parameter);
                                else
                                    dynFn = fFn;

                            }
                            this.m_filterCriteria.Add(s.Key.Value, fn);
                        }
                        return fn(data);
                    });

            // Now we want to filter by channel, since the channel is really what we're interested in
            foreach(var chnl in subscriptions.GroupBy(o=>o.ChannelKey))
            {
                var channelDef = this.m_pubSubManager.GetChannel(chnl.Key);
                var factory = Activator.CreateInstance(channelDef.DispatcherFactoryType) as IPubSubDispatcherFactory;
                yield return factory.CreateDispatcher(chnl.Key, channelDef.Endpoint, channelDef.Settings.ToDictionary(o => o.Name, o => o.Value));
            }
        }

        /// <summary>
        /// When unmerged
        /// </summary>
        private void OnUnmerged(object sender, Event.DataMergeEventArgs<TModel> e)
        {
            foreach (var dsptchr in this.GetDispatchers(PubSubEventType.UnMerge, this.m_repository.Get(e.MasterKey)))
                dsptchr.NotifyUnMerged(this.m_repository.Get(e.MasterKey), e.LinkedKeys.Select(o => this.m_repository.Get(o)).ToArray());
        }

        /// <summary>
        /// When merged
        /// </summary>
        private void OnMerged(object sender, Event.DataMergeEventArgs<TModel> e)
        {
            foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Merge, this.m_repository.Get(e.MasterKey)))
                dsptchr.NotifyMerged(this.m_repository.Get(e.MasterKey), e.LinkedKeys.Select(o => this.m_repository.Get(o)).ToArray());
        }

        /// <summary>
        /// When obsoleted
        /// </summary>
        private void OnObsoleted(object sender, Event.DataPersistedEventArgs<TModel> e)
        {
            foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Delete, e.Data))
                dsptchr.NotifyObsoleted(e.Data);
        }

        /// <summary>
        /// When saved (updated)
        /// </summary>
        private void OnSaved(object sender, Event.DataPersistedEventArgs<TModel> e)
        {
            foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Update, e.Data))
                dsptchr.NotifyUpdated(e.Data);
        }

        /// <summary>
        /// When inserted
        /// </summary>
        private void OnInserted(object sender, Event.DataPersistedEventArgs<TModel> e)
        {
            foreach (var dsptchr in this.GetDispatchers(PubSubEventType.Create, e.Data))
                dsptchr.NotifyCreated(e.Data);
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
            if(this.m_mergeService != null)
            {
                this.m_mergeService.Merged -= this.OnMerged;
                this.m_mergeService.UnMerged -= this.OnUnmerged;
            }
        }
    }
}
