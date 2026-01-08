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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System.Linq;
using System;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Place repository that uses local persistence
    /// </summary>
    public class LocalContainerRepository : GenericLocalMetadataRepository<Container>
    {
        private readonly IDataPersistenceService<EntityRelationship> m_entityRelationshipRepository;
        private readonly IPolicyEnforcementService m_pepService;
        private readonly ISecurityRepositoryService m_securityRepository;

        /// <summary>
        /// Privacy enforcement service
        /// </summary>
        public LocalContainerRepository(IPolicyEnforcementService policyService, ISecurityRepositoryService securityRepository, IDataPersistenceService<Container> dataPersistence, IDataPersistenceService<EntityRelationship> entityRelationshipService, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistence, privacyService)
        {
            this.m_entityRelationshipRepository = entityRelationshipService;
            this.m_pepService = policyService;
            this.m_securityRepository = securityRepository;
        }

        /// <inheritdoc/>
        public override void DemandWrite(object data) => this.DemandAlter(data);

        /// <inheritdoc/>
        /// <remarks>Skips policy checking for users that are managers </remarks>
        public override void DemandAlter(object data)
        {

            if (data is Container cont)
            {
                var containedFacilityRel = cont.Relationships?.FirstOrDefault(o => o.RelationshipTypeKey == EntityRelationshipTypeKeys.LocatedEntity) ??
                    this.m_entityRelationshipRepository.Query(o => o.TargetEntityKey == cont.Key && o.RelationshipTypeKey == EntityRelationshipTypeKeys.LocatedEntity, AuthenticationContext.SystemPrincipal).FirstOrDefault();

                if (containedFacilityRel == null)
                {
                    this.m_pepService.Demand(PermissionPolicyIdentifiers.UnrestrictedMetadata);
                }
                else
                {
                    // TODO: Allow for the classification of a facility administrative user
                    var thisUser = this.m_securityRepository.GetCdrEntity(AuthenticationContext.Current.Principal);
                    if (thisUser?.HasDirectOrParentRelationshipWith(EntityRelationshipTypeKeys.MaintainedEntity, containedFacilityRel.SourceEntityKey) != true)
                    {
                        if(!this.m_pepService.SoftDemand(PermissionPolicyIdentifiers.WriteMaterials, AuthenticationContext.Current.Principal))
                        {
                            throw new PolicyViolationException(AuthenticationContext.Current.Principal, PermissionPolicyIdentifiers.WriteMaterials, Model.Security.PolicyGrantType.Deny, ErrorMessages.NOT_A_MANAGER_ROLE);
                        }
                    }
                }
            }
            else {
                base.DemandAlter(data);
            }
        }

    }
}