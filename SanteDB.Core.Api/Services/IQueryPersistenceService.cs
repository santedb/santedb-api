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
 * Date: 2023-6-21
 */
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Defines a service which can store the results of a query for later retrieval
    /// </summary>
    /// <remarks>
    /// <para>When implementing query on a shared infrastructure such as the SanteDB iCDR or dCDR, it is important that consistent result sets be returned. This is
    /// important not only for user interfaces, but is vital to consistent synchronization across devices.</para>
    /// <para>There are several reasons we keep stateful query result sets in SanteDB:</para>
    /// <list type="bullet">
    ///     <item>User interfaces will have consistent pages, so results don't magically "appear" between paging results</item>
    ///     <item>Synchronization processes have a consistent paging result set as they download data</item>
    ///     <item>The heavy initial query is done only once and results are merely accessed afterwards</item>
    /// </list>
    /// <para>Implementers of this service should ensure that whatever technology they are using to store queries have the capability of expiration (i.e. queries are
    /// not stateful indefinitely), support the rapid read and update of result sets, and are cabable of being aborted/abandoned.</para>
    /// </remarks>
    [System.ComponentModel.Description("Stateful Query Provider")]
    public interface IQueryPersistenceService : IServiceImplementation
    {
        /// <summary>
        /// Locate the stateful query identifier using the tag which was attached to the query
        /// </summary>
        /// <param name="queryTag">The tag value which was added to query information</param>
        /// <returns>The UUID of the stateful query</returns>
        /// <remarks>SanteDB needs to support multiple standards, each standard has a different way of representing query identifiers for stateful queries. For example
        /// in HL7v2 QBP messages, the query identifier be a number, a timestamp, a UUID or other representation. The messaging providers will TAG the query
        /// with this information, and this method allows those callers to cross-reference the tag they have appended to the query with the internal UUID of
        /// the query.</remarks>
        Guid FindQueryId(object queryTag);

        /// <summary>
        /// Registers a new query result set with the stateful query provider
        /// </summary>
        /// <param name="queryId">The unique identifier for the query</param>
        /// <param name="results">The results (initial or total) to be stored in the query</param>
        /// <param name="tag">A user tag for the query result set which can be used in implementation specific contexts</param>
        /// <param name="totalResults">The total number of results in the query set</param>
        /// <remarks>
        /// <para>This method registers a new query with the stateful query provider and provides an initial set of results in the result. The <paramref name="totalResults"/>
        /// and <paramref name="results"/> arrays may represent a partial or preliminary count and series of results. To prevent wasted loading of data from the database,
        /// only the key identifiers of the results in the query set should be retrieved and stored in the query set.</para>
        /// <para>It is possible for callers to further load more results into the query result set using the <see cref="AddResults(Guid, IEnumerable{Guid}, int)"/> method. For example, the ADO.NET
        /// provider only loads and counts the first 200 results for an initial return, and then appends the full query result keys to the stateful query.</para>
        /// </remarks>
        /// <returns>True if the query set was registered successfully</returns>
        /// <seealso cref="AddResults(Guid, IEnumerable{Guid}, int)"/>
        bool RegisterQuerySet(Guid queryId, IEnumerable<Guid> results, object tag, int totalResults);

        /// <summary>
        /// Returns true if the query identifier has been registered
        /// </summary>
        /// <param name="queryId">The UUID of the stateful query to determine state of</param>
        /// <returns>True if the <paramref name="queryId"/> has been registered with the query provider</returns>
        /// <remarks><para>This method is used by callers to determine whether an inbound query request has already been executed and registered. Typically when this method
        /// returns true, the caller invoke <see cref="GetQueryResults(Guid, int, int)"/> rather than re-query the database. This, in effect, locks the results</para></remarks>
        bool IsRegistered(Guid queryId);

        /// <summary>
        /// Retrieves the result keys located between <paramref name="offset"/> and <paramref name="offset"/>+<paramref name="count"/> in the stateful query provider
        /// </summary>
        /// <param name="queryId">The identifier of the stateful query from which results should be retrieved</param>
        /// <param name="offset">The offset in the stateful query result set where the first returned value should be.</param>
        /// <param name="count">The number of records to read from the stateful query result set</param>
        /// <returns>The UUIDs of the records which originally matched the query represented by <paramref name="queryId"/></returns>
        IEnumerable<Guid> GetQueryResults(Guid queryId, int offset, int count);

        /// <summary>
        /// Retrieves the query tag stored when the query was registered, for the specified <paramref name="queryId"/>
        /// </summary>
        /// <param name="queryId">The UUID of the stateful query for which the query tag should be returned</param>
        /// <returns>The registered tag on the query</returns>
        object GetQueryTag(Guid queryId);

        /// <summary>
        /// Count the number of query results which have been registered for <paramref name="queryId"/>
        /// </summary>
        /// <param name="queryId">The identifier of the query for which results should be counted</param>
        /// <returns>The number of registered query results for <paramref name="queryId"/></returns>
        long QueryResultTotalQuantity(Guid queryId);

        /// <summary>
        /// Adds more results to an already registered stateful query
        /// </summary>
        /// <remarks>This method is used when the caller is loading blocks of results from a source query. This is done because, depending on the database traffic and size of result
        /// set, it may take several seconds/minutes to fetch the entirety of the result set. Adding results to an already existing result set allows the caller to return the initial
        /// pages of results quickly, whilst taking a longer time to deep-load the remaining results.</remarks>
        /// <param name="queryId">The query id for which results should be added</param>
        /// <param name="results">The set of result keys which should be added to the registered stateful query</param>
        /// <param name="totalResults">If the total number of results in the stateful query (used for calculating the total number of pages) has changed, this is the value to set total results to</param>
        void AddResults(Guid queryId, IEnumerable<Guid> results, int totalResults);

        /// <summary>
        /// Adds or changes the query tag on <paramref name="queryId"/> to <paramref name="value"/>
        /// </summary>
        /// <param name="queryId">The unique query identifier for which the tag value is being set</param>
        /// <param name="value">The value of the tag to set on the query</param>
        void SetQueryTag(Guid queryId, object value);

        /// <summary>
        /// Abort the query <paramref name="queryId"/>
        /// </summary>
        /// <param name="queryId"></param>
        void AbortQuerySet(Guid queryId);
    }
}