using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
