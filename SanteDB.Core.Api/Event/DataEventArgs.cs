﻿/*
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
using SanteDB.Core.Model;
using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Event
{

    /// <summary>
    /// Base class for query event args
    /// </summary>
    public abstract class QueryEventArgsBase<TData> : SecureAccessEventArgs where TData : class
    {

        /// <summary>
        /// Data query event ctor
        /// </summary>
        public QueryEventArgsBase(Expression<Func<TData, bool>> query, int offset, int? count, Guid? queryId, IPrincipal principal) : base(principal)
        {
            this.Offset = offset;
            this.Count = count;
            this.QueryId = queryId;
            this.Query = query;
        }

        /// <summary>
        /// Gets the offset requested
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets the count requested
        /// </summary>
        public int? Count { get; set; }


        /// <summary>
        /// Gets the total amount of results
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>The results.</value>
        public IEnumerable<TData> Results
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the query ID
        /// </summary>
        public Guid? QueryId { get; }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>The query.</value>
        public Expression<Func<TData, bool>> Query
        {
            get;
            set;
        }

    }

    /// <summary>
    /// Data query result event arguments.
    /// </summary>
    public class QueryResultEventArgs<TData> : QueryEventArgsBase<TData> where TData : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResultEventArgs{TData}"/> class.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="results">Results.</param>
        /// <param name="count">The number of results which are to be returned</param>
        /// <param name="offset">The offset of the result set</param>
        /// <param name="principal">The principal under which the query was executed</param>
        /// <param name="queryId">The unique identifier for the query</param>
        /// <param name="totalResults">The total results in the result set</param>
        public QueryResultEventArgs(Expression<Func<TData, bool>> query, IEnumerable<TData> results, int offset, int? count, int totalResults, Guid? queryId, IPrincipal principal) : base(query, offset, count, queryId, principal)
        {
            this.Results = results;
            this.TotalResults = totalResults;
        }


    }

    /// <summary>
    /// Data query pre event arguments.
    /// </summary>
    /// <remarks>This event allows cancellation and re-writing of queries by plugins prior to the query being executed on the persistence layer</remarks>
    public class QueryRequestEventArgs<TData> : QueryEventArgsBase<TData> where TData : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryRequestEventArgs{TData}"/> class.
        /// </summary>
        /// <param name="query">The query about to be executed</param>
        /// <param name="queryId">The query identifier</param>
        /// <param name="principal">The principal which is executing the query</param>
        /// <param name="offset">The requested offset in the result set</param>
        /// <param name="count">The requested total results to be returned in this result set</param>
        /// <param name="tag">A query tag object</param>
        public QueryRequestEventArgs(Expression<Func<TData, bool>> query, int offset, int? count, Guid? queryId, IPrincipal principal, dynamic tag = null) : base(query, offset, count, queryId, principal)
        {
            this.QueryTag = tag;
        }

        /// <summary>
        /// Gets the query tag
        /// </summary>
        public dynamic QueryTag
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance cancel.
        /// </summary>
        /// <value><c>true</c> if this instance cancel; otherwise, <c>false</c>.</value>
        public bool Cancel
        {
            get;
            set;
        }
        
        /// <summary>
        /// Instructs the query engine to use fuzzy totals
        /// </summary>
        public bool UseFuzzyTotals { get; set; }
    }

    /// <summary>
    /// Data persistence pre event arguments.
    /// </summary>
    /// <remarks>This class allows for cancelation of a persistence event (create, update, delete) by plugins and/or modification of the data to be persisted</remarks>
    public class DataPersistingEventArgs<TData> : DataPersistedEventArgs<TData> where TData : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPersistingEventArgs{TData}"/> class.
        /// </summary>
        /// <param name="data">The data to be persisted</param>
        /// <param name="principal">The principal under which the persistence is taking place</param>
        public DataPersistingEventArgs(TData data, IPrincipal principal) : base(data, principal)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance cancel.
        /// </summary>
        /// <value><c>true</c> if this instance cancel; otherwise, <c>false</c>.</value>
        public bool Cancel
        {
            get;
            set;
        }

    }

    /// <summary>
    /// Data persistence event arguments.
    /// </summary>
    /// <remarks>This event is fired whenever data has been successfully perssited to the database</remarks>
    public class DataPersistedEventArgs<TData> : SecureAccessEventArgs where TData : class
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPersistedEventArgs{TData}"/> class.
        /// </summary>
        /// <param name="data">The data that was persisted</param>
        /// <param name="principal">The principal which was responsible for the creation of the data</param>
        public DataPersistedEventArgs(TData data, IPrincipal principal) : base(principal)
        {
            this.Data = data;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public TData Data
        {
            get;
            set;
        }

    }

    /// <summary>
    /// Represents event data associated with a data retrieval operation
    /// </summary>
    /// <remarks>This event allows for the cancelation / inspection of queries to the data store prior to being executed</remarks>
    public class DataRetrievingEventArgs<TData> : SecureAccessEventArgs
        where TData : IdentifiedData
    {

        /// <summary>
        /// Creates a new pre-retrieval event args object
        /// </summary>
        /// <param name="identifier">The identifier of the object being retrieved</param>
        /// <param name="principal">The principal under which the query is being executed, or null if the current <see cref="AuthenticationContext"/> is being used</param>
        /// <param name="versionId">The version identifier of the object being retrieved if supplied</param>
        public DataRetrievingEventArgs(Guid? identifier, Guid? versionId, IPrincipal principal = null) : base(principal) 
        {
            this.Id = identifier;
            this.VersionId = versionId;
        }

        /// <summary>
        /// The identifier
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// The identifier
        /// </summary>
        public Guid? VersionId { get; set; }

        /// <summary>
        /// Gets the data retrieved
        /// </summary>
        public TData Result { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance cancel.
        /// </summary>
        /// <value><c>true</c> if this instance cancel; otherwise, <c>false</c>.</value>
        public bool Cancel
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A class used to store event information related to post-retrieval events
    /// </summary>
    /// <remarks>This event allows for inpection of data which was retrieved by identifier</remarks>
    public class DataRetrievedEventArgs<TData> : SecureAccessEventArgs
        where TData : IdentifiedData
    {

        /// <summary>
        /// Post retrieval data
        /// </summary>
        /// <param name="data">The data which was retrieved from the database</param>
        /// <param name="executionPrincipal">The prinicpal under which the query was executed</param>
        public DataRetrievedEventArgs(TData data, IPrincipal executionPrincipal) : base(executionPrincipal)
        {
            this.Data = data;
        }

        /// <summary>
        /// Gets the data retrieved
        /// </summary>
        public TData Data { get; set; }
    }

}
