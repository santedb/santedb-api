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
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Parameters;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Queue;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Core.PubSub.Broker
{
    /// <summary>
    /// Notification metadata which is placed in the persistence queue
    /// </summary>
    [XmlType(nameof(PubSubNotifyQueueEntry), Namespace = "http://santedb.org/pubsub")]
    [AddDependentSerializersAttribute]
    public class PubSubNotifyQueueEntry
    {
        /// <summary>
        /// Pub-sub notify
        /// </summary>
        public PubSubNotifyQueueEntry()
        {
        }

        /// <summary>
        /// Create new queue entry
        /// </summary>
        public PubSubNotifyQueueEntry(Type tmodelType, PubSubEventType eventType, object data)
        {
            this.TargetType = tmodelType;
            this.EventType = eventType;
            this.Data = data;
            this.NotificationDate = DateTime.Now;
        }

        /// <summary>
        /// Target type
        /// </summary>
        [XmlIgnore]
        public Type TargetType { get; set; }

        /// <summary>
        /// Gets or sets the target type XML
        /// </summary>
        [XmlAttribute("target")]
        public String TargetTypeXml
        {
            get => this.TargetType?.AssemblyQualifiedName;
            set => this.TargetType = value == null ? null : Type.GetType(value);
        }

        /// <summary>
        /// The event type
        /// </summary>
        [XmlAttribute("event")]
        public PubSubEventType EventType { get; set; }

        /// <summary>
        /// Notification date (note: we use datetime instead of datetimeoffset because XML serialization issues)
        /// </summary>
        [XmlAttribute("date")]
        public DateTime NotificationDate { get; set; }

        /// <summary>
        /// Gets or sets the payload of the data
        /// </summary>
        [XmlElement("data")]
        public Object Data { get; set; }
    }

    /// <summary>
    /// Represents a class which listens to a repository and notifies the various subscriptions
    /// </summary>
    public class PubSubRepositoryListener<TModel> : IDisposable where TModel : IdentifiedData
    {
        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(PubSubRepositoryListener<TModel>));

        // The repository this listener listens to
        private INotifyRepositoryService<TModel> m_repository;

        // Queue service
        private IDispatcherQueueManagerService m_queueService;

        // Service manager
        private IServiceManager m_serviceManager;

        // Merge service
        private IRecordMergingService<TModel> m_mergeService;

        // Manager
        private IPubSubManagerService m_pubSubManager;

        /// <summary>
        /// Constructs a new repository listener
        /// </summary>
        public PubSubRepositoryListener(IPubSubManagerService pubSubManager, IDispatcherQueueManagerService queueService, IServiceManager serviceManager)
        {
            this.m_pubSubManager = pubSubManager;
            this.m_repository = ApplicationServiceContext.Current.GetService<INotifyRepositoryService<TModel>>();
            this.m_queueService = queueService;
            this.m_serviceManager = serviceManager;

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
        /// When unmerged
        /// </summary>
        protected virtual void OnUnmerged(object sender, Event.DataMergeEventArgs<TModel> evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.UnMerge, new ParameterCollection(new Parameter("survivor", this.m_repository.Get(evt.SurvivorKey)), new Parameter("linkedDuplicates", evt.LinkedKeys.Select(o => this.m_repository.Get(o)).ToList()))));
        }

        /// <summary>
        /// When merged
        /// </summary>
        protected virtual void OnMerged(object sender, Event.DataMergeEventArgs<TModel> evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Merge, new ParameterCollection(new Parameter("survivor", this.m_repository.Get(evt.SurvivorKey)), new Parameter("linkedDuplicates", evt.LinkedKeys.Select(o => this.m_repository.Get(o)).ToList()))));
        }

        /// <summary>
        /// When obsoleted
        /// </summary>
        protected virtual void OnObsoleted(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Delete, evt.Data));
        }

        /// <summary>
        /// When saved (updated)
        /// </summary>
        protected virtual void OnSaved(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Update, evt.Data));
        }

        /// <summary>
        /// When inserted
        /// </summary>
        protected virtual void OnInserted(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Create, evt.Data));
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