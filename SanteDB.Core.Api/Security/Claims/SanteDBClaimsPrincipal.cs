using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents a generic claims principal
    /// </summary>
    public class SanteDBClaimsPrincipal : IClaimsPrincipal
    {

        // Get the identities
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
                new SanteDBClaimsIdentity(identity)
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
        /// Gets the claims in all the identities
        /// </summary>
        /// <value>The claims.</value>
        public IEnumerable<IClaim> Claims
        {
            get
            {
                return this.m_identities.SelectMany(o => o.Claims);
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
                this.m_identities.Add(identity as IClaimsIdentity);
            else
                this.m_identities.Add(new SanteDBClaimsIdentity(identity));
        }

        /// <summary>
        /// Find all matching
        /// </summary>
        public IEnumerable<IClaim> FindAll(String claimType)
        {
            return this.m_identities.SelectMany(o => o.Claims).Where(o => o.Type == claimType);
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
