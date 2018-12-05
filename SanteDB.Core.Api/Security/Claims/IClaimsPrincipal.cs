using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

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
