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
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Alter policies
    /// </summary>
    public class LocalSecurityPolicyRepository : GenericLocalSecurityRepository<SecurityPolicy>
    {
        /// <summary>
        /// DI constructor
        /// </summary>
        public LocalSecurityPolicyRepository(IPolicyEnforcementService policyService, ILocalizationService localizationService, IDataPersistenceService<SecurityPolicy> dataPersistence, IPrivacyEnforcementService privacyService = null) : base(policyService, localizationService, dataPersistence, privacyService)
        {
        }

        /// <inheritdoc/>
        protected override string WritePolicy => PermissionPolicyIdentifiers.AlterPolicy;
        /// <inheritdoc/>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.AlterPolicy;
        /// <inheritdoc/>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.AlterPolicy;
    }
}