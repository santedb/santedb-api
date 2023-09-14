/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Model.Query;
using SanteDB.Core.Cdss;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Default protocol repository services
    /// </summary>
    public class LocalProtocolRepositoryService : GenericLocalRepository<Model.Acts.Protocol>
    {
        /// <inheritdoc/>
        public LocalProtocolRepositoryService(IPolicyEnforcementService policyService, IDataPersistenceService<Model.Acts.Protocol> dataPersistence, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistence, privacyService)
        {
        }

        /// <inheritdoc/>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.DeleteClinicalProtocolConfigurationDefinition;
        /// <inheritdoc/>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition;
        /// <inheritdoc/>
        protected override string WritePolicy => PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition;
        /// <inheritdoc/>
        protected override string QueryPolicy => PermissionPolicyIdentifiers.ReadMetadata;
        /// <inheritdoc/>
        protected override string ReadPolicy => PermissionPolicyIdentifiers.ReadMetadata;


    }
}
