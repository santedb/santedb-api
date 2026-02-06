/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Principal;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a data persisetence service that can handle bulk operations
    /// </summary>
    [System.ComponentModel.Description("Bulk Data Access Provider")]
    public interface IBulkDataPersistenceService
    {

        /// <summary>
        /// Obsolete the specified data
        /// </summary>
        void Obsolete(TransactionMode transactionMode, IPrincipal principal, params Guid[] keysToObsolete);

        /// <summary>
        /// Purge the specified data (erase it)
        /// </summary>
        void Purge(TransactionMode transactionMode, IPrincipal principal, params Guid[] keysToPurge);

        /// <summary>
        /// Purge specified data
        /// </summary>
        void Purge(TransactionMode transactionMode, IPrincipal principal, Expression query);

        /// <summary>
        /// Query only for keys based on the expression (do not load objects from database)
        /// </summary>
        IEnumerable<Guid> QueryKeys(Expression query, int offset, int? count, out int totalResults);

    }
}
