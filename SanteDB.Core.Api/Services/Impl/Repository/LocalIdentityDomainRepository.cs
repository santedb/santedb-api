/*
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
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Linq;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Represents a repository service for managing assigning authorities.
    /// </summary>
    public class LocalIdentityDomainRepository :
        GenericLocalMetadataRepository<IdentityDomain>,
        IIdentityDomainRepositoryService
    {
        /// <summary>
        /// Local AA
        /// </summary>
        public LocalIdentityDomainRepository(IPolicyEnforcementService policyService, ILocalizationService localizationService, IDataPersistenceService<IdentityDomain> dataPersistence, IPrivacyEnforcementService privacyService = null) : base(policyService, localizationService, dataPersistence, privacyService)
        {
        }

        /// <summary>
        /// Get the specified assigning authority
        /// </summary>
        public IdentityDomain Get(Uri assigningAutUri)
        {

            if (assigningAutUri.Scheme == "urn" && assigningAutUri.LocalPath.StartsWith("oid:"))
            {
                var aaOid = assigningAutUri.LocalPath.Substring(4);
                return base.Find(o => o.Oid == aaOid).FirstOrDefault();
            }

            return base.Find(o => o.Url == assigningAutUri.OriginalString).FirstOrDefault();
        }

        /// <summary>
        /// Get assigning authority
        /// </summary>
        public IdentityDomain Get(string domain)
        {
            int tr = 0;
            return base.Find(o => o.DomainName == domain, 0, 1, out tr).FirstOrDefault();
        }
    }
}