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
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local material persistence service
    /// </summary>
    public class LocalPatientRepository : GenericLocalRepositoryEx<Patient>
    {
        /// <summary>
        /// Create a new patient repository
        /// </summary>
        public LocalPatientRepository(IPolicyEnforcementService policyService, ILocalizationService localizationService, IDataPersistenceService<Patient> persistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, localizationService, persistenceService, privacyService)
        {
        }

        /// <inheritdoc/>
        protected override string QueryPolicy => PermissionPolicyIdentifiers.QueryClinicalData;
        /// <inheritdoc/>
        protected override string ReadPolicy => PermissionPolicyIdentifiers.ReadClinicalData;
        /// <inheritdoc/>
        protected override string WritePolicy => PermissionPolicyIdentifiers.WriteClinicalData;
        /// <inheritdoc/>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.DeleteClinicalData;
        /// <inheritdoc/>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.WriteClinicalData;
    }
}