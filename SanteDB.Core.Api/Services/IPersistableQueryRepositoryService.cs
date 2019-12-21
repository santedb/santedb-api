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
 * User: Justin Fyfe
 * Date: 2019-8-8
 */
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SanteDB.Core.Model.Query;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Persistable query provider is an extensable interface which can perform a query with state
    /// </summary>
    public interface IPersistableQueryRepositoryService<TEntity> : IRepositoryService<TEntity>
        where TEntity : IdentifiedData
    {
        /// <summary>
        /// Performs a query which
        /// </summary>
        /// <typeparam name="TEntity">The underlying entity type which is being queried</typeparam>
        /// <param name="query">The query to be executed</param>
        /// <param name="offset">The offset</param>
        /// <param name="count">The number of results</param>
        /// <param name="totalResults">The total results in the query</param>
        /// <param name="queryId">The unique identifier for the query</param>
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> query, int offset, int? count, out int totalResults, Guid queryId, params ModelSort<TEntity>[] orderBy);

    }
}