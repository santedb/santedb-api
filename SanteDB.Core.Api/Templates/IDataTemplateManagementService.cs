/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-12-12
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using SanteDB.Core.Templates.Definition;

namespace SanteDB.Core.Templates
{
    /// <summary>
    /// Represents a service which manages user defined templates
    /// </summary>
    /// <remarks>
    /// User defined templates are a combination of: 
    /// 1. A template definition (model template)
    /// 2. One or more views for the template representing:
    ///     - Detail View
    ///     - Summary View
    ///     - Back Entry
    ///     - Entry
    /// </remarks>
    public interface IDataTemplateManagementService : IServiceImplementation, IRepositoryService<DataTemplateDefinition>, IRepositoryService
    {

        /// <summary>
        /// Find a template definition based on the query provided
        /// </summary>
        /// <param name="query">The query that is to be executed</param>
        /// <returns>The result set representing the matching data template definition</returns>
        IQueryResultSet<DataTemplateDefinition> Find(Expression<Func<DataTemplateDefinition, bool>> query);

        /// <summary>
        /// Gets the specified data template by the UUID key of the template definition
        /// </summary>
        /// <param name="key">The key of the template definition</param>
        /// <returns>The defined template</returns>
        DataTemplateDefinition Get(Guid key);

        /// <summary>
        /// Adds or updates the specified <paramref name="definition"/> into the template manager
        /// </summary>
        /// <param name="definition">The template definition which is to be updated or added</param>
        /// <returns>The added or updated template definition</returns>
        DataTemplateDefinition AddOrUpdate(DataTemplateDefinition definition);

        /// <summary>
        /// Removes the specified template definition
        /// </summary>
        /// <param name="key">The key of the definition that is to be deleted.</param>
        /// <returns>The deleted or removed template definition</returns>
        DataTemplateDefinition Remove(Guid key);

    }
}
