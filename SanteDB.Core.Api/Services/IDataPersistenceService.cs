/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Event;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Principal;
using SanteDB.Core.Model.Query;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Data persistence modes
    /// </summary>
    public enum TransactionMode
    {
        /// <summary>
        /// Inherit the persistence mode from a parent context
        /// </summary>
        None,
        /// <summary>
        /// Debug mode, this means nothing is actually committed to the database
        /// </summary>
        Rollback,
        /// <summary>
        /// Production, everything is for reals
        /// </summary>
        Commit
    }



    /// <summary>
    /// Represents a data persistence service which is capable of storing and retrieving data
    /// to/from a data store
    /// </summary>
    public interface IDataPersistenceService<TData> : IServiceImplementation where TData : IdentifiedData
    {
        /// <summary>
        /// Occurs when inserted.
        /// </summary>
        event EventHandler<DataPersistedEventArgs<TData>> Inserted;
        /// <summary>
        /// Occurs when inserting.
        /// </summary>
        event EventHandler<DataPersistingEventArgs<TData>> Inserting;
        /// <summary>
        /// Occurs when updated.
        /// </summary>
        event EventHandler<DataPersistedEventArgs<TData>> Updated;
        /// <summary>
        /// Occurs when updating.
        /// </summary>
        event EventHandler<DataPersistingEventArgs<TData>> Updating;
        /// <summary>
        /// Occurs when obsoleted.
        /// </summary>
        event EventHandler<DataPersistedEventArgs<TData>> Obsoleted;
        /// <summary>
        /// Occurs when obsoleting.
        /// </summary>
        event EventHandler<DataPersistingEventArgs<TData>> Obsoleting;
        /// <summary>
        /// Occurs when queried.
        /// </summary>
        event EventHandler<QueryResultEventArgs<TData>> Queried;
        /// <summary>
        /// Occurs when querying.
        /// </summary>
        event EventHandler<QueryRequestEventArgs<TData>> Querying;

        /// <summary>
        /// Data is being retrieved
        /// </summary>
        event EventHandler<DataRetrievingEventArgs<TData>> Retrieving;

        /// <summary>
        /// Fired when data has been retrieved
        /// </summary>
        event EventHandler<DataRetrievedEventArgs<TData>> Retrieved;

        /// <summary>
        /// Insert the specified data.
        /// </summary>
        /// <param name="data">Data.</param>
        TData Insert(TData data, TransactionMode mode, IPrincipal principal);

        /// <summary>
        /// Update the specified data
        /// </summary>
        /// <param name="data">Data.</param>
        TData Update(TData data, TransactionMode mode, IPrincipal principal);

        /// <summary>
        /// Obsolete the specified identified data
        /// </summary>
        /// <param name="data">Data.</param>
        TData Obsolete(TData data, TransactionMode mode, IPrincipal principal);

        /// <summary>
        /// Get the specified key.
        /// </summary>
        /// <param name="key">Key.</param>
        TData Get(Guid key, Guid? versionKey, bool loadFast, IPrincipal principal);

        /// <summary>
        /// Query the specified data
        /// </summary>
        /// <param name="query">Query.</param>
        IEnumerable<TData> Query(Expression<Func<TData, bool>> query, IPrincipal principal);

        /// <summary>
        /// Query the specified data
        /// </summary>
        /// <param name="query">Query.</param>
        IEnumerable<TData> Query(Expression<Func<TData, bool>> query, int offset, int? count, out int totalResults, IPrincipal principal, params ModelSort<TData>[] orderBy);
        
        /// <summary>
        /// Performs a fast count
        /// </summary>
        long Count(Expression<Func<TData, bool>> p, IPrincipal authContext = null);

    }

    /// <summary>
    /// Non-generic form of the data persistene service
    /// </summary>
    public interface IDataPersistenceService
    {
        /// <summary>
        /// Inserts the specified object
        /// </summary>
        Object Insert(Object data);

        /// <summary>
        /// Updates the specified data
        /// </summary>
        Object Update(Object data);

        /// <summary>
        /// Obsoletes the specified data
        /// </summary>
        Object Obsolete(Object data);

        /// <summary>
        /// Gets the specified data
        /// </summary>
        Object Get(Guid id);

        /// <summary>
        /// Query based on the expression given
        /// </summary>
        IEnumerable Query(Expression query, int offset, int? count, out int totalResults);
    }

}