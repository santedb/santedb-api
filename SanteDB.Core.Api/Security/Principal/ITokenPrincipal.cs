using SanteDB.Core.Security.Claims;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Principal
{
    /// <summary>
    /// A principal which was authenticated with token based credentials (so it is a Principal - but a session as well)
    /// </summary>
    public interface ITokenPrincipal : IClaimsPrincipal
    {

        /// <summary>
        /// Get the tokens with name and values
        /// </summary>
        String AccessToken { get; }

        /// <summary>
        /// Get the token types
        /// </summary>
        String TokenType { get; }

        /// <summary>
        /// Get the expiration time
        /// </summary>
        DateTimeOffset ExpiresAt { get; }

        /// <summary>
        /// Get the identity token structure
        /// </summary>
        String IdentityToken { get; }

        /// <summary>
        /// Gets the refresh token
        /// </summary>
        String RefreshToken { get; }
    }
}
