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
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{

    /// <summary>
    /// Authenticating event arguments.
    /// </summary>
    public class AuthenticatingEventArgs : AuthenticatedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatingEventArgs"/> class.
        /// </summary>
        /// <param name="userName">User name.</param>
        public AuthenticatingEventArgs(String userName) : base(userName, null, true)
        {

        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance cancel.
        /// </summary>
        /// <value><c>true</c> if this instance cancel; otherwise, <c>false</c>.</value>
        public bool Cancel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the override was successful
        /// </summary>
        public new bool Success
        {
            get => base.Success;
            set => base.Success = value;
        }
    }

    /// <summary>
    /// Authentication event args.
    /// </summary>
    public class AuthenticatedEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Services.AuthenticatingEventArgs"/> class.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="principal">The principal that was authenticated</param>
        /// <param name="success">True if the authentication was granted</param>
        public AuthenticatedEventArgs(String userName, IPrincipal principal, bool success)
        {
            this.UserName = userName;
            this.Principal = principal;
            this.Success = success;
        }

        /// <summary>
        /// Indicates success
        /// </summary>
        public bool Success { get; protected set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public String UserName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the principal.
        /// </summary>
        /// <value>The principal.</value>
        public IPrincipal Principal
        {
            get;
            set;
        }

    }

    /// <summary>
    /// Override event args
    /// </summary>
    public class SecurityOverrideEventArgs : EventArgs
    {
        /// <summary>
        /// Creates the override event args
        /// </summary>
        public SecurityOverrideEventArgs(IPrincipal principal, string purposeOfUse, IEnumerable<String> scopes)
        {
            this.Principal = principal;
            this.PurposeOfUse = purposeOfUse;
            this.Scopes = scopes;
        }

        /// <summary>
        /// Purpose of use
        /// </summary>
        public String PurposeOfUse { get; private set; }

        /// <summary>
        /// Gets the scopes
        /// </summary>
        public IEnumerable<String> Scopes { get; private set; }

        /// <summary>
        /// The principal requesting override
        /// </summary>
        public IPrincipal Principal { get; private set; }

        /// <summary>
        /// Set to true to cancel
        /// </summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// Identifies a class which can generate TFA secrets
    /// </summary>
    [System.ComponentModel.Description("TFA/MFA Secret Generator")]
    public interface ITwoFactorSecretGenerator : IServiceImplementation
    {
        /// <summary>
        /// Gets the name of the TFA generator
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Generate a TFA secret
        /// </summary>
        String GenerateTfaSecret();

        /// <summary>
        /// Validates the secret 
        /// </summary>
        bool Validate(String secret);
    }

    /// <summary>
    /// Identity provider service
    /// </summary>
    [System.ComponentModel.Description("User Identity Provider")]
    public interface IIdentityProviderService : IServiceImplementation
    {

        /// <summary>
        /// Fired prior to an authentication event
        /// </summary>
        event EventHandler<AuthenticatingEventArgs> Authenticating;

        /// <summary>
        /// Fired after an authentication decision being made
        /// </summary>
        event EventHandler<AuthenticatedEventArgs> Authenticated;

        /// <summary>
        /// Retrieves an identity from the object
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        IIdentity GetIdentity(String userName);

        /// <summary>
        /// Retrieves an identity by the security identifier
        /// </summary>
        /// <param name="sid">The security identifier</param>
        IIdentity GetIdentity(Guid sid);

        /// <summary>
        /// Create a basic identity in the provider
        /// </summary>
        /// <param name="userName">The username of the identity</param>
        /// <param name="password">The intitial password of the identity</param>
        /// <returns>The created identity</returns>
        /// <param name="principal">The principal that was created</param>
        IIdentity CreateIdentity(String userName, String password, IPrincipal principal);

        /// <summary>
        /// Authenticate the user creating an identity
        /// </summary>
        /// <returns></returns>
        IPrincipal Authenticate(String userName, String password);

        /// <summary>
        /// Authenticate the user using two factor authentication
        /// </summary>
        IPrincipal Authenticate(String userName, String password, String tfaSecret);


        /// <summary>
        /// Change user password
        /// </summary>
        void ChangePassword(String userName, String newPassword, IPrincipal principal);

        /// <summary>
        /// Delete an identity
        /// </summary>
        void DeleteIdentity(String userName, IPrincipal principal);

        /// <summary>
        /// Set lockout
        /// </summary>
        void SetLockout(String userName, bool lockout, IPrincipal principal);

        /// <summary>
        /// Adds a claim to the specified user account
        /// </summary>
        void AddClaim(String userName, IClaim claim, IPrincipal principal, TimeSpan? expiry = null);

        /// <summary>
        /// Removes a claim from the specified user account
        /// </summary>
        void RemoveClaim(String userName, String claimType, IPrincipal principal);

        /// <summary>
        /// Get all active claims for the specified user
        /// </summary>
        /// <param name="userName">The user name for which claims should be retrieved</param>
        /// <returns>The configured claims on the user</returns>
        IEnumerable<IClaim> GetClaims(String userName);

        /// <summary>
        /// Get the SID for the named user
        /// </summary>
        Guid GetSid(String name);
    }

}

