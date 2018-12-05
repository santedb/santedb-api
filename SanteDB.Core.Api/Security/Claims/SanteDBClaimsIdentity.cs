using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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
        /// Initializes a new instance of the <see cref="SanteDB.DisconnectedClient.Core.Security.ClaimsIdentity"/> class.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="isAuthenticated">If set to <c>true</c> is authenticated.</param>
        public SanteDBClaimsIdentity(String userName, bool isAuthenticated, string authenticationMethod, IEnumerable<IClaim> claims = null)
        {
            if (claims != null)
                this.m_claims = new List<IClaim>(claims);
            else
                this.m_claims = new List<IClaim>();

            if (!this.m_claims.Exists(o => o.Type == SanteDBClaimTypes.DefaultNameClaimType))
                this.m_claims.Add(new SanteDBClaim(SanteDBClaimTypes.DefaultNameClaimType, userName));
            this.IsAuthenticated = isAuthenticated;
            this.AuthenticationType = authenticationMethod;
        }

        /// <summary>
        /// Create new claims identity from the specified identity
        /// </summary>
        public SanteDBClaimsIdentity(IIdentity identity) : this(identity.Name, identity.IsAuthenticated, identity.AuthenticationType)
        {
        }

        /// <summary>
        /// Add a claim
        /// </summary>
        public void AddClaim(IClaim claim)
        {
            this.m_claims.Add(claim);
        }

        /// <summary>
        /// Add a claim
        /// </summary>
        public void AddClaims(IEnumerable<IClaim> claims)
        {
            this.m_claims.AddRange(claims);
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
        public void RemoveClaim(IClaim claim)
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
