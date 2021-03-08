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
using SanteDB.Core.Event;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Patch;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Represents a service which appropriately merges / unmerges records
    /// </summary>
    public interface IRecordMergingService<T> : IServiceImplementation
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

        /// <summary>
        /// Gets the duplicates for the specified master record
        /// </summary>
        /// <param name="masterKey">The master record</param>
        /// <returns>The duplicates currently identified/queried</returns>
        IEnumerable<T> GetDuplicates(Guid masterKey);

        /// <summary>
        /// Gets the ignore list for the specified master record
        /// </summary>
        /// <param name="masterKey">The master record</param>
        /// <returns>The records which will be ignored</returns>
        IEnumerable<T> GetIgnored(Guid masterKey);

        /// <summary>
        /// Indicates that the engine should ignore the specified false positives
        /// </summary>
        /// <param name="masterKey">The master record which has been identified</param>
        /// <param name="falsePositives">The list of false positives to be flagged</param>
        /// <returns>The updated master record</returns>
        T Ignore(Guid masterKey, IEnumerable<Guid> falsePositives);

        /// <summary>
        /// Indicates that an ignored record should be removed from the ignore list
        /// </summary>
        /// <param name="masterKey">The master record which has been identified</param>
        /// <param name="ignoredKeys">The list of ignored keys to be re-considered</param>
        /// <returns>The updated master record</returns>
        T UnIgnore(Guid masterKey, IEnumerable<Guid> ignoredKeys);

        /// <summary>
        /// Merges the specified <paramref name="linkedDuplicates"/> into <paramref name="master"/>
        /// </summary>
        /// <param name="masterKey">The master record to which the linked duplicates are to be attached</param>
        /// <param name="linkedDuplicates">The linked records to be merged to master</param>
        /// <returns>The newly updated master record</returns>
        T Merge(Guid masterKey, IEnumerable<Guid> linkedDuplicates);

        /// <summary>
        /// Un-merges the specified <paramref name="unmergeDuplicate"/> from <paramref name="master"/>
        /// </summary>
        /// <param name="masterKey">The master record from which a duplicate is to be removed</param>
        /// <param name="unmergeDuplicateKey">The record which is to be unmerged</param>
        /// <returns>The newly created master record from which <paramref name="unmergeDuplicateKey"/> was created</returns>
        T Unmerge(Guid masterKey, Guid unmergeDuplicateKey);

        /// <summary>
        /// Gets the differences between the master and the linked duplicate
        /// </summary>
        /// <param name="masterKey">The key of the master</param>
        /// <param name="linkedDuplicateKey">The key of the linked duplicate</param>
        /// <returns>The patch representing the difference</returns>
        Patch Diff(Guid masterKey, Guid linkedDuplicateKey);

        /// <summary>
        /// Clears all candidates and re-runs the specified <paramref name="configurationName"/> for the entire database
        /// </summary>
        /// <param name="configurationName">The name of the configuration to run</param>
        void FlagDuplicates(String configurationName = null);

        /// <summary>
        /// Clears the candidates and re-runs the specified <paramref name="configurationName"/> for the specified record
        /// </summary>
        /// <param name="key">The key of the record to flag duplicates for</param>
        /// <param name="configurationName">The configuration name to use</param>
        T FlagDuplicates(Guid key, String configurationName = null);

    }
}
