using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a security challenge service which can provide identity
    /// </summary>
    public interface ISecurityChallengeIdentityService : IServiceImplementation
    {

        /// <summary>
        /// Fired prior to an authentication event
        /// </summary>
        event EventHandler<AuthenticatingEventArgs> Authenticating;

        /// <summary>
        /// Fired after an authentication decision being made
        /// </summary>
        event EventHandler<AuthenticatedEventArgs> Authenticated;

        /// <summary>
        /// Authenticates the specified user with a challenge key and response
        /// </summary>
        IPrincipal Authenticate(String userName, Guid challengeKey, String response, String tfaSecret);
    }
}
