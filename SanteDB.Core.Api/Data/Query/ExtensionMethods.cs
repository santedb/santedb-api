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
 * Date: 2024-6-21
 */
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SanteDB.Core.Data.Query
{

    /// <summary>
    /// Extension methods for the query filters
    /// </summary>
    public static class ExtensionMethods
    {

        /// <summary>
        /// Compute the MD5 hash data for <paramref name="data"/>
        /// </summary>
        public static string ComputeMd5Hash(this byte[] data) => MD5.Create().ComputeHash(data).HexEncode();
        
        /// <summary>
        /// Determines if <paramref name="securityEntity"/> has a claim <paramref name="claimType"/> 
        /// </summary>
        /// <param name="securityEntity">The security object for which the claim is being looked up</param>
        /// <param name="claimType">The type of the claim</param>
        /// <returns>The value of the claim value matches the value</returns>
        public static String ClaimLookup(this SecurityEntity securityEntity, String claimType)
        {

            var appServiceProvider = ApplicationServiceContext.Current;
            IEnumerable<IClaim> claimSource = null;
            switch (securityEntity)
            {
                case SecurityUser su:
                    claimSource = appServiceProvider.GetService<IIdentityProviderService>().GetClaims(su.UserName);
                    break;
                case SecurityApplication sa:
                    claimSource = appServiceProvider.GetService<IApplicationIdentityProviderService>().GetClaims(sa.Name);
                    break;
                case SecurityDevice sd:
                    claimSource = appServiceProvider.GetService<IDeviceIdentityProviderService>().GetClaims(sd.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(SecurityUser), securityEntity.GetType()));
            }

            // Lookup claim value
            return claimSource.Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase)).Select(o => o.Value).FirstOrDefault();

        }
    }
}
