/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-6-28
 */
using SanteDB.Core.Security.Claims;
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
        /// Initializes a new instance of the <see cref="SanteDB.Core.Services.AuthenticatingEventArgs"/> class.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
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
    }

    /// <summary>
    /// Authentication event args.
    /// </summary>
    public class AuthenticatedEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SanteDB.Core.Services.AuthenticatingEventArgs"/> class.
        /// </summary>
        /// <param name="userName">User name.</param>
        public AuthenticatedEventArgs(String userName, IPrincipal principal, bool success)
        {
            this.UserName = userName;

            this.Success = success;
        }

        /// <summary>
        /// Indicates success
        /// </summary>
        public bool Success { get; private set; }

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
    public interface ITwoFactorSecretGenerator
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
    public interface IIdentityProviderService
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
        /// Create a basic identity in the provider
        /// </summary>
        /// <param name="userName">The username of the identity</param>
        /// <param name="password">The intitial password of the identity</param>
        /// <returns>The created identity</returns>
        IIdentity CreateIdentity(String userName, String password);

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
        void ChangePassword(String userName, String newPassword);

        /// <summary>
        /// Set the user's two factor authentication secret
        /// </summary>
        String GenerateTfaSecret(String userName);

        /// <summary>
        /// Delete an identity
        /// </summary>
        void DeleteIdentity(String userName);

        /// <summary>
        /// Set lockout
        /// </summary>
        void SetLockout(String userName, bool lockout);

        /// <summary>
        /// Adds a claim to the specified user account
        /// </summary>
        void AddClaim(String userName, IClaim claim);

        /// <summary>
        /// Removes a claim from the specified user account
        /// </summary>
        void RemoveClaim(String userName, String claimType);

    }

    /// <summary>
    /// Represents an identity provider that allows for elevation
    /// </summary>
    public interface IElevatableIdentityProviderService : IIdentityProviderService
    {

        /// <summary>
        /// The caller has requested an override
        /// </summary>
        event EventHandler<SecurityOverrideEventArgs> OverrideRequested;

        /// <summary>
        /// Requests the currently established principal to be elevated
        /// </summary>
        /// <param name="principal">The principal to be elevated</param>
        /// <param name="password">The password for the principal</param>
        /// <param name="purpose">The reason for the elevation</param>
        /// <param name="policies">One or more policies which the principal is seeking override</param>
        IPrincipal Elevate(IPrincipal principal, String password, String purpose, params String[] policies);
    }
   
}

