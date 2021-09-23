/*
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
 * Date: 2021-8-5
 */
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents a claims principal abstraction for PCL
    /// </summary>
    /// <remarks>This interface is used to abstract needed fields for allowing PCL 
    /// profile7 assemblies to access data about generated claims principals</remarks>
    public interface IClaimsPrincipal : IPrincipal
    {

        /// <summary>
        /// Gets the claims
        /// </summary>
        IEnumerable<IClaim> Claims { get; }

        /// <summary>
        /// Gets all the identities
        /// </summary>
        IClaimsIdentity[] Identities { get; }

        /// <summary>
        /// Find all claims
        /// </summary>
        IClaim FindFirst(string santeDBDeviceIdentifierClaim);

        /// <summary>
        /// Find all claims
        /// </summary>
        bool TryGetClaimValue(string claimType, out string value);

        /// <summary>
        /// Find all claims
        /// </summary>
        IEnumerable<IClaim> FindAll(string santeDBDeviceIdentifierClaim);

        /// <summary>
        /// Add an identity
        /// </summary>
        void AddIdentity(IIdentity identity);

        /// <summary>
        /// Determine if the principal has a claim
        /// </summary>
        bool HasClaim(Func<IClaim, bool> predicate);
    }
}
