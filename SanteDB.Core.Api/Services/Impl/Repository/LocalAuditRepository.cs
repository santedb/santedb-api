/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Represents an audit repository which stores and queries audit data.
    /// </summary>
    [ServiceProvider("Default Audit Repository")]
    public class LocalAuditRepository : IRepositoryService<AuditEventData>
    {
        // Localization Service
        private readonly ILocalizationService m_localizationService;

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(LocalAuditRepository));

        // Persistence service
        private readonly IDataPersistenceService<AuditEventData> m_persistenceService;
        private readonly IPolicyEnforcementService m_pepService;

        /// <summary>
        /// Construct instance of LocalAuditRepository
        /// </summary>
        public LocalAuditRepository(ILocalizationService localizationService, IPolicyEnforcementService pepService, IDataPersistenceService<AuditEventData> persistenceService = null)
        {
            this.m_localizationService = localizationService;
            this.m_persistenceService = persistenceService;
            this.m_pepService = pepService;
        }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Default Audit Repository";

        /// <summary>
        /// Find the specified data
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public IQueryResultSet<AuditEventData> Find(Expression<Func<AuditEventData, bool>> query)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.AccessAuditLog);
            return this.m_persistenceService?.Query(query, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Find with query controls
        /// </summary>
        [Obsolete("Use Find(Expression<Func<AuditEventData, bool>>)", true)]
        public IEnumerable<AuditEventData> Find(Expression<Func<AuditEventData, bool>> query, int offset, int? count, out int totalResults, params ModelSort<AuditEventData>[] orderBy)
        {
            var results = this.Find(query);

            if (results is IOrderableQueryResultSet<AuditEventData> orderable)
            {
                foreach (var itm in orderBy)
                {
                    switch (itm.SortOrder)
                    {
                        case SanteDB.Core.Model.Map.SortOrderType.OrderBy:
                            results = orderable.OrderBy(itm.SortProperty);
                            break;

                        case SanteDB.Core.Model.Map.SortOrderType.OrderByDescending:
                            results = orderable.OrderByDescending(itm.SortProperty);
                            break;
                    }
                }
            }

            totalResults = results.Count();
            return results.Skip(offset).Take(count ?? 100);
        }

        /// <summary>
        /// Gets the specified object
        /// </summary>
        public AuditEventData Get(Guid key)
        {
            return this.Get(key, Guid.Empty);
        }

        /// <summary>
        /// Gets the specified correlation key
        /// </summary>
        public AuditEventData Get(object correlationKey)
        {
            return this.Get((Guid)correlationKey, Guid.Empty);
        }

        /// <summary>
        /// Get the specified audit by key
        /// </summary>
        public AuditEventData Get(Guid key, Guid versionKey)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.AccessAuditLog);

            return this.m_persistenceService?.Get(key, versionKey, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Insert the specified data
        /// </summary>
        public AuditEventData Insert(AuditEventData audit)
        {
            var preArgs = new DataPersistingEventArgs<AuditEventData>(audit, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            var retVal = this.m_persistenceService?.Insert(audit, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            return retVal;
        }

        /// <summary>
        /// Obsolete the specified data
        /// </summary>
        public AuditEventData Delete(Guid key)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.AccessAuditLog);
            var retVal = this.m_persistenceService?.Delete(key, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            return retVal;
        }

        /// <summary>
        /// Save (create or update) the specified object
        /// </summary>
        public AuditEventData Save(AuditEventData data)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.AccessAuditLog);

            var preArgs = new DataPersistingEventArgs<AuditEventData>(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);

            var existing = this.m_persistenceService.Get(data.Key.Value, null, AuthenticationContext.Current.Principal);
            AuditEventData retVal = null;
            if (existing == null)
            {
                retVal = this.m_persistenceService?.Update(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            }
            else
            {
                retVal = this.m_persistenceService?.Insert(data, TransactionMode.Commit, AuthenticationContext.Current.Principal);
            }
            return retVal;
        }
    }
}