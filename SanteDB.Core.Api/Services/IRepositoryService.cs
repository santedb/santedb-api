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
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Represents a repository service
    /// </summary>
    /// <remarks>
    /// <para>In the <see href="https://help.santesuite.org/santedb/software-architecture#repository-services">SanteDB Software Architecture</see> the repository service 
    /// layer is the layer responsible for coordinating business rules, privacy, auditing, and other activities from the messaging or other 
    /// services in the SanteDB iCDR or dCDR.</para>
    /// <para>Repository services should be the primary method of interacting with the SanteDB server infrastructure, as it indicates a user, application or
    /// device process is not intending to modify underlying persistence data directly (as would be the case for a system process), rather it wishes SanteDB
    /// to execute all validation and rules as normal.</para>
    /// </remarks>
    [Description("Repository Service")]
    public interface IRepositoryService<TModel> : IServiceImplementation where TModel : IdentifiedData
    {

        /// <summary>
        /// Gets the specified model data
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Returns the model.</returns>
        TModel Get(Guid key);

        /// <summary>
        /// Gets the specified model data at the specified version
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="versionKey">The key of the version.</param>
        /// <returns>Returns the model.</returns>
        TModel Get(Guid key, Guid versionKey);

        /// <summary>
        /// Finds the specified data where the current version matches the query provided
        /// </summary>
        /// <param name="query">The query to be executed</param>
        /// <returns>Returns a list of <typeparamref name="TModel"/> matching the <paramref name="query"/>.</returns>
        IEnumerable<TModel> Find(Expression<Func<TModel, bool>> query);

        /// <summary>
        /// Finds the specified data with the specified control parameters
        /// </summary>
        /// <param name="query">The query to be executed</param>
        /// <param name="offset">The offset of the first record</param>
        /// <param name="count">The count of records to be returned</param>
        /// <param name="totalResults">The total results matching <paramref name="query"/></param>
        /// <param name="orderBy">The ordering instructions that are to be appended to the query</param>
        /// <returns>Returns a list of matching <typeparamref name="TModel"/> instances</returns>
        IEnumerable<TModel> Find(Expression<Func<TModel, bool>> query, int offset, int? count, out int totalResults, params ModelSort<TModel>[] orderBy);

        /// <summary>
        /// Inserts the specified model information
        /// </summary>
        /// <param name="data">The data to be inserted</param>
        /// <returns>The inserted data (including any generated properties like ID, timestamps, etc.)</returns>
        TModel Insert(TModel data);

        /// <summary>
        /// Inserts or updates the specified data
        /// </summary>
        /// <param name="data">The data to be saved to/from the data persistence layer</param>
        /// <returns>The current version of <paramref name="data"/></returns>
        TModel Save(TModel data);

        /// <summary>
        /// Obsoletes the specified object
        /// </summary>
        /// <param name="key">The key of the object to be obsoleted</param>
        /// <returns>The obsoleted data (including obsoletion time and provenance data)</returns>
        TModel Obsolete(Guid key);
    }

    /// <summary>
    /// Represents a <see cref="IRepositoryService"/> service which has extended functionality
    /// </summary>
    [Description("Repository Service with Extended Functions")]
    public interface IRepositoryServiceEx<TModel> : IRepositoryService<TModel>
        where TModel : IdentifiedData
    {

        /// <summary>
        /// Touch the specified object by updating its last modified time (forcing a re-synchronization) however 
        /// not modifying the data in the object
        /// </summary>
        /// <param name="key">The key of the <typeparamref name="TModel"/> to be touched</param>
        void Touch(Guid key);

        /// <summary>
        /// Nullifies the specified object (mark as "Entered in Error")
        /// </summary>
        /// <param name="id">The identifier of the <typeparamref name="TModel"/> to be nullified</param>
        /// <returns>The nullified object</returns>
        TModel Nullify(Guid id);
    }

    /// <summary>
    /// A <see cref="IRepositoryService"/> which can notify other classes of changes to data
    /// </summary>
    [Description("Repository Service with Notification Support")]
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
    /// Represents a repository that can cancel an <see cref="Act"/> that is in progress
    /// </summary>
    [Description("Repository Service with Cancellation Support")]
    public interface ICancelRepositoryService<TModel> : IRepositoryService<TModel>
        where TModel : IdentifiedData
    {

        /// <summary>
        /// Cancels the specified <see cref="Act"/>
        /// </summary>
        /// <param name="id">The identifier of the act to be cancelled</param>
        /// <returns>The cancelled act</returns>
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