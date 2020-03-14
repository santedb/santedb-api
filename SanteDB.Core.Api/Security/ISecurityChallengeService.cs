using SanteDB.Core.Model.Security;
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
    /// Represents an interface that allows for the retrieval of pre-configured security challenges
    /// </summary>
    public interface ISecurityChallengeService : IServiceImplementation
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
        /// Gets the challenges current registered for the user (not the answers)
        /// </summary>
        IEnumerable<SecurityChallenge> Get(Guid userKey);

        /// <summary>
        /// Add a challenge to the current registered user
        /// </summary>
        /// <param name="userKey">The user the challenge is being added to</param>
        /// <param name="challengeKey">The key of the challenge question</param>
        /// <param name="response">The response for the challenge</param>
        void Set(Guid userKey, Guid challengeKey, String response);

        /// <summary>
        /// Removes or clears the specified challenge
        /// </summary>
        /// <param name="userKey">The user towhich the challenge applies</param>
        /// <param name="challengeKey">The key of the challenge question</param>
        void Remove(Guid userKey, Guid challengeKey);

        /// <summary>
        /// Authenticates the specified user with a challenge key and response
        /// </summary>
        IPrincipal Authenticate(String userName, Guid challengeKey, String response);

    }
}
