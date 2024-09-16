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
 */
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Localuser entity repository
    /// </summary>
    public class LocalUserEntityRepository : GenericLocalMetadataRepository<UserEntity>
    {
        /// <summary>
        /// Privacy for a user entity
        /// </summary>
        public LocalUserEntityRepository(IPolicyEnforcementService policyService, IDataPersistenceService<UserEntity> userEntity, IPrivacyEnforcementService privacyService = null) : base(policyService, userEntity, privacyService)
        {
        }

        /// <summary>
        /// Demand write
        /// </summary>
        public override void DemandWrite(object data)
        {
            this.ValidateWritePermission(data as UserEntity);
        }

        /// <summary>
        /// Demand alter permission
        /// </summary>
        public override void DemandAlter(object data)
        {
            this.ValidateWritePermission(data as UserEntity);
        }

        /// <summary>
        /// Validate that the user has write permission
        /// </summary>
        private void ValidateWritePermission(UserEntity entity)
        {
            var user = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>()?.GetUser(AuthenticationContext.Current.Principal.Identity);
            if (user?.Key != entity.SecurityUserKey)
            {
                this.m_policyService.Demand(PermissionPolicyIdentifiers.AlterIdentity);
            }
        }

    }
}