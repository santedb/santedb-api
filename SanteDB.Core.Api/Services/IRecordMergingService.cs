﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Event;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Record merge statuys
    /// </summary>
    public enum RecordMergeStatus
    {
        /// <summary>
        /// Merge was completed as instructed
        /// </summary>
        Success = 0x0,

        /// <summary>
        /// Merge was cancelled
        /// </summary>
        Aborted = 0x1,

        /// <summary>
        /// An alternate merging strategy was used
        /// </summary>
        Alternate = 0x2,

        /// <summary>
        /// A link was used instead 
        /// </summary>
        LinkInsteadOfMerge = 0x4,

        /// <summary>
        /// A destructive merge was used
        /// </summary>
        DestructiveMerge = 0x8
    }

    /// <summary>
    /// Represents a record merging result
    /// </summary>
    public class RecordMergeResult
    {
        /// <summary>
        /// Indicates if the operation was a success
        /// </summary>
        public RecordMergeStatus Status { get; }

        /// <summary>
        /// Gets the records which survived the merge operation
        /// </summary>
        public IEnumerable<Guid> Survivors { get; }

        /// <summary>
        /// Gets the records which did not survive the merge operation
        /// </summary>
        public IEnumerable<Guid> Replaced { get; }

        /// <summary>
        /// Creates a new record merge result
        /// </summary>
        public RecordMergeResult(RecordMergeStatus status, Guid[] survivors, Guid[] replaced)
        {
            this.Status = status;
            this.Replaced = replaced;
            this.Survivors = survivors;
        }
    }

    /// <summary>
    /// Record merging service
    /// </summary>
    [System.ComponentModel.Description("Record Merging Provider")]
    public interface IRecordMergingService : IServiceImplementation
    {
        /// <summary>
        /// Gets the duplicates for the specified master record
        /// </summary>
        /// <param name="masterKey">The master record</param>
        /// <returns>The duplicates currently identified/queried</returns>
        IEnumerable<Guid> GetMergeCandidateKeys(Guid masterKey);

        /// <summary>
        /// Get merge candidate keys
        /// </summary>
        /// <param name="masterKey">The key of the master</param>
        IQueryResultSet<IdentifiedData> GetMergeCandidates(Guid masterKey);

        /// <summary>
        /// Get all merge candidates
        /// </summary>
        IQueryResultSet<ITargetedAssociation> GetGlobalMergeCandidates();

        /// <summary>
        /// Gets the ignore list for the specified master record
        /// </summary>
        IEnumerable<Guid> GetIgnoredKeys(Guid masterKey);

        /// <summary>
        /// Gets the ignore list for the specified master record
        /// </summary>
        IQueryResultSet<IdentifiedData> GetIgnored(Guid masterKey);

        /// <summary>
        /// Indicates that the engine should ignore the specified false positives
        /// </summary>
        /// <param name="masterKey">The master record which has been identified</param>
        /// <param name="falsePositives">The list of false positives to be flagged</param>
        /// <returns>The updated master record</returns>
        IdentifiedData Ignore(Guid masterKey, IEnumerable<Guid> falsePositives);

        /// <summary>
        /// Indicates that an ignored record should be removed from the ignore list
        /// </summary>
        /// <param name="masterKey">The master record which has been identified</param>
        /// <param name="ignoredKeys">The list of ignored keys to be re-considered</param>
        /// <returns>The updated master record</returns>
        IdentifiedData UnIgnore(Guid masterKey, IEnumerable<Guid> ignoredKeys);

        /// <summary>
        /// Merges the specified <paramref name="linkedDuplicates"/> into <paramref name="masterKey"/>
        /// </summary>
        /// <param name="masterKey">The master record to which the linked duplicates are to be attached</param>
        /// <param name="linkedDuplicates">The linked records to be merged to master</param>
        /// <returns>The newly updated master record</returns>
        RecordMergeResult Merge(Guid masterKey, IEnumerable<Guid> linkedDuplicates);

        /// <summary>
        /// Un-merges the specified <paramref name="unmergeDuplicateKey"/> from <paramref name="masterKey"/>
        /// </summary>
        /// <param name="masterKey">The master record from which a duplicate is to be removed</param>
        /// <param name="unmergeDuplicateKey">The record which is to be unmerged</param>
        /// <returns>The newly created master record from which <paramref name="unmergeDuplicateKey"/> was created</returns>
        RecordMergeResult Unmerge(Guid masterKey, Guid unmergeDuplicateKey);

        /// <summary>
        /// Reset the merge links in the data
        /// </summary>
        void DetectGlobalMergeCandidates();

        /// <summary>
        /// Clear global merge candidates
        /// </summary>
        void ClearGlobalMergeCanadidates();

        /// <summary>
        /// Clear all ignore flags
        /// </summary>
        void ClearGlobalIgnoreFlags();

        /// <summary>
        /// Clear all merge candidates
        /// </summary>
        void ClearMergeCandidates(Guid masterKey);

        /// <summary>
        /// Clear ignored flags
        /// </summary>
        void ClearIgnoreFlags(Guid masterKey);

        /// <summary>
        /// Perform the necessary operations to detect merge candidates for <paramref name="masterKey" />
        /// </summary>
        /// <param name="masterKey">The key of the master record for which merge candidtes should be detected</param>
        void DetectMergeCandidates(Guid masterKey);

        /// <summary>
        /// Provides a request to cancel the detection of merge candidates
        /// </summary>
        void CancelDetectGlobalMergeCandidates();

        /// <summary>
        /// Reset the specified merge service data on the specified record
        /// </summary>
        void Reset(Guid masterKey, bool includeVerified, bool linksOnly);

        /// <summary>
        /// Removes all data related to matching and re-runs the matching
        /// </summary>
        /// <param name="includeVerified">When true, even data/links which are marked as "verified" should be removed</param>
        /// <param name="linksOnly">When true, only the links between objects should be removed, when false the implementer should remove links and any orphaned records</param>
        void Reset(bool includeVerified, bool linksOnly);
    }

    /// <summary>
    /// Represents a service which appropriately merges / unmerges records
    /// </summary>
    public interface IRecordMergingService<T> : IRecordMergingService
        where T : IdentifiedData
    {
        /// <summary>
        /// Fired prior to a merge occurring
        /// </summary>
        event EventHandler<DataMergingEventArgs<T>> Merging;

        /// <summary>
        /// Fired after a merge has occurred
        /// </summary>
        event EventHandler<DataMergeEventArgs<T>> Merged;

        /// <summary>
        /// Fired prior to a merge occurring
        /// </summary>
        event EventHandler<DataMergingEventArgs<T>> UnMerging;

        /// <summary>
        /// Fired after a merge has occurred
        /// </summary>
        event EventHandler<DataMergeEventArgs<T>> UnMerged;
    }
}