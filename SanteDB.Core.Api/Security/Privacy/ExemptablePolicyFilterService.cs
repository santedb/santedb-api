/*
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
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System.Security.Principal;

namespace SanteDB.Core.Security.Privacy
{
    /// <summary>
    /// A data privacy filter service which supports exemption based on configuration
    /// </summary>
    /// <remarks>
    /// <para>This class is an extension of the <see cref="DataPolicyFilterService"/> which adds support for exempting certain types
    /// of principals from the enforcement action. This is useful for scenarios where, for example, a <see cref="IDeviceIdentity"/>
    /// may be a node that is synchronizing data.</para>
    /// </remarks>
    public class ExemptablePolicyFilterService : DataPolicyFilterService
    {
        // Security configuration
        private SanteDB.Core.Security.Configuration.SecurityConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<SanteDB.Core.Security.Configuration.SecurityConfigurationSection>();

        /// <summary>
        /// Creates a new instance with DI
        /// </summary>
        public ExemptablePolicyFilterService(IConfigurationManager configManager, IPasswordHashingService passwordService, IPolicyDecisionService pdpService, IPolicyInformationService pipService, IAdhocCacheService adhocCache = null)
            : base(configManager, passwordService, pdpService, pipService, adhocCache)
        {
        }

        /// <summary>
        /// Handle post query event
        /// </summary>
        public override IQueryResultSet<TData> Apply<TData>(IQueryResultSet<TData> results, IPrincipal principal)
        {
            // If the current authentication context is a device (not a user) then we should allow the data to flow to the device
            switch (this.m_configuration.PepExemptionPolicy)
            {
                case PolicyEnforcementExemptionPolicy.AllExempt:
                    return results;

                case PolicyEnforcementExemptionPolicy.DevicePrincipalsExempt:
                    if (principal.Identity is IDeviceIdentity || principal.Identity is IApplicationIdentity)
                    {
                        return results;
                    }

                    break;

                case PolicyEnforcementExemptionPolicy.UserPrincipalsExempt:
                    if (!(principal.Identity is IDeviceIdentity || principal.Identity is IApplicationIdentity))
                    {
                        return results;
                    }

                    break;
            }
            return base.Apply(results, principal);
        }

        /// <summary>
        /// Handle post query event
        /// </summary>
        ///
        public override TData Apply<TData>(TData result, IPrincipal principal)
        {
            if (result == null) // no result
            {
                return null;
            }

            // If the current authentication context is a device (not a user) then we should allow the data to flow to the device
            switch (this.m_configuration.PepExemptionPolicy)
            {
                case PolicyEnforcementExemptionPolicy.AllExempt:
                    return result;

                case PolicyEnforcementExemptionPolicy.DevicePrincipalsExempt:
                    if (principal.Identity is IDeviceIdentity)
                    {
                        return result;
                    }

                    break;
                case PolicyEnforcementExemptionPolicy.ApplicationPrincipalsExempt:
                    if(principal.Identity is IApplicationIdentity)
                    {
                        return result;
                    }
                    break;
                case PolicyEnforcementExemptionPolicy.ApplicationsOrDevicesExempt:
                    if (principal.Identity is IApplicationIdentity || 
                        principal.Identity is IDeviceIdentity)
                    {
                        return result;
                    }
                    break;
                case PolicyEnforcementExemptionPolicy.UserPrincipalsExempt:
                            if (!(principal.Identity is IDeviceIdentity || principal.Identity is IApplicationIdentity))
                            {
                                return result;
                            }

                            break;
                        }
            return base.Apply(result, principal);
        }
    }
}