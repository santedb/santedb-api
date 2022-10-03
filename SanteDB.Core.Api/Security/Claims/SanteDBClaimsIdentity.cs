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
 * Date: 2022-5-30
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents a generic claims identity
    /// </summary>
    /// <remarks>The whole reason for this class is PCL's lack of System.IdentityModel ClaimsIdentity</remarks>
    public class SanteDBClaimsIdentity : IClaimsIdentity
    {
        // Claims made about the user
        private List<IClaim> m_claims;

        /// <summary>
        /// Initializes a new instance of the <see cref="SanteDBClaimsIdentity"/> class.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="isAuthenticated">If set to <c>true</c> is authenticated.</param>
        /// <param name="authenticationMethod">Identifies how the principal was authenticated</param>
        /// <param name="claims">The claims which are to be attached to the identity</param>
        public SanteDBClaimsIdentity(String userName, bool isAuthenticated, string authenticationMethod, IEnumerable<IClaim> claims = null)
        {
            if (claims != null)
            {
                this.m_claims = new List<IClaim>(claims);
            }
            else
            {
                this.m_claims = new List<IClaim>();
            }

            if (!this.m_claims.Exists(o => o.Type == SanteDBClaimTypes.DefaultNameClaimType))
            {
                this.m_claims.Add(new SanteDBClaim(SanteDBClaimTypes.DefaultNameClaimType, userName));
            }

            this.IsAuthenticated = isAuthenticated;
            this.AuthenticationType = authenticationMethod;
        }

        /// <summary>
        /// Create new claims identity from the specified identity
        /// </summary>
        public SanteDBClaimsIdentity(IIdentity identity) : this(identity, null)
        {
        }

        /// <summary>
        /// Creates new claims identity from specified identity and claims
        /// </summary>
        public SanteDBClaimsIdentity(IIdentity identity, IEnumerable<IClaim> claims)
            : this(identity.Name, identity.IsAuthenticated, identity.AuthenticationType, claims)
        {

        }

        /// <summary>
        /// Add a claim
        /// </summary>
        protected void AddClaim(IClaim claim)
        {
            if (!this.m_claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
            {
                this.m_claims.Add(claim);
            }
        }

        /// <summary>
        /// Add a claim
        /// </summary>
        protected void AddClaims(IEnumerable<IClaim> claims)
        {
            foreach (var claim in claims)
            {
                if (!this.m_claims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
                {
                    this.m_claims.Add(claim);
                }
            }
        }

        /// <summary>
        /// Find the first
        /// </summary>
        public IClaim FindFirst(string claimType)
        {
            return this.m_claims.FirstOrDefault(o => o.Type == claimType);
        }

        /// <summary>
        /// Find all matching
        /// </summary>
        public IEnumerable<IClaim> FindAll(string claimType)
        {
            return this.m_claims.Where(o => o.Type == claimType);
        }

        /// <summary>
        /// Remove the specified claim
        /// </summary>
        protected void RemoveClaim(IClaim claim)
        {
            this.m_claims.Remove(claim);
        }

        /// <summary>
        /// Gets the list of claims made about the identity
        /// </summary>
        /// <value>The claim.</value>
        public IEnumerable<IClaim> Claims
        {
            get
            {
                return this.m_claims;
            }
        }

        #region IIdentity implementation

        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <returns>The type of authentication used to identify the user.</returns>
        /// <value>The type of the authentication.</value>
        public virtual string AuthenticationType
        {
            get;
        }

        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <returns>true if the user was authenticated; otherwise, false.</returns>
        /// <value><c>true</c> if this instance is authenticated; otherwise, <c>false</c>.</value>
        public virtual bool IsAuthenticated
        {
            get;
        }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <returns>The name of the user on whose behalf the code is running.</returns>
        /// <value>The name.</value>
        public virtual string Name
        {
            get
            {
                return this.m_claims.Find(o => o.Type == SanteDBClaimTypes.DefaultNameClaimType).Value;
            }
        }

        #endregion
    }
}
