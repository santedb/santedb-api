/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.Services;
using System;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Session token resolver service
    /// </summary>
    public interface ISessionTokenResolverService : IServiceImplementation
    {
        /// <summary>
        /// Get the encoded id token of the <paramref name="session"/> instance provided.
        /// </summary>
        /// <param name="session">The session which should be used to generated an encoded token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
        string GetEncodedIdToken(ISession session);
        /// <summary>
        /// Get the encoded refresh token of the <paramref name="session"/> instance provided.
        /// </summary>
        /// <param name="session">The session which should be used to generated an encoded token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="session"/> is null.</exception>
        string GetEncodedRefreshToken(ISession session);
        /// <summary>
        /// Resolves the session associated with the ID token provided in <paramref name="encodedToken"/>.
        /// </summary>
        /// <param name="encodedToken">The encoded id token for the session.</param>
        /// <returns></returns>
        /// <exception cref="System.Security.SecurityException"></exception>
        ISession GetSessionFromBearerToken(string encodedToken);
        /// <summary>
        /// Resolves the session associated with the Refresh token provided in <paramref name="encodedToken"/>.
        /// </summary>
        /// <param name="encodedToken">The encoded refresh token for the session.</param>
        /// <returns></returns>
        ISession ExtendSessionWithRefreshToken(string encodedToken);
    }
}
