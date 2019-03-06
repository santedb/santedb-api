/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: justi
 * Date: 2019-1-12
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
        /// Initializes a new instance of the <see cref="SanteDB.SanteDB.Core.Services.DataQueryResultEventArgs"/> class.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="results">Results.</param>
        public QueryResultEventArgs(Expression<Func<TData, bool>> query, IEnumerable<TData> results, int offset, int? count, int totalResults, Guid? queryId, IPrincipal principal) : base(query, offset, count, queryId, principal)
        {
            this.Results = results;
            this.TotalResults = totalResults;
        }


    }

    /// <summary>
    /// Data query pre event arguments.
    /// </summary>
    public class QueryRequestEventArgs<TData> : QueryEventArgsBase<TData> where TData : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SanteDB.SanteDB.Core.Services.DataQueryPreEventArgs"/> class.
        /// </summary>
        /// <param name="query">Query.</param>
        public QueryRequestEventArgs(Expression<Func<TData, bool>> query, int offset, int? count, Guid? queryId, IPrincipal principal) : base(query, offset, count, queryId, principal)
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
    /// Data persistence pre event arguments.
    /// </summary>
    public class DataPersistingEventArgs<TData> : DataPersistedEventArgs<TData> where TData : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SanteDB.SanteDB.Core.Services.DataPersistencePreEventArgs"/> class.
        /// </summary>
        /// <param name="data">Data.</param>
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
    public class DataPersistedEventArgs<TData> : SecureAccessEventArgs where TData : class
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SanteDB.SanteDB.Core.Services.DataPersistenceEventArgs"/> class.
        /// </summary>
        /// <param name="data">Data.</param>
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
    public class DataRetrievingEventArgs<TData> : SecureAccessEventArgs
        where TData : IdentifiedData
    {

        /// <summary>
        /// Creates a new pre-retrieval event args object
        /// </summary>
        public DataRetrievingEventArgs(Guid? identifier, Guid? versionId, IPrincipal overrideAuthContext = null) : base(overrideAuthContext) 
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
    public class DataRetrievedEventArgs<TData> : SecureAccessEventArgs
        where TData : IdentifiedData
    {

        /// <summary>
        /// Post retrieval data
        /// </summary>
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
