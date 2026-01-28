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
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Linq.Expressions;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local entity repository 
    /// </summary>
    public class LocalEntityRepository : GenericLocalRepositoryEx<Entity>
    {
        /// <summary>
        /// Local entity repository
        /// </summary>
        public LocalEntityRepository(IPolicyEnforcementService policyService, IDataPersistenceService<Entity> dataPersistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistenceService, privacyService)
        {
        }

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string ReadPolicy => PermissionPolicyIdentifiers.ReadMetadata;

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string WritePolicy => PermissionPolicyIdentifiers.LoginAsService;

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.UnrestrictedMetadata;

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.LoginAsService;

        /// <inheritdoc/>
        /// <remarks>This is handled based on the return type</remarks>
        protected override string QueryPolicy => PermissionPolicyIdentifiers.ReadMetadata;

        /// <inheritdoc/>
        public override Entity Delete(Guid key)
        {
            var data = this.Get(key);
            var repositoryType = typeof(IRepositoryService<>).MakeGenericType(data.GetType());
            var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as ISecuredRepositoryService;

            if (otherRepository == this)
            {
                return base.Delete(key);
            }
            else
            {
                otherRepository.DemandDelete(key);
                return otherRepository.Delete(key) as Entity; // Let the other repository decide what to do 
            }
        }

        /// <inheritdoc/>
        public override Entity Save(Entity data)
        {
            if (data.GetType() != typeof(Entity))
            {
                var repositoryType = typeof(IRepositoryService<>).MakeGenericType(data.GetType());
                var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as IRepositoryService;
                return otherRepository.Save(data) as Entity; // Let the other repository decide what to do 
            }
            else
            {
                return base.Save(data);
            }
        }

        /// <inheritdoc/>
        public override Entity Insert(Entity data)
        {
            if (data.GetType() != typeof(Entity))
            {
                var repositoryType = typeof(IRepositoryService<>).MakeGenericType(data.GetType());
                var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as IRepositoryService;
                return otherRepository.Insert(data) as Entity; // Let the other repository decide what to do 
            }
            else
            {
                return base.Insert(data);
            }
        }

        /// <inheritdoc/>
        public override Entity Get(Guid key, Guid versionKey)
        {

            var data = base.Get(key, versionKey);
            if (data != null)
            {
                // Allow the other to demand its permissions
                var repositoryType = typeof(IRepositoryService<>).MakeGenericType(data.GetType());
                var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as ISecuredRepositoryService;
                otherRepository.DemandRead(key);
                return data;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override IQueryResultSet<Entity> Find(Expression<Func<Entity, bool>> query)
        {
            return new NestedQueryResultSet<Entity>(base.Find(query), (o) =>
            {
                var repositoryType = typeof(IRepositoryService<>).MakeGenericType(o.GetType());
                var otherRepository = ApplicationServiceContext.Current.GetService(repositoryType) as ISecuredRepositoryService;
                otherRepository.DemandQuery();
                return o;
            });
        }

    }
}
