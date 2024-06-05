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
 * Date: 2024-2-18
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.i18n;
using SanteDB.Core.Matching;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SanteDB.Core.Data.Management
{
    /// <summary>
    /// Single Instance Mode Handler
    /// </summary>
    /// <remarks>This class binds to startup and enables the listening and merging of records based on the record matcher</remarks>
    internal class SimResourceInterceptor<TModel> : IRecordMergingService<TModel>, ISimResourceInterceptor, IReportProgressChanged
        where TModel : BaseEntityData, IHasState, IHasClassConcept, IVersionedData, new()
    {
        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(SimDataManagementService));

        // The configuration
        private readonly IRecordMatchingConfigurationService m_matchingConfigurationService;
        private readonly IPolicyEnforcementService m_pepService;
        private readonly IRecordMatchingService m_matchingService;
        private readonly IDataPersistenceServiceEx<EntityRelationship> m_entityRelationshipService;
        private readonly IDataPersistenceServiceEx<ActRelationship> m_actRelationshipService;
        private readonly IDataPersistenceService<Bundle> m_bundlePersistenceService;
        private readonly IDataPersistenceService<TModel> m_persistenceService;
        private readonly IBusinessRulesService<TModel> m_businessRulesService;

        /// <inheritdoc/>
        public event EventHandler<DataMergingEventArgs<TModel>> Merging;

        /// <inheritdoc/>
        public event EventHandler<DataMergeEventArgs<TModel>> Merged;
#pragma warning disable CS0067

        /// <inheritdoc/>
        public event EventHandler<DataMergingEventArgs<TModel>> UnMerging;
        /// <inheritdoc/>
        public event EventHandler<DataMergeEventArgs<TModel>> UnMerged;
        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore

        public string ServiceName => "Single Instance Method Data Management Service";

        /// <summary>
        /// Creates a new resource merger with specified configuration
        /// </summary>
        public SimResourceInterceptor(
            IDataPersistenceService<Bundle> bundlePersistence,
            IDataPersistenceService<TModel> persistenceService,
            INotifyRepositoryService<TModel> repositoryService,
            IRecordMatchingService matchingService,
            IPolicyEnforcementService pepService,
            IRecordMatchingConfigurationService configurationService,
            IDataPersistenceServiceEx<EntityRelationship> entityRelationshipService,
            IDataPersistenceServiceEx<ActRelationship> actRelationshipService,
            IBusinessRulesService<TModel> businessRulesService = null)
        {
            // Find the specified matching configuration
            this.m_pepService = pepService;
            this.m_matchingService = matchingService;
            this.m_matchingConfigurationService = configurationService;
            this.m_entityRelationshipService = entityRelationshipService;
            this.m_actRelationshipService = actRelationshipService;
            this.m_bundlePersistenceService = bundlePersistence;
            this.m_persistenceService = persistenceService;
            this.m_businessRulesService = businessRulesService;
            repositoryService.Inserting += OnInserting;
            repositoryService.Saving += OnSaving;
            repositoryService.Deleted += OnDeleted;

        }

        private void OnInserting(object sender, DataPersistingEventArgs<TModel> e)
        {
            e.Data.BatchOperation = Model.DataTypes.BatchOperationType.Insert;
            this.DoSimDataManagementInternal(e);
        }

        private void OnSaving(object sender, DataPersistingEventArgs<TModel> e)
        {
            e.Data.BatchOperation = Model.DataTypes.BatchOperationType.Update;
            this.DoSimDataManagementInternal(e);
        }

        /// <summary>
        /// Perform the SIM data management event handler 
        /// </summary>
        private void DoSimDataManagementInternal(DataPersistingEventArgs<TModel> e)
        {
            var transactionBundle = new Bundle();
            e.Data.Key = e.Data.Key ?? Guid.NewGuid();
            transactionBundle.AddRange(this.DoDataMatchingLogicInternal(e.Data));
            if (transactionBundle.Count == 0)
            {
                return; // no need to interrupt
            }

            e.Cancel = true;

            // Manually run the BRE
            if (e.Data.BatchOperation == Model.DataTypes.BatchOperationType.Insert)
            {
                e.Data = this.m_businessRulesService?.BeforeInsert(e.Data) ?? e.Data;
            }
            else
            {
                e.Data = this.m_businessRulesService?.BeforeUpdate(e.Data) ?? e.Data;
            }

            transactionBundle = this.m_bundlePersistenceService.Insert(transactionBundle, e.Mode, AuthenticationContext.Current.Principal);
            e.Data = transactionBundle.Item.Find(o => o.Key == e.Data.Key) as TModel;

            if (e.Data.BatchOperation == Model.DataTypes.BatchOperationType.Insert)
            {
                e.Data = this.m_businessRulesService?.AfterInsert(e.Data) ?? e.Data;
            }
            else
            {
                e.Data = this.m_businessRulesService?.AfterUpdate(e.Data) ?? e.Data;
            }
        }

        /// <summary>
        /// Data deleted event handler - clear out all duplicates which point at me 
        /// </summary>
        private void OnDeleted(object sender, DataPersistedEventArgs<TModel> e)
        {
            var deleteBundle = new Bundle(this.DoDataDeletionLogicInternal(e.Data));
            // Persist
            if (deleteBundle.Item.Any())
            {
                this.m_bundlePersistenceService.Insert(deleteBundle, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
            }
        }

        private IEnumerable<IdentifiedData> DoDataDeletionLogicInternal(TModel data)
        {
            if (data is Act)
            {
                foreach (var ar in this.m_actRelationshipService.Query(o => o.TargetActKey == data.Key && o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate, AuthenticationContext.SystemPrincipal))
                {
                    ar.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
                    yield return ar;
                }
            }
            else if (data is Entity)
            {
                foreach (var er in this.m_entityRelationshipService.Query(o => o.TargetEntityKey == data.Key && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate, AuthenticationContext.SystemPrincipal))
                {
                    er.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
                    yield return er;
                }
            }

        }

        /// <summary>
        /// Perform matching logic
        /// </summary>
        private IEnumerable<IdentifiedData> DoDataMatchingLogicInternal(TModel inputRecord)
        {
            // Detect any duplicates
            var matches = m_matchingConfigurationService.Configurations.Where(o => o.AppliesTo.Contains(typeof(TModel)) && o.Metadata.Status == MatchConfigurationStatus.Active).SelectMany(o => m_matchingService.Match(inputRecord, o.Id, this.GetIgnoredKeys(inputRecord.Key.GetValueOrDefault())));
            var groupedMatches = matches
                .Where(o => o.Record.Key != inputRecord.Key && o.Classification != RecordMatchClassification.NonMatch)
                .GroupBy(o => o.Classification)
                .ToDictionary(o => o.Key, o => o.ToArray());

            // Fill out the duplicate dictionary
            if (!groupedMatches.ContainsKey(RecordMatchClassification.Match))
            {
                groupedMatches.Add(RecordMatchClassification.Match, new IRecordMatchResult<TModel>[0]);
            }
            if (!groupedMatches.ContainsKey(RecordMatchClassification.Probable))
            {
                groupedMatches.Add(RecordMatchClassification.Probable, new IRecordMatchResult<TModel>[0]);
            }

            // 1. Exactly one match is found and AutoMerge so we merge
            if (groupedMatches[RecordMatchClassification.Match].Count() == 1 &&
                !groupedMatches[RecordMatchClassification.Probable].Any() &&
                groupedMatches[RecordMatchClassification.Match].Single().Configuration.Metadata.Tags.TryGetValue(SanteDBConstants.AutoMatchTagName, out var autoMatchString)
                && Boolean.TryParse(autoMatchString, out var autoMatchBool)
                && autoMatchBool)
            {
                var match = groupedMatches[RecordMatchClassification.Match].Single();
                this.m_tracer.TraceInfo("{0} matches with {1} ({2}) and AutoMerge is true --- NEW RECORD WILL BE SET TO NULLIFIED ---", inputRecord.Key, match.Record, match.Score);
                foreach (var itm in this.DoMergeLogicInternal(inputRecord, match.Record))
                {
                    yield return itm;
                }
            }
            else
            {
                foreach (var itm in this.DoMarkDuplicateLogicInternal(inputRecord, matches))
                {
                    yield return itm;
                }
            }

        }

        /// <summary>
        /// Mark duplicate record entries
        /// </summary>
        private IEnumerable<IdentifiedData> DoMarkDuplicateLogicInternal(TModel newRecord, IEnumerable<IRecordMatchResult<TModel>> matches)
        {
            // Load the duplicate relationships for the source for removal 
            switch (newRecord)
            {
                case Act act:
                    foreach (var itm in this.m_actRelationshipService.Query(o => o.SourceEntityKey == newRecord.Key && o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate, AuthenticationContext.SystemPrincipal))
                    {
                        itm.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
                        yield return itm;
                    }
                    // Add detected duplicates with each object
                    foreach (var match in matches)
                    {
                        yield return new ActRelationship(ActRelationshipTypeKeys.Duplicate, match.Record.Key)
                        {
                            SourceEntityKey = newRecord.Key,
                            NegationIndicator = false,
                            BatchOperation = Model.DataTypes.BatchOperationType.Insert
                        };
                    }
                    break;
                case Entity ent:
                    foreach (var itm in this.m_entityRelationshipService.Query(o => o.SourceEntityKey == newRecord.Key && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate, AuthenticationContext.SystemPrincipal))
                    {
                        itm.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
                        yield return itm;
                    }

                    foreach (var match in matches)
                    {
                        yield return new EntityRelationship(EntityRelationshipTypeKeys.Duplicate, match.Record.Key)
                        {
                            SourceEntityKey = newRecord.Key,
                            Strength = match.Strength,
                            NegationIndicator = false,
                            BatchOperation = Model.DataTypes.BatchOperationType.Insert
                        };
                    }
                    break;
            }
        }

        private IEnumerable<IdentifiedData> DoMergeLogicInternal(IdentifiedData subsumedRecord, IdentifiedData survivorRecord)
        {
            if (subsumedRecord is IHasState sts)
            {
                sts.StatusConceptKey = StatusKeys.Obsolete;
                subsumedRecord.BatchOperation = Model.DataTypes.BatchOperationType.Update;
                yield return subsumedRecord;
            }

            if(survivorRecord is IHasIdentifiers survivorId && subsumedRecord is IHasIdentifiers subsumedId)
            {
                foreach(var itm in subsumedId.LoadCollection(o=>o.Identifiers))
                {
                    if(!survivorId.Identifiers.Any(i=>i.IdentityDomainKey == itm.IdentityDomainKey && i.Value == itm.Value && i.CheckDigit == itm.CheckDigit))
                    {
                        var newId = survivorId.AddIdentifier(itm.IdentityDomainKey.Value, itm.Value);
                        newId.Reliability = itm.Reliability;
                        newId.CheckDigit = itm.CheckDigit;
                        newId.ExpiryDate = itm.ExpiryDate;
                        newId.IssueDate = itm.IssueDate;
                        newId.IdentifierTypeKey = itm.IdentifierTypeKey;
                    }
                }
            }
            switch (survivorRecord)
            {
                case Act survivorAct:
                    yield return new ActRelationship(ActRelationshipTypeKeys.Replaces, subsumedRecord.Key) { SourceEntityKey = survivorRecord.Key };
                    // Delete the relationships between 
                    foreach (var ar in this.m_actRelationshipService.Query(o => (o.SourceEntityKey == subsumedRecord.Key || o.TargetActKey == subsumedRecord.Key) && o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate, AuthenticationContext.SystemPrincipal))
                    {
                        ar.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
                        yield return ar;
                    }
                    break;
                case Entity survivorEntity:
                    yield return new EntityRelationship(EntityRelationshipTypeKeys.Replaces, subsumedRecord.Key) { SourceEntityKey = survivorRecord.Key };
                    // Delte the old relationships
                    foreach (var er in this.m_entityRelationshipService.Query(o => (o.SourceEntityKey == subsumedRecord.Key || o.TargetEntityKey == subsumedRecord.Key) && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate, AuthenticationContext.SystemPrincipal))
                    {
                        er.BatchOperation = Model.DataTypes.BatchOperationType.Delete;
                        yield return er;
                    }
                    break;
            }
        }

        /// <summary>
        /// Merges the specified duplicates into the master
        /// </summary>
        public virtual RecordMergeResult Merge(Guid survivorKey, IEnumerable<Guid> linkedDuplicates)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.UnrestrictedClinicalData);

            var mergeEventArgs = new DataMergingEventArgs<TModel>(survivorKey, linkedDuplicates);
            this.Merging?.Invoke(this, mergeEventArgs);
            if (mergeEventArgs.Cancel)
            {
                this.m_tracer.TraceInfo("Pre-Event trigger indicated cancel merge");
                return new RecordMergeResult(RecordMergeStatus.Aborted, null, null);
            }

            // The invoke may have changed the master
            survivorKey = mergeEventArgs.SurvivorKey;
            var survivor = this.m_persistenceService.Get(survivorKey, null, AuthenticationContext.Current.Principal);
            if (survivor == null)
            {
                throw new KeyNotFoundException($"{typeof(TModel).GetSerializationName()}/{survivorKey}");
            }

            Bundle persistenceBundle = new Bundle();
            foreach (var l in linkedDuplicates)
            {
                var local = this.m_persistenceService.Get(l, null, AuthenticationContext.Current.Principal);
                if (local == null)
                {
                    throw new KeyNotFoundException($"{typeof(TModel).GetSerializationName()}/{l}");
                }
                persistenceBundle.AddRange(this.DoMergeLogicInternal(local, survivor));
                (local as IHasState).StatusConceptKey = StatusKeys.Nullified;
                local.BatchOperation = Model.DataTypes.BatchOperationType.Update;
                persistenceBundle.Add(local);
            }
            survivor.Key = survivorKey;
            persistenceBundle.Insert(0, survivor);

            // Persist
            this.m_bundlePersistenceService.Update(persistenceBundle, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
            this.Merged?.Invoke(this, new DataMergeEventArgs<TModel>(survivorKey, linkedDuplicates));
            return new RecordMergeResult(RecordMergeStatus.Success, new Guid[] { survivorKey }, linkedDuplicates.ToArray());
        }

        /// <summary>
        /// Unmerge - Not supported by SIM
        /// </summary>
        public virtual RecordMergeResult Unmerge(Guid master, Guid unmergeDuplicate)
        {
            throw new NotSupportedException("Single Instance Mode cannot un-merge data");
        }

        /// <inheritdoc />
        public IdentifiedData Ignore(Guid masterKey, IEnumerable<Guid> falsePositives)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.UnrestrictedClinicalData);
            var master = this.m_persistenceService.Get(masterKey, null, AuthenticationContext.Current.Principal);
            if (master == null)
            {
                throw new KeyNotFoundException($"{typeof(TModel).GetSerializationName()}/{masterKey}");
            }

            master.BatchOperation = Model.DataTypes.BatchOperationType.InsertOrUpdate;
            // Remove all the existing links
            var persistenceBundle = new Bundle() { Item = new List<IdentifiedData>() { master } };
            // Remove all duplicate indicators
            if (typeof(Act).IsAssignableFrom(typeof(TModel)))
            {
                persistenceBundle.AddRange(this.m_actRelationshipService.Query(o => o.SourceEntityKey == masterKey && falsePositives.Contains(o.TargetActKey.Value) && o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate && o.NegationIndicator == true, AuthenticationContext.SystemPrincipal)
                    .Union(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate && falsePositives.Contains(o.SourceEntityKey.Value) && o.TargetActKey == masterKey && o.NegationIndicator == true)
                    .ToArray().Select(o => new ActRelationship()
                    {
                        Key = o.Key,
                        BatchOperation = Model.DataTypes.BatchOperationType.Delete
                    }));
                persistenceBundle.AddRange(falsePositives.Select(o => new ActRelationship()
                {
                    SourceEntityKey = o,
                    TargetActKey = masterKey,
                    RelationshipTypeKey = ActRelationshipTypeKeys.Duplicate,
                    NegationIndicator = true
                }));
            }
            else
            {
                // Remove all duplicate indicators
                persistenceBundle.AddRange(this.m_entityRelationshipService.Query(o => o.SourceEntityKey == masterKey && falsePositives.Contains(o.TargetEntityKey.Value) && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.NegationIndicator == true, AuthenticationContext.SystemPrincipal)
                    .Union(o => o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && falsePositives.Contains(o.SourceEntityKey.Value) && o.TargetEntityKey == masterKey && o.NegationIndicator == true)
                    .ToArray().Select(o => new EntityRelationship()
                    {
                        Key = o.Key,
                        BatchOperation = Model.DataTypes.BatchOperationType.Delete
                    }));
                persistenceBundle.AddRange(falsePositives.Select(o => new EntityRelationship()
                {
                    SourceEntityKey = o,
                    TargetEntityKey = masterKey,
                    RelationshipTypeKey = EntityRelationshipTypeKeys.Duplicate,
                    NegationIndicator = true
                }));
            }

            var retVal = this.m_bundlePersistenceService.Insert(persistenceBundle, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            return retVal.Item.Find(o => o.Key == masterKey);

        }

        /// <summary>
        /// Perform an un-ignore operation
        /// </summary>
        public IdentifiedData UnIgnore(Guid masterKey, IEnumerable<Guid> ignoredKeys)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.UnrestrictedClinicalData);
            var master = this.m_persistenceService.Get(masterKey, null, AuthenticationContext.Current.Principal);
            if (master == null)
            {
                throw new KeyNotFoundException($"{typeof(TModel).GetSerializationName()}/{masterKey}");
            }

            master.BatchOperation = Model.DataTypes.BatchOperationType.InsertOrUpdate;
            // Remove all the existing links
            var persistenceBundle = new Bundle() { Item = new List<IdentifiedData>() { master } };
            // Remove all duplicate indicators
            if (typeof(Act).IsAssignableFrom(typeof(TModel)))
            {
                // Remove all ignore measures
                persistenceBundle.AddRange(this.m_actRelationshipService.Query(o => o.NegationIndicator == true &&
                    o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate &&
                    (o.SourceEntityKey == masterKey && ignoredKeys.Contains(o.TargetActKey.Value)) || (o.TargetActKey == masterKey && ignoredKeys.Contains(o.SourceEntityKey.Value)), AuthenticationContext.SystemPrincipal)
                    .ToArray()
                    .Select(o => new ActRelationship()
                    {
                        Key = o.Key,
                        BatchOperation = Model.DataTypes.BatchOperationType.Delete
                    }));
            }
            else
            {
                // Remove all ignore measures
                persistenceBundle.AddRange(this.m_entityRelationshipService.Query(o => o.NegationIndicator == true &&
                    o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate &&
                    (o.SourceEntityKey == masterKey && ignoredKeys.Contains(o.TargetEntityKey.Value)) || (o.TargetEntityKey == masterKey && ignoredKeys.Contains(o.SourceEntityKey.Value)), AuthenticationContext.SystemPrincipal)
                    .ToArray()
                    .Select(o => new EntityRelationship()
                    {
                        Key = o.Key,
                        BatchOperation = Model.DataTypes.BatchOperationType.Delete
                    }));
            }

            var retVal = this.m_bundlePersistenceService.Insert(persistenceBundle, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            return retVal.Item.Find(o => o.Key == masterKey);
        }

        /// <summary>
        /// Get the merge candidates (those which might be a match)
        /// </summary>
        public IEnumerable<Guid> GetMergeCandidateKeys(Guid masterKey)
        {
            if (typeof(Act).IsAssignableFrom(typeof(TModel)))
            {
                return this.m_actRelationshipService.Query(o => o.SourceEntityKey == masterKey && o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate && o.NegationIndicator == false, AuthenticationContext.Current.Principal).Select(o => o.TargetActKey.Value);
            }
            else
            {
                return this.m_entityRelationshipService.Query(o => o.SourceEntityKey == masterKey && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.NegationIndicator == false, AuthenticationContext.Current.Principal).Select(o => o.TargetEntityKey.Value);
            }
        }

        /// <summary>
        /// Get the ignore list
        /// </summary>
        public IEnumerable<Guid> GetIgnoredKeys(Guid masterKey)
        {
            if (typeof(Act).IsAssignableFrom(typeof(TModel)))
            {
                return this.m_actRelationshipService.Query(o => o.SourceEntityKey == masterKey && o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate && o.NegationIndicator == true, AuthenticationContext.Current.Principal).Select(o => o.TargetActKey.Value);
            }
            else
            {
                return this.m_entityRelationshipService.Query(o => o.SourceEntityKey == masterKey && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.NegationIndicator == true, AuthenticationContext.Current.Principal).Select(o => o.TargetEntityKey.Value);
            }
        }

        private IdentifiedData AddMatchScoreTag(IdentifiedData identifiedData)
        {
            if (identifiedData is EntityRelationship er)
            {
                var rv = er.LoadProperty(p => p.TargetEntity);
                rv.AddTag(SystemTagNames.MatchScoreTag, er.Strength.ToString());
                return rv;
            }
            else if (identifiedData is ActRelationship ar)
            {
                var rv = ar.LoadProperty(p => p.TargetAct);
                //rv.AddTag(SystemTagNames.MatchScoreTag, ar.Strength.ToString());
                return rv;
            }
            else
            {
                throw new ArgumentOutOfRangeException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, identifiedData.GetType(), typeof(EntityRelationship)));
            }
        }

        /// <summary>
        /// Get merged candidates
        /// </summary>
        public IQueryResultSet<IdentifiedData> GetMergeCandidates(Guid masterKey)
        {
            if (typeof(Act).IsAssignableFrom(typeof(TModel)))
            {
                var candidateResults = this.m_actRelationshipService.Query(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate && o.SourceEntityKey == masterKey && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == false, AuthenticationContext.Current.Principal);
                return new TransformQueryResultSet<ActRelationship, IdentifiedData>(candidateResults, this.AddMatchScoreTag);
            }
            else
            {
                var candidateResults = this.m_entityRelationshipService.Query(o => o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.SourceEntityKey == masterKey && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == false, AuthenticationContext.Current.Principal);
                return new TransformQueryResultSet<EntityRelationship, IdentifiedData>(candidateResults, this.AddMatchScoreTag);
            }

        }

        /// <summary>
        /// Get ignored list
        /// </summary>
        public IQueryResultSet<IdentifiedData> GetIgnored(Guid masterKey)
        {
            if (typeof(Act).IsAssignableFrom(typeof(TModel)))
            {
                var candidateResults = this.m_actRelationshipService.Query(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate && o.SourceEntityKey == masterKey && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == true, AuthenticationContext.Current.Principal);
                return new TransformQueryResultSet<ActRelationship, IdentifiedData>(candidateResults, this.AddMatchScoreTag);
            }
            else
            {
                var candidateResults = this.m_entityRelationshipService.Query(o => o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.SourceEntityKey == masterKey && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == true, AuthenticationContext.Current.Principal);
                return new TransformQueryResultSet<EntityRelationship, IdentifiedData>(candidateResults, this.AddMatchScoreTag);
            }
        }

        /// <summary>
        /// Get global merge candidates
        /// </summary>
        public IQueryResultSet<ITargetedAssociation> GetGlobalMergeCandidates()
        {
            if (typeof(Act).IsAssignableFrom(typeof(TModel)))
            {
                var candidate = this.m_actRelationshipService.Query(o => o.RelationshipTypeKey == ActRelationshipTypeKeys.Duplicate && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == false, AuthenticationContext.SystemPrincipal);
                return new TransformQueryResultSet<ActRelationship, ITargetedAssociation>(candidate, (o) => o);
            }
            else
            {
                var candidate = this.m_entityRelationshipService.Query(o => o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == false, AuthenticationContext.SystemPrincipal);
                return new TransformQueryResultSet<EntityRelationship, ITargetedAssociation>(candidate, (o) => o);
            }
        }

        /// <summary>
        /// Re-run a global merge candidate scoring
        /// </summary>
        public void DetectGlobalMergeCandidates()
        {
            try
            {
                this.m_pepService.Demand(PermissionPolicyIdentifiers.UnrestrictedClinicalData);

                int maxWorkers = 1;
                if (ApplicationServiceContext.Current.HostType == SanteDBHostType.Server)
                {
                    maxWorkers = (Environment.ProcessorCount / 4) + 1;
                }
                var classKeys = typeof(TModel).GetClassKeys();

                // TODO: Make this a multi-threaded process
                using (var matchContext = new BackgroundMatchContext<TModel>(maxWorkers, this))
                {
                    matchContext.Start();

                    // Matcher queue
                    this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(nameof(DetectGlobalMergeCandidates), 0f, $"Gathering sources..."));

                    // Main matching loop - 
                    try
                    {
                        var recordsToProcess = this.m_persistenceService.Query(o => StatusKeys.ActiveStates.Contains(o.StatusConceptKey.Value) && classKeys.Contains(o.ClassConceptKey.Value), AuthenticationContext.SystemPrincipal);
                        var totalRecords = recordsToProcess.Count();
                        var rps = 0.0f;
                        var sw = new Stopwatch();
                        var nRecordsLoaded = 0;
                        sw.Start();

                        using (DataPersistenceControlContext.Create(LoadMode.QuickLoad))
                        {
                            foreach (var itm in recordsToProcess)
                            {
                                this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(nameof(DetectGlobalMergeCandidates), nRecordsLoaded++ / (float)totalRecords, $"Matching {matchContext.RecordsProcessed} recs @ {rps:#.#} r/s"));
                                rps = 1000.0f * (float)matchContext.RecordsProcessed / (float)sw.ElapsedMilliseconds;
                                matchContext.QueueLoadedRecord(itm);
                            }
                        }

                        sw.Stop();
                    }
                    catch (Exception e)
                    {
                        matchContext.Halt(e);
                    }
                    this.m_tracer.TraceVerbose("DetectGlobalMergeCandidate: Completed matching");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error running detect of global merge candidates", e);
            }
        }

        /// <summary>
        /// Clear the merge candidates
        /// </summary>
        public void ClearGlobalMergeCanadidates()
        {
            try
            {
                var classKeys = typeof(TModel).GetClassKeys();

                using (DataPersistenceControlContext.Create(DeleteMode.PermanentDelete).WithName("Clearing Merge Candidiates"))
                {
                    this.m_entityRelationshipService.DeleteAll(o => classKeys.Contains(o.SourceEntity.ClassConceptKey.Value) && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == false, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    this.m_actRelationshipService.DeleteAll(o => classKeys.Contains(o.SourceEntity.ClassConceptKey.Value) && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == false, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                }
            }
            catch (Exception e)
            {
                m_tracer.TraceError("Error clearing global merge candidates: {0}", e);
                throw new Exception("Error clearing global merge candidates", e);
            }
        }

        /// <summary>
        /// Reset the entire environment
        /// </summary>
        /// <remarks>Since SIM has no special capabilities - we just clear candidates</remarks>
        public void Reset(bool includeVerified, bool linksOnly)
        {
            ClearGlobalMergeCanadidates();
            ClearGlobalIgnoreFlags();
        }

        /// <summary>
        /// Clear global ignore flags
        /// </summary>
        public void ClearGlobalIgnoreFlags()
        {
            try
            {
                var classKeys = typeof(TModel).GetClassKeys();

                using (DataPersistenceControlContext.Create(DeleteMode.PermanentDelete).WithName("Clearing Ignore"))
                {
                    this.m_entityRelationshipService.DeleteAll(o => classKeys.Contains(o.SourceEntity.ClassConceptKey.Value) && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == true, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    this.m_actRelationshipService.DeleteAll(o => classKeys.Contains(o.SourceEntity.ClassConceptKey.Value) && o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate && o.ObsoleteVersionSequenceId == null && o.NegationIndicator == true, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                }
            }
            catch (Exception e)
            {
                m_tracer.TraceError("Error clearing global merge candidates: {0}", e);
                throw new Exception("Error clearing global merge candidates", e);
            }
        }

        /// <summary>
        /// Clear merge candidates from the database
        /// </summary>
        public void ClearMergeCandidates(Guid masterKey)
        {
            try
            {
                var classKeys = typeof(TModel).GetClassKeys();

                using (DataPersistenceControlContext.Create(DeleteMode.PermanentDelete).WithName("Clearing Candidiates"))
                {
                    if (typeof(Entity).IsAssignableFrom(typeof(TModel)))
                    {
                        this.m_entityRelationshipService.DeleteAll(o => classKeys.Contains(o.TargetEntity.ClassConceptKey.Value) &&
                            (o.TargetEntityKey == masterKey || o.SourceEntityKey == masterKey) &&
                            o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate &&
                            o.ObsoleteVersionSequenceId == null &&
                            o.NegationIndicator == false, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    }
                    else
                    {
                        this.m_actRelationshipService.DeleteAll(o => classKeys.Contains(o.TargetAct.ClassConceptKey.Value) &&
                            (o.TargetActKey == masterKey || o.SourceEntityKey == masterKey) &&
                            o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate &&
                            o.ObsoleteVersionSequenceId == null &&
                            o.NegationIndicator == false, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    }
                }
            }
            catch (Exception e)
            {
                m_tracer.TraceError("Error clearing global merge candidates: {0}", e);
                throw new Exception("Error clearing global merge candidates", e);
            }
        }

        /// <summary>
        /// Clear ignore flags
        /// </summary>
        public void ClearIgnoreFlags(Guid masterKey)
        {
            try
            {
                var classKeys = typeof(TModel).GetClassKeys();
                using (DataPersistenceControlContext.Create(DeleteMode.PermanentDelete).WithName("Clearing Candidiates"))
                {
                    if (typeof(Entity).IsAssignableFrom(typeof(TModel)))
                    {
                        this.m_entityRelationshipService.DeleteAll(o => classKeys.Contains(o.TargetEntity.ClassConceptKey.Value) &&
                            (o.TargetEntityKey == masterKey || o.SourceEntityKey == masterKey) &&
                            o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate &&
                            o.ObsoleteVersionSequenceId == null &&
                            o.NegationIndicator == true, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    }
                    else
                    {
                        this.m_actRelationshipService.DeleteAll(o => classKeys.Contains(o.TargetAct.ClassConceptKey.Value) &&
                            (o.TargetActKey == masterKey || o.SourceEntityKey == masterKey) &&
                            o.RelationshipTypeKey == EntityRelationshipTypeKeys.Duplicate &&
                            o.ObsoleteVersionSequenceId == null &&
                            o.NegationIndicator == true, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    }
                }
            }
            catch (Exception e)
            {
                m_tracer.TraceError("Error clearing global merge candidates: {0}", e);
                throw new Exception("Error clearing global merge candidates", e);
            }
        }

        /// <summary>
        /// Reset all data for specified key
        /// </summary>
        public void Reset(Guid masterKey, bool includeVerified, bool linksOnly)
        {
            ClearMergeCandidates(masterKey);
            ClearIgnoreFlags(masterKey);
        }

        /// <summary>
        /// Dispose of this handler
        /// </summary>
        public void Dispose()
        {
            this.m_persistenceService.Inserting -= this.OnInserting;
            this.m_persistenceService.Updating -= this.OnSaving;
            this.m_persistenceService.Deleted -= this.OnDeleted;
        }

        public IEnumerable<IdentifiedData> DoMatchingLogic(IdentifiedData inputRecord) => this.DoDataMatchingLogicInternal((TModel)inputRecord);
        public IEnumerable<IdentifiedData> DoDeletionLogic(IdentifiedData inputRecord) => this.DoDataDeletionLogicInternal((TModel)inputRecord);

        /// <inheritdoc/>
        public void DetectMergeCandidates(Guid masterKey)
        {
            try
            {
                this.ClearMergeCandidates(masterKey);
                using (DataPersistenceControlContext.Create(LoadMode.FullLoad).WithName("Matching Context"))
                {
                    var sourceRecord = this.m_persistenceService.Get(masterKey, Guid.Empty, AuthenticationContext.Current.Principal);
                    if (sourceRecord == null)
                    {
                        throw new KeyNotFoundException($"{typeof(TModel).GetSerializationName()}/{masterKey}");
                    }
                    var persistenceBundle = new Bundle(this.DoDeletionLogic(sourceRecord));
                    this.m_bundlePersistenceService.Insert(persistenceBundle, TransactionMode.Commit, AuthenticationContext.Current.Principal);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error detecting merge candidates for {typeof(TModel).GetSerializationName()}/{masterKey}", e);
            }

        }
    }

}
