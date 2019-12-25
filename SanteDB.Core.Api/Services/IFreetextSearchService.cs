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
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Free text search service
    /// </summary>
    public interface IFreetextSearchService : IServiceImplementation
    {

        /// <summary>
        /// Performs a full index scan
        /// </summary>
        bool Index();

        /// <summary>
        /// Performs a freetext search 
        /// </summary>
        IEnumerable<TEntity> Search<TEntity>(String term, int offset, int? count, out int totalResults) where TEntity : IdentifiedData;
        
        /// <summary>
        /// Search based on tokens
        /// </summary>
        IEnumerable<TEntity> Search<TEntity>(String[] term, int offset, int? count, out int totalResults) where TEntity : IdentifiedData;

    }
}
