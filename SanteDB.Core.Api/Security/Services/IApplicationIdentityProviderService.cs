/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a service which retrieves <see cref="IApplicationIdentity"/> and can authenticate to an <see cref="IPrincipal"/> for applications.
    /// </summary>
    /// <remarks>
    /// <para>In SanteDB, a security session is comprised of up to three security identities/principals:</para>
    /// <list type="bullet">
    ///     <item>(Optional) User identity representing the human using the application</item>
    ///     <item>(Optional) Device identity representing the device running the application, and</item>
    ///     <item>An <see cref="IApplicationIdentity"/> representing the application</item>
    /// </list>
    /// <para>This service is what is used to authenticate the application identity from a central credential store of registered applications.</para>
    /// <para>See: <see href="https://help.santesuite.org/santedb/security-architecture#principals-and-identities">SanteDB authentication architecture</see></para>
    /// </remarks>
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
        /// <param name="applicationName">The application id to authenticate.</param>
        /// <param name="applicationSecret">The application secret to authenticate.</param>
        /// <returns>Returns the principal of the application.</returns>
        IPrincipal Authenticate(String applicationName, String applicationSecret);

        /// <summary>
        /// Authenticate the application identity given an existing authentication context.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="authenticationContext"></param>
        /// <returns></returns>
        IPrincipal Authenticate(String applicationName, IPrincipal authenticationContext);

        /// <summary>
        /// Create a basic identity in the provider
        /// </summary>
        /// <param name="applicationName">The username of the identity</param>
        /// <param name="password">The intitial password of the identity</param>
        /// <returns>The created identity</returns>
        /// <param name="principal">The principal that was created</param>
        /// <param name="withSid">When supplied the security identifier for the new identity</param>
        IApplicationIdentity CreateIdentity(String applicationName, String password, IPrincipal principal, Guid? withSid = null);

        /// <summary>
        /// Gets the specified identity for an application.
        /// </summary>
        /// <param name="applicationName">The name of the application for which to retrieve the identity.</param>
        /// <returns>Returns the identity of the application.</returns>
        IApplicationIdentity GetIdentity(string applicationName);

        /// <summary>
        /// Gets the specified identity for the application
        /// </summary>
        /// <param name="sid">The security identifier </param>
        /// <returns>The application identity</returns>
        IApplicationIdentity GetIdentity(Guid sid);

        /// <summary>
        /// Gets the SID for the specified identity
        /// </summary>
        Guid GetSid(string name);

        /// <summary>
        /// Set the lockout status
        /// </summary>
        /// <param name="applicationName">The name of the device</param>
        /// <param name="lockoutState">The status of the lockout</param>
        /// <param name="principal">The principal which is locking the device</param>
        void SetLockout(string applicationName, bool lockoutState, IPrincipal principal);

        /// <summary>
        /// Change the specified application identity's secret
        /// </summary>
        /// <param name="applicationName">The name of the application</param>
        /// <param name="secret">The new secret</param>
        /// <param name="principal">The principal that is changing the secret</param>
        void ChangeSecret(String applicationName, String secret, IPrincipal principal);

        /// <summary>
        /// Add a <paramref name="claim"/> to <paramref name="applicationName"/> 
        /// </summary>
        /// <param name="applicationName">The name of the device to which the claim should be added</param>
        /// <param name="claim">The claim which is to be added</param>
        /// <param name="principal">The principal which is adding the claim</param>
        /// <param name="expiry">The expiry time for the claim</param>
        void AddClaim(String applicationName, IClaim claim, IPrincipal principal, TimeSpan? expiry = null);

        /// <summary>
        /// Get all active claims for the specified application
        /// </summary>
        /// <param name="applicationName">The application name for which claims should be retrieved</param>
        /// <returns>The configured claims on the application</returns>
        IEnumerable<IClaim> GetClaims(String applicationName);

        /// <summary>
        /// Removes a claim from the specified device account
        /// </summary>
        /// <param name="claimType">The claim type to be removed</param>
        /// <param name="principal">The principal which is removing the claim</param>
        /// <param name="applicationName">The name of the device account from which the claim should be removed</param>
        void RemoveClaim(String applicationName, String claimType, IPrincipal principal);
    }


}