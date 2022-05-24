﻿/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-27
 */

using SanteDB.Core;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using SanteDB.Core.Services;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local security application repository
    /// </summary>
    public class LocalSecurityApplicationRepository : GenericLocalSecurityRepository<SecurityApplication>
    {
        // Id provider
        private IApplicationIdentityProviderService m_identityProvider;

        /// <summary>
        /// DI constructor
        /// </summary>
        public LocalSecurityApplicationRepository(IApplicationIdentityProviderService identityProvider, IPolicyEnforcementService policyService, ILocalizationService localizationService, IDataPersistenceService<SecurityApplication> persistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, localizationService, persistenceService, privacyService)
        {
            this.m_identityProvider = identityProvider;
        }

        /// <inheritdoc/>
        protected override string WritePolicy => PermissionPolicyIdentifiers.CreateApplication;
        /// <inheritdoc/>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.CreateApplication;
        /// <inheritdoc/>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.CreateApplication;

        /// <summary>
        /// Insert the device
        /// </summary>
        public override SecurityApplication Insert(SecurityApplication data)
        {
            // Create the identity
            var id = this.m_identityProvider.CreateIdentity(data.Name, data.ApplicationSecret, AuthenticationContext.Current.Principal);

            // Now ensure local db record exists
            Guid sid = Guid.Empty;
            if (id is IClaimsIdentity cIdentity)
            {
                var sidClaim = cIdentity.FindFirst(SanteDBClaimTypes.Sid);
                Guid.TryParse(sidClaim.Value, out sid);
            }
            else
            {
                sid = this.m_identityProvider.GetSid(id.Name);
            }

            data.Key = sid;
            data.ApplicationSecret = null;
            return base.Save(data);
        }

        /// <summary>
        /// Save the security device
        /// </summary>
        public override SecurityApplication Save(SecurityApplication data)
        {
            if (!String.IsNullOrEmpty(data.ApplicationSecret))
            {
                this.m_identityProvider.ChangeSecret(data.Name, data.ApplicationSecret, AuthenticationContext.Current.Principal);
                data.ApplicationSecret = null;
            }
            return base.Save(data);
        }
    }
}