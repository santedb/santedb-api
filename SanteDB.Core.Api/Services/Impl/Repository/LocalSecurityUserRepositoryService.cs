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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using SanteDB.Core.Services;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Security user repository
    /// </summary>
    public class LocalSecurityUserRepositoryService : GenericLocalSecurityRepository<SecurityUser>
    {
        /// <summary>
        /// Creates a DI security user repository
        /// </summary>
        public LocalSecurityUserRepositoryService(IPolicyEnforcementService policyService, ILocalizationService localizationService, IDataPersistenceService<SecurityUser> dataPersistenceService, IPrivacyEnforcementService privacyService = null) : base(policyService, localizationService, dataPersistenceService, privacyService)
        {
        }

        /// <inheritdoc/>
        protected override string WritePolicy => PermissionPolicyIdentifiers.CreateIdentity;
        /// <inheritdoc/>
        protected override string DeletePolicy => PermissionPolicyIdentifiers.AlterIdentity;

        /// <summary>
        /// Demand altering
        /// </summary>
        /// <param name="data"></param>
        public override void DemandAlter(object data)
        {
            var su = data as SecurityUser;
            if (!su.UserName.Equals(AuthenticationContext.Current.Principal.Identity.Name, StringComparison.OrdinalIgnoreCase))
                this.m_policyService.Demand(PermissionPolicyIdentifiers.AlterIdentity);
        }

        /// <summary>
        /// Insert the user
        /// </summary>
        public override SecurityUser Insert(SecurityUser data)
        {
            this.m_traceSource.TraceEvent(EventLevel.Verbose, "Creating user {0}", data);

            var iids = ApplicationServiceContext.Current.GetService<IIdentityProviderService>();

            // Verify password meets requirements
            if (ApplicationServiceContext.Current.GetService<IPasswordValidatorService>()?.Validate(data.Password) == false)
                throw new DetectedIssueException(new DetectedIssue(DetectedIssuePriorityType.Error, "err.password", this.m_localizationService.GetString("error.server.core.validationFail", new
                {
                    param = "Password"
                }), DetectedIssueKeys.SecurityIssue));

            // Create the identity
            var id = iids.CreateIdentity(data.UserName, data.Password, AuthenticationContext.Current.Principal);

            // Now ensure local db record exists
            var retVal = this.Find(o => o.UserName == data.UserName).FirstOrDefault();
            if (retVal == null)
            {
                throw new InvalidOperationException(this.m_localizationService.GetString("error.server.core.userCreated"));
            }
            else
            {
                // The identity provider only creates a minimal identity, let's beef it up
                retVal.Email = data.Email;
                retVal.EmailConfirmed = data.EmailConfirmed;
                retVal.InvalidLoginAttempts = data.InvalidLoginAttempts;
                retVal.LastLoginTime = data.LastLoginTime;
                retVal.Lockout = data.Lockout;
                retVal.PhoneNumber = data.PhoneNumber;
                retVal.PhoneNumberConfirmed = data.PhoneNumberConfirmed;
                retVal.SecurityHash = data.SecurityHash;
                retVal.TwoFactorEnabled = data.TwoFactorEnabled;
                retVal.UserPhoto = data.UserPhoto;
                retVal.UserClass = data.UserClass;
                base.Save(retVal);
            }

            return retVal;
        }

        /// <summary>
        /// Save the user credential
        /// </summary>
        public override SecurityUser Save(SecurityUser data)
        {
            if (!String.IsNullOrEmpty(data.Password))
            {
                ApplicationServiceContext.Current.GetService<IIdentityProviderService>().ChangePassword(data.UserName, data.Password, AuthenticationContext.Current.Principal);
            }
            return base.Save(data);
        }
    }
}