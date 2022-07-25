using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Session token resolver service
    /// </summary>
    public interface ISessionTokenResolverService : IServiceImplementation
    {

        /// <summary>
        /// Resolve an <see cref="ISession"/> instance from the provided token
        /// </summary>
        /// <param name="token">The token which contains the session information</param>
        /// <returns>The resolved session</returns>
        ISession Resolve(String token);

        /// <summary>
        /// Serialize the <paramref name="session"/> to a byte format
        /// </summary>
        /// <param name="session">The session which is to be serialized</param>
        /// <returns>The serialized session</returns>
        String Serialize(ISession session);
    }
}
