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
