/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System.Collections.Generic;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Implementers can map tokens to/from an identity claim set
    /// </summary>
    public interface IClaimMapper
    {

        /// <summary>
        /// Get the external token format (jwt, saml, etc.)
        /// </summary>
        string ExternalTokenFormat { get; }

        /// <summary>
        /// Map <paramref name="internalClaims"/> taken from a <see cref="IClaimsPrincipal"/> to 
        /// the external token format
        /// </summary>
        /// <param name="internalClaims">The claims from the internal SanteDB identity</param>
        /// <returns>The claims for the external token</returns>
        IDictionary<string, object> MapToExternalIdentityClaims(IEnumerable<IClaim> internalClaims);

        /// <summary>
        /// Map the external claims from a token from <paramref name="externalClaims"/> to 
        /// SanteDB equivalents
        /// </summary>
        /// <param name="externalClaims">The claims from the external token</param>
        /// <returns>The collection of SanteDB claims</returns>
        IEnumerable<IClaim> MapToInternalIdentityClaims(IDictionary<string, object> externalClaims);

        /// <summary>
        /// Gets the external claim type from the <paramref name="internalClaimType"/>
        /// </summary>
        /// <param name="internalClaimType">The internal SanteDB claim name</param>
        /// <returns>The external/standard claim</returns>
        string MapToExternalClaimType(string internalClaimType);

        /// <summary>
        /// Gets the internal claim type form the <paramref name="externalClaimType"/>
        /// </summary>
        /// <param name="externalClaimType">The standard claim to be mapped</param>
        /// <returns>The internal claim type</returns>
        string MapToInternalClaimType(string externalClaimType);

    }
}
