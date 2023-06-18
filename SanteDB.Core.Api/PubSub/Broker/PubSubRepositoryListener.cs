/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Data;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Parameters;
using SanteDB.Core.Queue;
using SanteDB.Core.Services;
using System;
using System.Linq;
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
            this.NotificationDate = DateTimeOffset.Now;
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
        public string NotificationDateXml
        {
            get => this.NotificationDate.ToString("o");
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    this.NotificationDate = DateTimeOffset.MinValue;
                }
                else if (DateTimeOffset.TryParse(value, out var result))
                {
                    this.NotificationDate = result;
                }
                else
                {
                    throw new FormatException($"Cannot parse {value} as datetime");
                }
            }
        }

        /// <summary>
        /// Gets or sets the notification date
        /// </summary>
        [XmlIgnore]
        public DateTimeOffset NotificationDate { get; set; }

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
        private readonly IDataManagedLinkProvider<TModel> m_managedLinkService;

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
        public PubSubRepositoryListener(IPubSubManagerService pubSubManager, IDispatcherQueueManagerService queueService, IServiceManager serviceManager, INotifyRepositoryService<TModel> repositoryService, IDataManagedLinkProvider<TModel> managedLinkProvider = null)
        {
            this.m_pubSubManager = pubSubManager;
            this.m_repository = repositoryService;
            this.m_managedLinkService = managedLinkProvider;
            this.m_queueService = queueService;
            this.m_serviceManager = serviceManager;

            if (this.m_repository == null)
            {
                throw new InvalidOperationException($"Cannot actively subscribe to {typeof(TModel).FullName} as this repository does not raise events");
            }

            this.m_repository.Inserted += OnInserted;
            this.m_repository.Saved += OnSaved;
            this.m_repository.Deleted += OnDeleted;

            this.m_mergeService = ApplicationServiceContext.Current.GetService<IRecordMergingService<TModel>>();
            if (this.m_mergeService != null)
            {
                this.m_mergeService.Merged += OnMerged;
                this.m_mergeService.UnMerged += OnUnmerged;
            }

            if (this.m_managedLinkService != null)
            {
                this.m_managedLinkService.ManagedLinkEstablished += OnLinked;
                this.m_managedLinkService.ManagedLinkRemoved += OnUnLinked;
            }
        }

        /// <summary>
        /// When unmerged
        /// </summary>
        protected virtual void OnUnmerged(object sender, Event.DataMergeEventArgs<TModel> evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.UnMerge, new ParameterCollection(new Parameter("survivor", this.m_repository.Get(evt.SurvivorKey)), new Parameter("linkedDuplicates", new Bundle(evt.LinkedKeys.Select(o => this.m_repository.Get(o)).ToList())))));
        }

        /// <summary>
        /// When merged
        /// </summary>
        protected virtual void OnMerged(object sender, Event.DataMergeEventArgs<TModel> evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Merge, new ParameterCollection(new Parameter("survivor", this.m_repository.Get(evt.SurvivorKey)), new Parameter("linkedDuplicates", new Bundle(evt.LinkedKeys.Select(o => this.m_repository.Get(o)).ToList())))));
        }

        /// <summary>
        /// When obsoleted
        /// </summary>
        protected virtual void OnDeleted(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.EnqueueObject(evt.Data, PubSubEventType.Delete);
        }

        /// <summary>
        /// When saved (updated)
        /// </summary>
        protected virtual void OnSaved(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.EnqueueObject(evt.Data, PubSubEventType.Update);
        }

        /// <summary>
        /// When inserted
        /// </summary>
        protected virtual void OnInserted(object sender, Event.DataPersistedEventArgs<TModel> evt)
        {
            this.EnqueueObject(evt.Data, PubSubEventType.Create);
        }

        /// <summary>
        /// When link occurs
        /// </summary>
        protected virtual void OnLinked(object sender, Data.DataManagementLinkEventArgs evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Link, new ParameterCollection(new Parameter("holder", evt.TargetedAssociation.LoadProperty(o => o.SourceEntity)), new Parameter("target", evt.TargetedAssociation.LoadProperty(o => o.TargetEntity)))));

        }

        /// <summary>
        /// When link occurs
        /// </summary>
        protected virtual void OnUnLinked(object sender, Data.DataManagementLinkEventArgs evt)
        {
            this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Link, new ParameterCollection(new Parameter("holder", evt.TargetedAssociation.LoadProperty(o => o.SourceEntity)), new Parameter("target", evt.TargetedAssociation.LoadProperty(o => o.TargetEntity)))));

        }

        /// <summary>
        /// Enqueue object
        /// </summary>
        private void EnqueueObject(IdentifiedData dataToQueue, PubSubEventType defaultEventType)
        {
            switch (dataToQueue.BatchOperation)
            {
                case Model.DataTypes.BatchOperationType.Update:
                    this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Update, dataToQueue));
                    break;
                case Model.DataTypes.BatchOperationType.Delete:
                    this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Delete, dataToQueue));
                    break;
                case Model.DataTypes.BatchOperationType.Insert:
                    this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), PubSubEventType.Create, dataToQueue));
                    break;
                case Model.DataTypes.BatchOperationType.InsertOrUpdate:
                case Model.DataTypes.BatchOperationType.Auto:
                    this.m_queueService.Enqueue(PubSubBroker.QueueName, new PubSubNotifyQueueEntry(typeof(TModel), defaultEventType, dataToQueue));
                    break;
            }
        }
        /// <summary>
        /// Dispose of this
        /// </summary>
        public void Dispose()
        {
            if (this.m_repository != null)
            {
                this.m_repository.Inserted -= this.OnInserted;
                this.m_repository.Deleted -= this.OnDeleted;
                this.m_repository.Saved -= this.OnSaved;
                this.m_repository = null;
            }
            if (this.m_mergeService != null)
            {
                this.m_mergeService.Merged -= this.OnMerged;
                this.m_mergeService.UnMerged -= this.OnUnmerged;
            }
            if (this.m_managedLinkService != null)
            {
                this.m_managedLinkService.ManagedLinkEstablished -= this.OnLinked;
                this.m_managedLinkService.ManagedLinkRemoved -= this.OnUnLinked;
            }
        }
    }
}