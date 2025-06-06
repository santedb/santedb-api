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
 * Date: 2023-6-21
 */
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Security.Services;
using System;
using System.Linq;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local extension types
    /// </summary>
    public class LocalExtensionTypeRepository : GenericLocalMetadataRepository<ExtensionType>, IExtensionTypeRepository
    {
        /// <summary>
        /// Local extension type repository
        /// </summary>
        public LocalExtensionTypeRepository(IPolicyEnforcementService policyService, IDataPersistenceService<ExtensionType> dataPersistence, IPrivacyEnforcementService privacyService = null) : base(policyService, dataPersistence, privacyService)
        {
        }

        /// <summary>
        /// Get the xtension type by uri
        /// </summary>
        public ExtensionType Get(Uri uri)
        {
            var name = uri.ToString();
            int t;
            return base.Find(o => o.Uri == name, 0, 1, out t).FirstOrDefault();
        }
    }
}