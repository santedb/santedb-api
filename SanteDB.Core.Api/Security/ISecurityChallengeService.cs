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
 * Date: 2020-3-18
 */
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
        /// Gets the challenges current registered for the user (not the answers)
        /// </summary>
        IEnumerable<SecurityChallenge> Get(String userName, IPrincipal principal);


        /// <summary>
        /// Gets the challenges current registered for the user (not the answers)
        /// </summary>
        IEnumerable<SecurityChallenge> Get(Guid userKey, IPrincipal principal);

        /// <summary>
        /// Add a challenge to the current registered user
        /// </summary>
        /// <param name="userName">The user the challenge is being added to</param>
        /// <param name="challengeKey">The key of the challenge question</param>
        /// <param name="response">The response for the challenge</param>
        /// <param name="principal">The principal that is setting this response</param>
        void Set(String userName, Guid challengeKey, String response, IPrincipal principal);

        /// <summary>
        /// Removes or clears the specified challenge
        /// </summary>
        /// <param name="userName">The user towhich the challenge applies</param>
        /// <param name="challengeKey">The key of the challenge question</param>
        /// <param name="principal">The principal that is setting this response</param>
        void Remove(String userName, Guid challengeKey, IPrincipal principal);


    }
}
