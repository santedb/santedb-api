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
using SanteDB.Core.Security.Principal;
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
    /// Represents a service which retrieves <see cref="IDeviceIdentity"/> and can authenticate to an <see cref="IPrincipal"/> for devices.
    /// </summary>
    /// <remarks>
    /// <para>In SanteDB, a security session is comprised of up to three security identities/principals:</para>
    /// <list type="bullet">
    ///     <item>(Optional) User identity representing the human using the application</item>
    ///     <item>(Optional) A <see cref="IDeviceIdentity"/> representing the device running the application, and</item>
    ///     <item>An <see cref="IApplicationIdentity"/> representing the application</item>
    /// </list>
    /// <para>This service is what is used to authenticate the device identity from a central credential store of registered devices. This service may be 
    /// called with a shared device id/secret (like a user name and password), or may be called with a device ID and x509 certificate (if used for authenticating
    /// sessions with a client certificate)</para>
    /// <para>See: <see href="https://help.santesuite.org/santedb/security-architecture#principals-and-identities">SanteDB authentication architecture</see></para>
    /// </remarks>
    [System.ComponentModel.Description("Device Identity Provider")]
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
        /// <param name="deviceSecret">The new secret (or thumbprint of certificate to be used)</param>
        /// <param name="name">The device identifier for which the secret is being changed</param>
        /// <param name="principal">The principal which is changing the secret</param>
        void ChangeSecret(string name, string deviceSecret, IPrincipal principal);
    }
}
