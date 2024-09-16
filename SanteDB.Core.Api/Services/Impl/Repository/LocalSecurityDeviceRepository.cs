﻿/*
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
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Local security device repository
    /// </summary>
    public class LocalSecurityDeviceRepository : GenericLocalSecurityRepository<SecurityDevice>
    {
        // ID Provider
        private IDeviceIdentityProviderService m_identityProvider;

        /// <summary>
        /// DI constructor
        /// </summary>
        public LocalSecurityDeviceRepository(IDeviceIdentityProviderService identityProvider, IPolicyEnforcementService policyService, IDataPersistenceService<SecurityDevice> persistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, persistenceService, privacyService)
        {
            this.m_identityProvider = identityProvider;
        }

        /// <inheritdoc/>
        protected override string WritePolicy => PermissionPolicyIdentifiers.CreateDevice;
        /// <inheritdoc/>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.CreateDevice;
        /// <inheritdoc/>
        protected override string AlterPolicy => PermissionPolicyIdentifiers.CreateDevice;

        /// <summary>
        /// Insert the device
        /// </summary>
        public override SecurityDevice Insert(SecurityDevice data)
        {
            // Create the identity
            var id = this.m_identityProvider.CreateIdentity(data.Name, data.DeviceSecret, AuthenticationContext.Current.Principal, data.Key);

            // Now ensure local db record exists
            Guid sid = Guid.Empty;
            if (id is IClaimsIdentity cIdentity)
            {
                var sidClaim = cIdentity.FindFirst(SanteDBClaimTypes.SecurityId);
                Guid.TryParse(sidClaim.Value, out sid);
            }
            else
            {
                sid = this.m_identityProvider.GetSid(id.Name);
            }

            data.Key = sid;
            data.DeviceSecret = null;
            return base.Save(data);
        }

        /// <summary>
        /// Save the security device
        /// </summary>
        public override SecurityDevice Save(SecurityDevice data)
        {
            if (!String.IsNullOrEmpty(data.DeviceSecret))
            {
                this.m_identityProvider.ChangeSecret(data.Name, data.DeviceSecret, AuthenticationContext.Current.Principal);
                data.DeviceSecret = null;
            }
            return base.Save(data);
        }
    }
}