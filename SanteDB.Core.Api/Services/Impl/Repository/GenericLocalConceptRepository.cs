﻿/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-9-7
 */
using SanteDB.Core.Model;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Generic local concept repository with sufficient permissions
    /// </summary>
    public class GenericLocalConceptRepository<TModel> : GenericLocalMetadataRepository<TModel>
        where TModel : IdentifiedData
    {
        /// <summary>
        /// Creates a new generic local concept repository
        /// </summary>
        public GenericLocalConceptRepository(IPolicyEnforcementService policyService, IDataPersistenceService<TModel> persistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, persistenceService, privacyService)
        {
        }

        /// <summary>
        /// The query policy for concepts
        /// </summary>
        protected override string QueryPolicy => PermissionPolicyIdentifiers.ReadMetadata;

        /// <summary>
        /// The read policy for concepts
        /// </summary>
        protected override string ReadPolicy => PermissionPolicyIdentifiers.ReadMetadata;

        /// <summary>
        /// The write policy for concepts
        /// </summary>
        protected override string WritePolicy => PermissionPolicyIdentifiers.AdministerConceptDictionary;

        /// <summary>
        /// The delete policy for concepts
        /// </summary>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.AdministerConceptDictionary;

        /// <summary>
        /// The alter policy for concepts
        /// </summary>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.AdministerConceptDictionary;
    }
}