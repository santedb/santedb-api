/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-27
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents an interface that allows for the retrieval of pre-configured security challenges
    /// </summary>
    [System.ComponentModel.Description("Security Challenge Storage Provider")]
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
