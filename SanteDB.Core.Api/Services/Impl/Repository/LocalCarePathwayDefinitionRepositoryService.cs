﻿/*
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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local care pathway definition
    /// </summary>
    public class LocalCarePathwayDefinitionRepositoryService :
        GenericLocalMetadataRepository<CarePathwayDefinition>,
        ICarePathwayDefinitionRepositoryService
    {
        /// <inheritdoc/>
        public LocalCarePathwayDefinitionRepositoryService(IPolicyEnforcementService policyService, IDataPersistenceService<CarePathwayDefinition> dataPersistence, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistence, privacyService)
        {
        }

        /// <inheritdoc/>
        public CarePathwayDefinition GetCarepathDefinition(string mnemonic) => this.Find(o => o.Mnemonic == mnemonic && o.ObsoletionTime == null).FirstOrDefault();

    }
}
