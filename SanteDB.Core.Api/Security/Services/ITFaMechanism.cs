using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents the TFA mechanism
    /// </summary>
    public interface ITfaMechanism
    {

        /// <summary>
        /// Gets the identifier of the mechanism
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the name of the TFA mechanism
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Send the specified two factor authentication via the mechanism 
        /// </summary>
        /// <param name="user">The user to send the TFA secret for</param>
        /// <returns>Special instructional text</returns>
        String Send(IIdentity user);

        /// <summary>
        /// Validate the secret
        /// </summary>
        bool Validate(IIdentity user, string secret);
    }
}
