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
 * Date: 2023-6-21
 */
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security.Services;
using SharpCompress;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local batch repository service
    /// </summary>
    public class LocalBatchRepository :
        GenericLocalRepository<Bundle>
    {
        /// <summary>
        /// Creates a new batch repository
        /// </summary>
        public LocalBatchRepository(IPolicyEnforcementService policyService, IDataPersistenceService<Bundle> dataPersistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistenceService, privacyService)
        {
        }

        /// <summary>
        /// Find the specified bundle (Not supported)
        /// </summary>
        public override IQueryResultSet<Bundle> Find(Expression<Func<Bundle, bool>> query)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Get the specified bundle (not supported)
        /// </summary>
        public override Bundle Get(Guid key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Get the specified bundle (not supported)
        /// </summary>
        public override Bundle Get(Guid key, Guid versionKey)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Insert the bundle
        /// </summary>
        public override Bundle Insert(Bundle data)
        {
            this.ValidateDemands(data);
            return base.Insert(data);
        }

        private void ValidateDemands(Bundle data)
        {
            // We need permission to insert all of the objects
            foreach (var itm in data.Item.Where(i => i.BatchOperation != Model.DataTypes.BatchOperationType.Ignore))
            {
                var irst = typeof(IRepositoryService<>).MakeGenericType(itm is IdentifiedDataReference idr ? idr.ReferencedType : itm.GetType());
                var irsi = ApplicationServiceContext.Current.GetService(irst);
                if (irsi is ISecuredRepositoryService isrs && !(itm is IdentifiedDataReference))
                {
                    switch(itm.BatchOperation)
                    {
                        case BatchOperationType.Auto:
                        case BatchOperationType.Insert:
                            isrs.DemandWrite(itm);
                            break;
                        case BatchOperationType.InsertOrUpdate:
                        case BatchOperationType.Update:
                            isrs.DemandAlter(itm);
                            break;
                        case BatchOperationType.Delete:
                        case BatchOperationType.DeletePreserveContained:
                            isrs.DemandDelete(itm.Key.Value);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Obsoleting bundles are not supported
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override Bundle Delete(Guid key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Save the specified bundle
        /// </summary>
        public override Bundle Save(Bundle data)
        {
            this.ValidateDemands(data);
            return base.Save(data);
        }
    }
}