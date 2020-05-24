/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System.Security.Principal;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a session identity service that can provide identities
    /// </summary>
    public interface ISessionIdentityProviderService : IServiceImplementation
    {

        /// <summary>
        /// Authenticate based on session
        /// </summary>
        /// <param name="session">The session which is being sought for authentication</param>
        /// <returns>The authenticated principal</returns>
        IPrincipal Authenticate(ISession session);


        /// <summary>
        /// Gets an un-authenticated principal from the specified session
        /// </summary>
        /// <param name="session">The session to get an authenticated principal from</param>
        /// <returns>The unauthenticated principal</returns>
        IIdentity[] GetIdentities(ISession session);
        
    }
}
