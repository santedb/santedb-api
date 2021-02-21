/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents the claims identity
    /// </summary>
    public interface IClaimsIdentity : IIdentity
    {

        /// <summary>
        /// Gets the claims
        /// </summary>
        IEnumerable<IClaim> Claims { get; }

        /// <summary>
        /// Find the first 
        /// </summary>
        IClaim FindFirst(String claimType);

        /// <summary>
        /// Find all matching
        /// </summary>
        IEnumerable<IClaim> FindAll(String claimType);

        /// <summary>
        /// Add a single claim
        /// </summary>
        void AddClaim(IClaim claim);

        /// <summary>
        /// Add multiple claims
        /// </summary>
        void AddClaims(IEnumerable<IClaim> claims);

        /// <summary>
        /// Remove a claim
        /// </summary>
        void RemoveClaim(IClaim claim);
    }
}
