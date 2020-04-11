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
 * User: fyfej
 * Date: 2019-11-27
 */
using SanteDB.Core.Services;
using System;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{

    /// <summary>
    /// Represents methods of authentication
    /// </summary>
    [Flags]
    public enum AuthenticationMethod
    {
        /// <summary>
        /// Perform only local authentication
        /// </summary>
        Local = 0x1,
        /// <summary>
        /// Perform only online authentication
        /// </summary>
        Online = 0x2,
        /// <summary>
        /// Use either method
        /// </summary>
        Any = Local | Online
    }

    /// <summary>
	/// Represents an identity service which authenticates devices.
	/// </summary>
	public interface IDeviceIdentityProviderService : IServiceImplementation
    {
        /// <summary>
        /// Fired after an authentication request has been made.
        /// </summary>
        event EventHandler<AuthenticatedEventArgs> Authenticated;

        /// <summary>
        /// Fired prior to an authentication request being made.
        /// </summary>
        event EventHandler<AuthenticatingEventArgs> Authenticating;

        /// <summary>
        /// Authenticates the specified device identifier.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceSecret">The device secret.</param>
        /// <param name="authMethod">Identifies the allowed authentication methods</param>
        /// <returns>Returns the authenticated device principal.</returns>
        IPrincipal Authenticate(string deviceId, string deviceSecret, AuthenticationMethod authMethod = AuthenticationMethod.Any);


        /// <summary>
        /// Gets the specified identity for an device.
        /// </summary>
        /// <param name="name">The name of the application for which to retrieve the identity.</param>
        /// <returns>Returns the identity of the application.</returns>
        IIdentity GetIdentity(string name);

        /// <summary>
        /// Set the lockout status 
        /// </summary>
        /// <param name="name">The name of the device</param>
        /// <param name="lockoutState">The status of the lockout</param>
        /// <param name="principal">The principal which is locking the device</param>
        void SetLockout(string name, bool lockoutState, IPrincipal principal);

        /// <summary>
        /// Change the device secret
        /// </summary>
        void ChangeSecret(string name, string deviceSecret, IPrincipal systemPrincipal);
    }
}
