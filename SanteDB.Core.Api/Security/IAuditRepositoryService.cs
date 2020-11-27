/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Auditing;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a service which can persist and retrieve audits
    /// </summary>
    public interface IAuditRepositoryService : IServiceImplementation
    {

        /// <summary>
        /// Insert an audit into the repository
        /// </summary>
        /// <param name="audit">The audit to be inserted</param>
        AuditData Insert(AuditData audit);

        /// <summary>
        /// Find an audit from the audit repository
        /// </summary>
        /// <param name="query">The query of audits to find</param>
        /// <returns>The located audits</returns>
        IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query);

        /// <summary>
        /// Get the specified audit
        /// </summary>
        AuditData Get(Object correlationKey);

        /// <summary>
        /// Find an audit from the repository with the specified query controls
        /// </summary>
        /// <param name="query">Query to match</param>
        /// <param name="offset">Offset within the result set</param>
        /// <param name="count">Count of results in the current call </param>
        /// <param name="totalResults">Total results matching query</param>
        /// <param name="orderBy">Identifies the model sorting options for the query</param>
        /// <returns>The located audits trimmed for offset and count</returns>
        IEnumerable<AuditData> Find(Expression<Func<AuditData, bool>> query, int offset, int? count, out int totalResults, params ModelSort<AuditData>[] orderBy);
        
    }
}
