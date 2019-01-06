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
 * Date: 2018-11-26
 */
using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Principal;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a data persistence provider that can store and continue queries
    /// </summary>
    public interface IStoredQueryDataPersistenceService<TEntity> : IDataPersistenceService<TEntity>
        where TEntity: IdentifiedData
    {
        /// <summary>
        /// Queries or continues a query 
        /// </summary>
        IEnumerable<TEntity> Query(Expression<Func<TEntity, bool>> query, Guid queryId, int offset, int? count, out int totalCount, IPrincipal overrideAuthContext, params ModelSort<TEntity>[] orderBy);
    }
}
