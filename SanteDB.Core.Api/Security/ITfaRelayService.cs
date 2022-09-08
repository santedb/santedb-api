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
 * Date: 2022-5-30
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;

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

    /// <summary>
    /// Represents a two-factor authentication relay service
    /// </summary>
    public interface ITfaRelayService : IServiceImplementation
    {

        /// <summary>
        /// Send the secret for the specified user
        /// </summary>
        string SendSecret(Guid mechanismId, IIdentity user);

        /// <summary>
        /// Send the secret for the specified user
        /// </summary>
        bool ValidateSecret(Guid mechanismId, IIdentity user, String secret);

        /// <summary>
        /// Gets the tfa mechanisms supported by this relay service
        /// </summary>
        IEnumerable<ITfaMechanism> Mechanisms { get; }
        
    }
}
