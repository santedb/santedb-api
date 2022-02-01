/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-27
 */
using SanteDB.Core.Jobs;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Contract which defines free-text search functionality
    /// </summary>
    /// <remarks>
    /// <para>In SanteDB HDSI, the <c>_any</c> parameter can be used by a client to indicate that the caller does not
    /// care which field the value matches, rather the client is performing a free-text or full-text search. Full text
    /// searches can be used to search for content like: <c>John Smith HBA1C December</c>. Such requests are passed
    /// to the <see cref="IFreetextSearchService"/> as a series of terms provided by the client.</para>
    /// <para>Implementers are expected to call their full-text technology provider to perform the search. Additionally, 
    /// implementers should provide an <see cref="IJob"/> implementation (or should subscribe to updates from the <see cref="IDataPersistenceService"/>)
    /// to maintain the index.</para>
    /// <para>Implementations of the freetext search service may be technologies like Apache Lucene, PostgreSQL Free-Text Search, Amazon Elastic Search, etc.</para>
    /// </remarks>
    [System.ComponentModel.Description("Freetext Search Provider")]
    public interface IFreetextSearchService : IServiceImplementation
    {

        /// <summary>
        /// Search the provider of freetext indexing for any term provided
        /// </summary>
        /// <param name="count">The number of results which the caller wishes returned</param>
        /// <param name="offset">The offset of the first record to be returned</param>
        /// <param name="orderBy">The ordering desired by the caller</param>
        /// <param name="queryId">The unique identifier for the query (cached results)</param>
        /// <param name="term">The search term(s) provided by the user</param>
        /// <param name="totalResults">The total results matching the query</param>
        /// <returns>The entities which match the provided search</returns>
        /// <typeparam name="TEntity">The type of entity being searched</typeparam>
        IEnumerable<TEntity> Search<TEntity>(String[] term, Guid queryId, int offset, int? count, out int totalResults, ModelSort<TEntity>[] orderBy) where TEntity : IdentifiedData, new();

    }

}
