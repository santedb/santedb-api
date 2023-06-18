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
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Principal;

namespace SanteDB.Core.Event
{
    /// <summary>
    /// Fired for free-text search queries
    /// </summary>
    public abstract class FreeTextQueryEventArgsBase : SecureAccessEventArgs
    {
        /// <summary>
        /// Creates a new freetext query event
        /// </summary>
        /// <param name="principal">The user which is executing the query</param>
        /// <param name="terms">The terms the user is searching for</param>
        public FreeTextQueryEventArgsBase(IPrincipal principal, String[] terms) : base(principal)
        {
        }

        /// <summary>
        /// Gets the search terms that were used for the query
        /// </summary>
        public String[] Terms { get; }
    }

    /// <summary>
    /// Fired before the freetext query
    /// </summary>
    public class FreeTextQueryRequestEventArgs<TData> : FreeTextQueryEventArgsBase
    {
        /// <summary>
        /// Fired before the
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="terms"></param>
        public FreeTextQueryRequestEventArgs(IPrincipal principal, string[] terms) : base(principal, terms)
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

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>The results.</value>
        public IQueryResultSet<TData> Results
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Fired before the freetext query
    /// </summary>
    public class FreeTextQueryResultEventArgs<TData> : FreeTextQueryEventArgsBase
    {
        /// <summary>
        /// Fired before the
        /// </summary>
        /// <param name="principal">The principal that executed the query</param>
        /// <param name="terms">The terms searched for</param>
        /// <param name="results">The results of the query</param>
        public FreeTextQueryResultEventArgs(IPrincipal principal, string[] terms, IQueryResultSet<TData> results) : base(principal, terms)
        {
            this.Results = results;
        }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>The results.</value>
        public IEnumerable<TData> Results
        {
            get;
        }
    }

    /// <summary>
    /// Base class for query event args
    /// </summary>
    public abstract class QueryEventArgsBase<TData> : SecureAccessEventArgs where TData : class
    {
        /// <summary>
        /// Data query event ctor
        /// </summary>
        public QueryEventArgsBase(Expression<Func<TData, bool>> query, IPrincipal principal) : base(principal)
        {
            this.Query = query;
        }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        /// <value>The query.</value>
        public Expression<Func<TData, bool>> Query
        {
            get;
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
        /// <param name="principal">The principal under which the query was executed</param>
        public QueryResultEventArgs(Expression<Func<TData, bool>> query, IQueryResultSet<TData> results, IPrincipal principal) : base(query, principal)
        {
            this.Results = results;
        }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>The results.</value>
        public IQueryResultSet<TData> Results
        {
            get; set;
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
        /// <param name="principal">The principal which is executing the query</param>
        public QueryRequestEventArgs(Expression<Func<TData, bool>> query, IPrincipal principal) : base(query, principal)
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

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>The results.</value>
        public IQueryResultSet<TData> Results
        {
            get;
            set;
        }
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
        /// <param name="mode">The mode of transaction (commit, rollback)</param>
        public DataPersistingEventArgs(TData data, TransactionMode mode, IPrincipal principal) : base(data, mode, principal)
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

        /// <summary>
        /// Gets or sets a value which indicates whether the event was successfully handled and inserted
        /// </summary>
        /// <remarks>This is used to notify the caller that it should assume operation continued as normal</remarks>
        public bool Success { get; set; }
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
        /// <param name="transactionMode">The mode of the transaction</param>
        public DataPersistedEventArgs(TData data, TransactionMode transactionMode, IPrincipal principal) : base(principal)
        {
            this.Data = data;
            this.Mode = transactionMode;
        }

        /// <summary>
        /// Gets the mode of transaction
        /// </summary>
        public TransactionMode Mode { get; }

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
        where TData : class, IIdentifiedResource
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
        where TData : class, IIdentifiedResource
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