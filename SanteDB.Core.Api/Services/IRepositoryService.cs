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
using SanteDB.Core.Model.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Represents event args fired at the repository level
    /// </summary>
    public class RepositoryEventArgs<TModel> : EventArgs
    {
        /// <summary>
        /// Creates a new instance of repository data event args
        /// </summary>
        public RepositoryEventArgs(TModel data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Gets the data elements related to the event
        /// </summary>
        public TModel Data { get; private set; }

    }

    /// <summary>
    /// Repository service
    /// </summary>
    public interface IRepositoryService : IServiceImplementation
    {

        /// <summary>
        /// Get the specified object
        /// </summary>
        IdentifiedData Get(Guid key);

        /// <summary>
        /// Find the specified object
        /// </summary>
        IEnumerable<IdentifiedData> Find(Expression query);

        /// <summary>
        /// Find the specified object
        /// </summary>
        IEnumerable<IdentifiedData> Find(Expression query, int offset, int? count, out int totalResults);

        /// <summary>
        /// Inserts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>TModel.</returns>
        IdentifiedData Insert(object data);

        /// <summary>
        /// Saves the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Returns the model.</returns>
        IdentifiedData Save(object data);

        /// <summary>
        /// Obsoletes the specified data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns the model.</returns>
        IdentifiedData Obsolete(Guid key);
    }

    /// <summary>
    /// Represents a repository service base
    /// </summary>
    public interface IRepositoryService<TModel> : IServiceImplementation where TModel : IdentifiedData
    {

        /// <summary>
        /// Gets the specified model.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns the model.</returns>
        TModel Get(Guid key);

        /// <summary>
        /// Gets the specified model.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="versionKey">The key of the version.</param>
        /// <returns>Returns the model.</returns>
        TModel Get(Guid key, Guid versionKey);

        /// <summary>
        /// Finds the specified data.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Returns a list of identified data.</returns>
        IEnumerable<TModel> Find(Expression<Func<TModel, bool>> query);

        /// <summary>
        /// Finds the specified data.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <param name="totalResults">The total results.</param>
        /// <returns>Returns a list of identified data.</returns>
        IEnumerable<TModel> Find(Expression<Func<TModel, bool>> query, int offset, int? count, out int totalResults, params ModelSort<TModel>[] orderBy);

        /// <summary>
        /// Inserts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>TModel.</returns>
        TModel Insert(TModel data);

        /// <summary>
        /// Saves the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Returns the model.</returns>
        TModel Save(TModel data);

        /// <summary>
        /// Obsoletes the specified data.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns the model.</returns>
        TModel Obsolete(Guid key);
    }

    /// <summary>
    /// Represents a repository service wrapping an extended persistence service
    /// </summary>
    public interface IRepositoryServiceEx<TModel> : IRepositoryService<TModel>
        where TModel : IdentifiedData
    {

        /// <summary>
        /// Touch the specified object
        /// </summary>
        void Touch(Guid key);

        /// <summary>
        /// Nullifies a specific instance
        /// </summary>
        TModel Nullify(Guid id);
    }

    /// <summary>
    /// Repreents a repository which notifies of changes
    /// </summary>
    public interface INotifyRepositoryService<TModel> : IRepositoryService<TModel>
        where TModel : IdentifiedData
    {
        /// <summary>
        /// Data is inserting
        /// </summary>
        event EventHandler<DataPersistingEventArgs<TModel>> Inserting;
        /// <summary>
        /// Fired after data was inserted 
        /// </summary>
        event EventHandler<DataPersistedEventArgs<TModel>> Inserted;
        /// <summary>
        /// Fired before saving
        /// </summary>
        event EventHandler<DataPersistingEventArgs<TModel>> Saving;
        /// <summary>
        /// Fired after data was saved
        /// </summary>
        event EventHandler<DataPersistedEventArgs<TModel>> Saved;
        /// <summary>
        /// Fired before obsoleting
        /// </summary>
        event EventHandler<DataPersistingEventArgs<TModel>> Obsoleting;
        /// <summary>
        /// Fired after data was obsoleted
        /// </summary>
        event EventHandler<DataPersistedEventArgs<TModel>> Obsoleted;
        /// <summary>
        /// Retrieving the data
        /// </summary>
        event EventHandler<DataRetrievingEventArgs<TModel>> Retrieving;
        /// <summary>
        /// Fired after data was retrieved
        /// </summary>
        event EventHandler<DataRetrievedEventArgs<TModel>> Retrieved;
        /// <summary>
        /// Fired after data was queried
        /// </summary>
        event EventHandler<QueryRequestEventArgs<TModel>> Querying;
        /// <summary>
        /// Fired after data was queried
        /// </summary>
        event EventHandler<QueryResultEventArgs<TModel>> Queried;
    }

    /// <summary>
    /// Represents a repository that can cancel an act
    /// </summary>
    public interface ICancelRepositoryService<TModel> : IRepositoryService<TModel>
        where TModel : IdentifiedData
    {

        /// <summary>
        /// Cancels the specified object
        /// </summary>
        TModel Cancel(Guid id);
    }

    /// <summary>
    /// Represents a repository service that applies permission
    /// </summary>
    public interface ISecuredRepositoryService
    {
        /// <summary>
        /// Demand write permission
        /// </summary>
        void DemandWrite(object data);

        /// <summary>
        /// Demand read permission
        /// </summary>
        void DemandRead(Guid key);

        /// <summary>
        /// Demand delete permission
        /// </summary>
        void DemandDelete(Guid key);

        /// <summary>
        /// Demand alter permission
        /// </summary>
        void DemandAlter(object data);

        /// <summary>
        /// Demand query permission
        /// </summary>
        void DemandQuery();
    }
}