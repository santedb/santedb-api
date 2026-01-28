/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Jobs;
using SanteDB.Core.Model;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Service contract for data archival and purging
    /// </summary>
    /// <remarks>
    /// <para>The data archive service is used by various jobs throughout SanteDB iCDR (such as the <see cref="DataRetentionJob"/>) to 
    /// copy records which are past their retention rules to a secondary data storage facility. This service can be used for long-term
    /// archival of old (non-clinically relevant data) data and supports the purging of data which is no longer needed.</para>
    /// </remarks>
    [System.ComponentModel.Description("Data Archiving Service")]
    public interface IDataArchiveService : IServiceImplementation
    {
        /// <summary>
        /// Push the specified records to the archive
        /// </summary>
        /// <param name="keysToBeArchived">The identifiers of the objects in the primary <see cref="IDataPersistenceService"/> which are to be copied to the archive</param>
        /// <param name="modelType">The type of data that is being archived</param>
        void Archive(Type modelType, params Guid[] keysToBeArchived);

        /// <summary>
        /// Retrieve a record from the archive by key and type
        /// </summary>
        /// <param name="modelType">The type of data which is being retrieved from the archive</param>
        /// <param name="keyToRetrieve">The key of the data to be retrieved from the archive</param>
        IdentifiedData Retrieve(Type modelType, Guid keyToRetrieve);

        /// <summary>
        /// Validates whether the specified key exists in the archive
        /// </summary>
        /// <param name="modelType">The type of data to determine existence for</param>
        /// <param name="keyToCheck">The key to check for existence</param>
        bool Exists(Type modelType, Guid keyToCheck);

        /// <summary>
        /// Purge the specified object from the archive
        /// </summary>
        /// <param name="modelType">The type of data to be purged</param>
        /// <param name="keysToBePurged">The keys of the data to be purged</param>
        void Purge(Type modelType, params Guid[] keysToBePurged);

    }

}
