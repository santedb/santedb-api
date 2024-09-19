/*
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents a generic claims principal
    /// </summary>
    public class SanteDBClaimsPrincipal : IClaimsPrincipal
    {

        /// <summary>
        /// Identities that this claims principal contains
        /// </summary>
        protected List<IClaimsIdentity> m_identities;

        /// <summary>
        /// Identities
        /// </summary>
        public SanteDBClaimsPrincipal()
        {
            this.m_identities = new List<IClaimsIdentity>();
        }

        /// <summary>
        /// Create a new claims principal
        /// </summary>
        public SanteDBClaimsPrincipal(IClaimsIdentity identity)
        {
            this.m_identities = new List<IClaimsIdentity>() { identity };
        }

        /// <summary>
        /// Creates a new claims principal with the specified identity
        /// </summary>
        public SanteDBClaimsPrincipal(IIdentity identity)
        {
            this.m_identities = new List<IClaimsIdentity>()
            {
                identity as IClaimsIdentity ?? new SanteDBClaimsIdentity(identity)
            };
        }

        /// <summary>
        /// Identities
        /// </summary>
        public SanteDBClaimsPrincipal(IEnumerable<IClaimsIdentity> identities)
        {
            this.m_identities = identities.ToList();
        }

        /// <summary>
        /// Identities
        /// </summary>
        public SanteDBClaimsPrincipal(IEnumerable<IIdentity> identities)
        {
            this.m_identities = identities.Select(o => o is IClaimsIdentity ? o : new SanteDBClaimsIdentity(o)).OfType<IClaimsIdentity>().ToList();
        }

        /// <summary>
        /// Gets the claims in all the identities
        /// </summary>
        /// <value>The claims.</value>
        public IEnumerable<IClaim> Claims
        {
            get
            {
                var claims = this.m_identities.SelectMany(o => o.Claims).ToList();
                while (claims.Count(o => o.Type == SanteDBClaimTypes.DefaultNameClaimType) > 1)
                {
                    claims.Remove(claims.Last(o => o.Type == SanteDBClaimTypes.DefaultNameClaimType));
                }

                return claims;
            }
        }

        /// <summary>
        /// Determines whether this instance has claim the specified predicate.
        /// </summary>
        /// <returns><c>true</c> if this instance has claim the specified predicate; otherwise, <c>false</c>.</returns>
        /// <param name="predicate">Predicate.</param>
        public bool HasClaim(Func<IClaim, bool> predicate)
        {
            return this.Claims.Any(predicate);
        }

        /// <summary>
        /// Finds the specified claim.
        /// </summary>
        /// <returns>The claim.</returns>
        /// <param name="claimType">Claim type.</param>
        public IClaim FindFirst(String claimType)
        {
            return this.Claims.FirstOrDefault(o => o.Type == claimType);
        }

        #region IPrincipal implementation
        /// <summary>
        /// Determines whether the current principal belongs to the specified role.
        /// </summary>
        /// <returns>true if the current principal is a member of the specified role; otherwise, false.</returns>
        /// <param name="role">The name of the role for which to check membership.</param>
        public bool IsInRole(string role)
        {
            return this.Claims.Any(o => o.Type == SanteDBClaimTypes.DefaultRoleClaimType && o.Value == role);
        }

        /// <summary>
        /// Add identity
        /// </summary>
        public void AddIdentity(IIdentity identity)
        {
            if (identity is IClaimsIdentity)
            {
                this.m_identities.Add(identity as IClaimsIdentity);
            }
            else
            {
                this.m_identities.Add(new SanteDBClaimsIdentity(identity));
            }
        }

        /// <summary>
        /// Find all matching
        /// </summary>
        public IEnumerable<IClaim> FindAll(String claimType)
        {
            return this.m_identities.SelectMany(o => o.Claims).Where(o => o.Type == claimType);
        }

        /// <summary>
        /// Try to get the claim value
        /// </summary>
        public bool TryGetClaimValue(string claimType, out string value)
        {
            var claim = this.m_identities.SelectMany(o => o.Claims).FirstOrDefault(o => o.Type.Equals(claimType));
            value = claim?.Value;
            return claim != null;
        }


        /// <summary>
        /// Gets the primary identity
        /// </summary>
        /// <value>The identity.</value>
        public IIdentity Identity
        {
            get
            {
                return this.m_identities.FirstOrDefault();
            }
        }

        /// <summary>
        /// Get all identities
        /// </summary>
        public IClaimsIdentity[] Identities => this.m_identities.ToArray();

        #endregion
    }
}
