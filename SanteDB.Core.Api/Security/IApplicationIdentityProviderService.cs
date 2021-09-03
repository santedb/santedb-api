/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.ComponentModel;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a service which retrieves IPrincipal objects for applications.
    /// </summary>
    [System.ComponentModel.Description("Application Identity Provider")]
    public interface IApplicationIdentityProviderService : IServiceImplementation
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
		/// Authenticate the application identity.
		/// </summary>
		/// <param name="applicationId">The application id to authenticate.</param>
		/// <param name="applicationSecret">The application secret to authenticate.</param>
		/// <returns>Returns the principal of the application.</returns>
		IPrincipal Authenticate(String applicationId, String applicationSecret);

		/// <summary>
		/// Gets the specified identity for an application.
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
        /// Change the specified application identity's secret
        /// </summary>
        /// <param name="name">The name of the application</param>
        /// <param name="secret">The new secret</param>
        /// <param name="principal">The principal that is changing the secret</param>
        void ChangeSecret(String name, String secret, IPrincipal principal);

        /// <summary>
        /// Get the secure key for the specified application (can be used for symmetric encryption)
        /// </summary>
        byte[] GetSecureKey(String name);
    }
}