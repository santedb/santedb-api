﻿/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2020-2-28
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Data
{
    /// <summary>
    /// Represents a daemon service that registers a series of merge services which can merge records together
    /// </summary>
    public class SimDataManagementService : IDaemonService
    {

        /// <summary>
        /// Single Instance Mode Handler
        /// </summary>
        /// <remarks>This class binds to startup and enables the listening and merging of records based on the record matcher</remarks>
        private class SimResourceMerger<TModel> : IRecordMergingService<TModel>
            where TModel : VersionedEntityData<TModel>, new()
        {

            // Tracer
            private Tracer m_tracer = Tracer.GetTracer(typeof(SimDataManagementService));

            // The configuration
            private ResourceMergeConfiguration m_configuration;

            // Service for matching
            private IRecordMatchingService m_matchingService;

            /// <summary>
            /// Fired when data is about to be merged
            /// </summary>
            public event EventHandler<DataMergingEventArgs<TModel>> Merging;

            /// <summary>
            /// Fired after data has been merged
            /// </summary>
            public event EventHandler<DataMergeEventArgs<TModel>> Merged;

            public string ServiceName => throw new NotImplementedException();

            /// <summary>
            /// Creates a new resource merger with specified configuration
            /// </summary>
            public SimResourceMerger(ResourceMergeConfiguration configuration)
            {
                this.m_configuration = configuration;

                // Find the specified matching configuration
                this.m_matchingService = ApplicationServiceContext.Current.GetService<IRecordMatchingService>();

                ApplicationServiceContext.Current.GetService<IDataPersistenceService<TModel>>().Inserting += DataInsertingHandler;
                ApplicationServiceContext.Current.GetService<IDataPersistenceService<TModel>>().Updating += DataUpdatingHandler;
            }

            /// <summary>
            /// Data updating handler
            /// </summary>
            /// <remarks>This method will detect whether a duplicate exists and will mark it as such</remarks>
            private void DataUpdatingHandler(object sender, Event.DataPersistingEventArgs<TModel> e)
            {
                // Detect any duplicates
                var matches = this.m_matchingService.Match<TModel>(e.Data, this.m_configuration.MatchConfiguration);

                // Clear out current duplicate markers
                this.MarkDuplicates(e.Data, matches.Where(o => o.Classification != RecordMatchClassification.NonMatch && o.Record.Key != e.Data.Key));
            }

            /// <summary>
            /// Data inserting handler
            /// </summary>
            /// <remarks>This method will detect whether a duplicate exists and will either merge it or simply mark it as duplicate</remarks>
            private void DataInsertingHandler(object sender, Event.DataPersistingEventArgs<TModel> e)
            {
                // Detect any duplicates
                var matches = this.m_matchingService.Match<TModel>(e.Data, this.m_configuration.MatchConfiguration);

                // 1. Exactly one match is found and AutoMerge so we merge
                if (this.m_configuration.AutoMerge && matches.Count(o => o.Classification != RecordMatchClassification.Match && o.Record.Key != e.Data.Key) == 1)
                {
                    var match = matches.SingleOrDefault();
                    if (this.m_configuration.PreserveOriginal)
                    {
                        this.m_tracer.TraceInfo("{0} matches with {1} ({2}) and AutoMerge is true --- NEW RECORD WILL BE SET TO NULLIFIED ---", e.Data, match.Record, match.Score);
                        var mergeResult = this.Merge(match.Record, new List<TModel>() { e.Data }); // Merge the data
                        if (mergeResult != null)
                            (e.Data as IHasState).StatusConceptKey = StatusKeys.Nullified;
                    }
                    else
                    {
                        this.m_tracer.TraceWarning("{0} matches with {1} ({2}) and AutoMerge is true --- NEW RECORD WILL NOT BE PERSISTED ---", e.Data, match.Record, match.Score);
                        var mergeResult = this.Merge(match.Record, new List<TModel>() { e.Data }); // Merge the data
                        if (mergeResult != null)
                        {
                            e.Cancel = true;
                            e.Data = mergeResult;
                        }
                    }
                }

                this.MarkDuplicates(e.Data, matches.Where(o => o.Classification != RecordMatchClassification.NonMatch && o.Record.Key != e.Data.Key));
            }

            /// <summary>
            /// Mark duplicate record entries
            /// </summary>
            private void MarkDuplicates(TModel duplicate, IEnumerable<IRecordMatchResult<TModel>> matches)
            {
                // Remove all duplicates
                if (duplicate is Act)
                    (duplicate as Act).Relationships.RemoveAll(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate);
                else if (duplicate is Entity)
                    (duplicate as Entity).Relationships.RemoveAll(o => o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate);

                // Now process any matches
                foreach (var match in matches)
                {
                    this.m_tracer.TraceWarning("{0} matches with {1} ({2} / {3}) - Marking as duplicate", duplicate, match.Record, match.Score, match.Classification);

                    // Mark duplicates
                    if (duplicate is Entity)
                        (duplicate as Entity).Relationships.Add(new EntityRelationship(EntityRelationshipTypeKeys.Duplicate, match.Record.Key) { Quantity = (int)match.Score * 100 });
                    else if (duplicate is Act)
                        (duplicate as Act).Relationships.Add(new ActRelationship(ActRelationshipTypeKeys.Duplicate, match.Record.Key));
                    else
                        throw new InvalidOperationException($"Can't determine how to mark type of {duplicate.GetType().Name} as duplicate");
                }
            }

            /// <summary>
            /// Merges the specified duplicates into the master
            /// </summary>
            public virtual TModel Merge(TModel master, IEnumerable<TModel> linkedDuplicates)
            {
                var mergeEventArgs = new DataMergingEventArgs<TModel>(master, linkedDuplicates);
                this.Merging?.Invoke(this, mergeEventArgs);
                if (mergeEventArgs.Cancel)
                {
                    this.m_tracer.TraceInfo("Pre-Event trigger indicated cancel merge");
                    return null;
                }

                // The invoke may have changed the master
                master = mergeEventArgs.Master;

                // We'll update the parameters from the candidate to create a single master record
                // TODO: Verify this in edge cases
                Guid? masterKey = master.Key, masterVersionKey = master.VersionKey;
                Bundle persistenceBundle = new Bundle();

                foreach (var l in linkedDuplicates)
                {
                    master.CopyObjectData(l, false); // Copy data which is different

                    // Add replaces and nullify
                    if (l.Key.HasValue)
                    {
                        if (master is Act)
                            (master as Act).Relationships.Add(new ActRelationship(ActRelationshipTypeKeys.Replaces, l.Key));
                        else if (l is Entity)
                            (master as Entity).Relationships.Add(new EntityRelationship(EntityRelationshipTypeKeys.Replaces, l.Key));
                        persistenceBundle.Add(l);
                    }
                    else // Not persisted yet
                    {
                        if (l is Act)
                            (l as Act).Relationships.Add(new ActRelationship(ActRelationshipTypeKeys.Replaces, masterKey) { TargetActKey = l.Key });
                        else if (l is Entity)
                            (l as Entity).Relationships.Add(new EntityRelationship(EntityRelationshipTypeKeys.Replaces, l.Key) { TargetEntityKey = l.Key });
                    }
                    (l as IHasState).StatusConceptKey = StatusKeys.Nullified;

                }
                master.Key = masterKey;
                master.VersionKey = masterVersionKey;
                persistenceBundle.Add(master);

                // Persist
                ApplicationServiceContext.Current.GetService<IDataPersistenceService<Bundle>>().Update(persistenceBundle, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                this.Merged?.Invoke(this, new DataMergeEventArgs<TModel>(master, linkedDuplicates));
                return master;
            }

            /// <summary>
            /// Unmerge - Not supported by SIM
            /// </summary>
            public virtual TModel Unmerge(TModel master, TModel unmergeDuplicate)
            {
                throw new NotSupportedException("Single Instance Mode cannot un-merge data");
            }
        }

        // Tracer for SIM
        private Tracer m_tracer = Tracer.GetTracer(typeof(SimDataManagementService));

        // Configuration section 
        private ResourceMergeConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<ResourceMergeConfigurationSection>();

        // Merge services
        private List<IDisposable> m_mergeServices = new List<IDisposable>();

        /// <summary>
        /// True if is running
        /// </summary>
        public bool IsRunning => false;

        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "Single Instance Merge Service";

        /// <summary>
        /// Service is starting
        /// </summary>
        public event EventHandler Starting;
        /// <summary>
        /// Service has started
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Service is stopping
        /// </summary>
        public event EventHandler Stopping;
        /// <summary>
        /// Service has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Start the service
        /// </summary>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);

            if (ApplicationServiceContext.Current.GetService<IRecordMatchingService>() == null)
                throw new InvalidOperationException("This service requires a record matching service to be registered");

            // Register mergers for all types in configuration
            foreach (var i in this.m_configuration.ResourceTypes)
            {
                this.m_tracer.TraceInfo("Creating record management service for {0}", i.ResourceType.Name);
                var idt = typeof(SimResourceMerger<>).MakeGenericType(i.ResourceType);
                this.m_mergeServices.Add(Activator.CreateInstance(idt, i) as IDisposable);
            }

            this.Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stop the service
        /// </summary>
        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            foreach (var s in this.m_mergeServices)
            {
                ApplicationServiceContext.Current.GetService<IServiceManager>().RemoveServiceProvider(s.GetType());
                s.Dispose();
            }
            this.m_mergeServices.Clear();

            this.Stopped?.Invoke(this, EventArgs.Empty);

            return true;
        }
    }
}