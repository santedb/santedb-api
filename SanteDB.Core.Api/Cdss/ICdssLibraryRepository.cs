/*
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
 * Date: 2024-6-21
 */
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using System;
using System.Linq.Expressions;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Represents a generic repository which is used for the management of <see cref="ICdssAsset"/>
    /// </summary>
    /// <remarks>The clinical protocol asset repository is responsible for the storage and creation of relevant 
    /// <see cref="ICdssProtocol"/> and <see cref="ICdssLibrary"/> instances which are used by the CDSS 
    /// engine to actually perform their duties</remarks>
    public interface ICdssLibraryRepository : IServiceImplementation
    {

        /// <summary>
        /// Find all protocol assets which match the specified filter
        /// </summary>
        /// <param name="filter">The filter to be applied</param>
        /// <returns>The query result set containing the objects</returns>
        IQueryResultSet<ICdssLibrary> Find(Expression<Func<ICdssLibrary, bool>> filter);

        /// <summary>
        /// Get the protocol asset by identifier
        /// </summary>
        /// <param name="libraryUuid">The protocol asset identifier</param>
        /// <param name="versionUuid">The version of the data to fetch</param>
        /// <returns>The protocol asset with the matching asset id</returns>
        ICdssLibrary Get(Guid libraryUuid, Guid? versionUuid);

        /// <summary>
        /// Insert a protocol asset into the store
        /// </summary>
        /// <param name="libraryToInsert">The protocol asset to insert</param>
        /// <returns>The inserted protocol asset</returns>
        ICdssLibrary InsertOrUpdate(ICdssLibrary libraryToInsert);

        /// <summary>
        /// Remove a protocol asset from the repository by identifier
        /// </summary>
        /// <param name="libraryUuid">The protocol asset to be removed</param>
        /// <returns>The removed protocol asset definition</returns>
        ICdssLibrary Remove(Guid libraryUuid);

    }
}
