﻿/*
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

using SanteDB.Core.Event;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Principal;

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
    /// Specified the method of deletion
    /// </summary>
    public enum DeleteMode
    {
        /// <summary>
        /// Logically delete the record - it should not appear in query results as the data is not accurate - it should be marked as inactive
        /// </summary>
        LogicalDelete = 0,

        /// <summary>
        /// The data is no longer active - it should be set to obsolete
        /// </summary>
        ObsoleteDelete = 1,

        /// <summary>
        /// Nullify the record - it never should have existed in the first place
        /// </summary>
        NullifyDelete = 2,

        /// <summary>
        /// The record should be marked as PURGED or marked in a state which it won't be returned from the datamodel
        /// </summary>
        VersionedDelete = 3,

        /// <summary>
        /// Permanently delete - it should be purged from the database
        /// </summary>
        PermanentDelete = 4
    }

    /// <summary>
    /// Load strategy
    /// </summary>
    public enum LoadMode
    {
        /// <summary>
        /// Quick loading - No properties are loaded and the caller must load
        /// </summary>
        QuickLoad = 0,

        /// <summary>
        /// Sync loading - only properties which are necessary for synchronization
        /// </summary>
        SyncLoad = 1,

        /// <summary>
        /// Full loading - load all properties
        /// </summary>
        FullLoad = 2
    }

    /// <summary>
    /// A query context class that allows the caller to specify / override the load settings for the .Query() methods
    /// </summary>
    public class DataPersistenceControlContext : IDisposable
    {
        // The current query context
        [ThreadStatic]
        private static DataPersistenceControlContext m_current;

        // Loading mode
        private readonly LoadMode? m_loadMode;

        // Delete mode
        private readonly DeleteMode? m_deleteMode;

        // Wrapped
        private readonly DataPersistenceControlContext m_wrapped;

        /// <summary>
        /// Constructor for query context
        /// </summary>
        private DataPersistenceControlContext(LoadMode loadingMode, DataPersistenceControlContext wrapped)
        {
            this.m_loadMode = loadingMode;
            this.m_wrapped = wrapped;
        }

        /// <summary>
        /// Constructor for query context
        /// </summary>
        private DataPersistenceControlContext(DeleteMode deleteMode, DataPersistenceControlContext wrapped)
        {
            this.m_deleteMode = deleteMode;
            this.m_wrapped = wrapped;

        }

        /// <summary>
        /// Constructor for query context
        /// </summary>
        private DataPersistenceControlContext(LoadMode loadMode, DeleteMode deleteMode, DataPersistenceControlContext wrapped)
        {
            this.m_deleteMode = deleteMode;
            this.m_loadMode = loadMode;
            this.m_wrapped = wrapped;
        }

        /// <summary>
        /// Gets the current query context
        /// </summary>
        public static DataPersistenceControlContext Current => m_current;

        /// <summary>
        /// Gets this context's load mode
        /// </summary>
        public LoadMode? LoadMode => this.m_loadMode;


        /// <summary>
        /// Gets this context's deletion mode
        /// </summary>
        public DeleteMode? DeleteMode => this.m_deleteMode;

        /// <summary>
        /// Sets the current loading mode
        /// </summary>
        /// <param name="loadMode"></param>
        /// <returns></returns>
        public static DataPersistenceControlContext Create(LoadMode loadMode)
        {
            m_current = new DataPersistenceControlContext(loadMode, m_current);
            return m_current;
        }

        /// <summary>
        /// Sets the current deletion mode for all persistence requests on this thread (or until a wrapped context is done)
        /// </summary>
        /// <param name="deleteMode">The mode of deletion</param>
        public static DataPersistenceControlContext Create(DeleteMode deleteMode)
        {
            m_current = new DataPersistenceControlContext(deleteMode, m_current);
            return m_current;
        }

        /// <summary>
        /// Unload the current
        /// </summary>
        public void Dispose()
        {
            m_current = m_wrapped;
        }
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
        [Obsolete("Use Deleted Event", true)]
        event EventHandler<DataPersistedEventArgs<TData>> Obsoleted;

        /// <summary>
        /// Occurs when obsoleting.
        /// </summary>
        [Obsolete("Use Deleting Event", true)]
        event EventHandler<DataPersistingEventArgs<TData>> Obsoleting;

        /// <summary>
        /// Occurs when obsoleted.
        /// </summary>
        event EventHandler<DataPersistedEventArgs<TData>> Deleted;

        /// <summary>
        /// Occurs when obsoleting.
        /// </summary>
        event EventHandler<DataPersistingEventArgs<TData>> Deleting;

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
        /// <param name="principal">The principal which is executing the insert</param>
        /// <param name="transactionMode">The mode of insert (commit or rollback for testing)</param>
        TData Insert(TData data, TransactionMode transactionMode, IPrincipal principal);

        /// <summary>
        /// Update the specified data
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="transactionMode">The mode of update (commit or rollback)</param>
        /// <param name="principal">The principal which is executing the operation</param>
        TData Update(TData data, TransactionMode transactionMode, IPrincipal principal);

        /// <summary>
        /// Obsolete the specified identified data
        /// </summary>
        [Obsolete("Use Delete(key, mode, principal, DeleteMode.Obsolete) instead", true)]
        TData Obsolete(Guid key, TransactionMode transactionMode, IPrincipal principal);

        /// <summary>
        /// Delete the specified identified data
        /// </summary>
        /// <param name="deletionMode">How the persistence service should attempt to remove data</param>
        /// <param name="key">The identifier/key of the data to be deleted</param>
        /// <param name="principal">The principal which is deleting the data</param>
        /// <param name="transactionMode">The transaction mode</param>
        /// <remarks>
        /// <para>
        /// This method will attempt to delete data according to the <paramref name="deletionMode"/> specified by the caller.
        /// </para>
        /// <list type="table">
        ///     <item>
        ///         <term>LogicalDelete</term>
        ///         <description>The perssitence layer should attempt to logically delete the record. This means that the record should not appear in queries, nor in direct retrieves unless specifically asked for</description>
        ///     </item>
        ///     <item>
        ///         <term>ObsoleteDelete</term>
        ///         <description>The persistence layer should mark the record as obsolete.</description>
        ///     </item>
        ///     <item>
        ///         <term>NullifyDelete</term>
        ///         <description>The persistence layer should mark the record as nullified (i.e. entered in error)</description>
        ///     </item>
        ///     <item>
        ///         <term>PermanentDelete</term>
        ///         <description>The persistence layer should purge the data from the database</description>
        ///     </item>
        /// </list>
        /// <para>
        /// The <paramref name="deletionMode"/> parameter is a suggestion to the persistence layer, generally the closest, most appropriate value
        /// is chosen based on:
        /// </para>
        /// <list type="bullet">
        ///     <item>Whether the <typeparamref name="TData"/> class can be logically deleted (i.e. does it carry the necessary fields to support deletion)</item>
        ///     <item>Whether there are other references to the object</item>
        ///     <item>Whether the configuration for the persistence layer permits logical deletion</item>
        /// </list>
        /// </remarks>
        TData Delete(Guid key, TransactionMode transactionMode, IPrincipal principal, DeleteMode deletionMode);

        /// <summary>
        /// Get the object with identifier <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The identifier of the object to fetch</param>
        /// <param name="principal">The security principal which is executing the retrieve</param>
        /// <param name="versionKey">The version of the oject to fetch</param>
        /// <remarks>
        /// This method will retrieve the record of type <typeparamref name="TData"/> from the database regardless of its state. If the
        /// record is logically deleted, or indicated as inactive (i.e. would not appear in a result set), this method will still
        /// retrieve the data from the database.
        /// </remarks>
        TData Get(Guid key, Guid? versionKey, IPrincipal principal);

        /// <summary>
        /// Query for <typeparamref name="TData"/> whose current version matches <paramref name="query"/>
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="principal">The principal under which the query is occurring</param>
        /// <remarks>
        /// <para>This method will query for all records of type <typeparamref name="TData"/>. By default the query will
        /// only return active records, unless a status parameter is passed, in which case records matching the
        /// requested status will be returned.</para>
        /// <para>The result of this call is an <see cref="IQueryResultSet{TData}"/>, this class supports delayed execution
        /// and yielded returns of records. This means that each call to methods on the return value may result in a
        /// query to the database.</para>
        /// </remarks>
        IQueryResultSet<TData> Query(Expression<Func<TData, bool>> query, IPrincipal principal);

        /// <summary>
        /// Query the specified data
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="orderBy">The ordering instrutions to send the query</param>
        /// <param name="totalResults">The total number of results matching the query</param>
        /// <param name="count">The count of results to include in the response set</param>
        /// <param name="offset">The offset of the first result</param>
        /// <param name="principal">The security principal under which the query is occurring</param>
        [Obsolete("Use Query(query, principal) instead", false)]
        IEnumerable<TData> Query(Expression<Func<TData, bool>> query, int offset, int? count, out int totalResults, IPrincipal principal, params ModelSort<TData>[] orderBy);

        /// <summary>
        /// Performs a fast count
        /// </summary>
        [Obsolete("Use Query(Expression).Count()", true)]
        long Count(Expression<Func<TData, bool>> query, IPrincipal authContext = null);
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
        Object Obsolete(Guid id);

        /// <summary>
        /// Gets the specified data
        /// </summary>
        Object Get(Guid id);

        /// <summary>
        /// Query based on the expression given
        /// </summary>
        [Obsolete("Use Query(Expression query)", false)]
        IEnumerable Query(Expression query, int offset, int? count, out int totalResults);

        /// <summary>
        /// Query the specified expression
        /// </summary>
        IQueryResultSet Query(Expression query);
    }
}