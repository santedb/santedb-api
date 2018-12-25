﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-6-22
 */
using SanteDB.Core.Model;
using System;
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
        IEnumerable<TModel> Find(Expression<Func<TModel, bool>> query, int offset, int? count, out int totalResults);

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
    /// Repreents a repository which notifies of changes
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public interface INotifyRepositoryService<TModel> : IRepositoryService<TModel>
        where TModel : IdentifiedData
    {
        /// <summary>
        /// Fired after data was inserted 
        /// </summary>
        event EventHandler<RepositoryEventArgs<TModel>> Inserted;
        /// <summary>
        /// Fired after data was saved
        /// </summary>
        event EventHandler<RepositoryEventArgs<TModel>> Saved;
        /// <summary>
        /// Fired after data was retrieved
        /// </summary>
        event EventHandler<RepositoryEventArgs<TModel>> Retrieved;
        /// <summary>
        /// Fired after data was queried
        /// </summary>
        event EventHandler<RepositoryEventArgs<IEnumerable<TModel>>> Queried;
    }
    /// <summary>
    /// Represents a repository which can nullify an object
    /// </summary>
    public interface INullifyRepositoryService<TModel> : IRepositoryService<TModel>
        where TModel : IdentifiedData
    {
        /// <summary>
        /// Nullifies a specific instance
        /// </summary>
        TModel Nullify(Guid id);
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