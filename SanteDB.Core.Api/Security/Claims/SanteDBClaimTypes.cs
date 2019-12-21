/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-8-8
 */
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// SanteDB Claim Types
    /// </summary>
    public static class SanteDBClaimTypes
    {

        /// <summary>
        /// Authentication type
        /// </summary>
        public const string AuthenticationType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authentication";
        /// <summary>
        /// The authentication instant claim.
        /// </summary>
        public const string AuthenticationInstant = "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant";
        /// <summary>
        /// The authentication method claim.
        /// </summary>
        public const string AuthenticationMethod = "http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod";
        /// <summary>
        /// The expiration claim.
        /// </summary>
        public const string Expiration = "http://schemas.microsoft.com/ws/2008/06/identity/claims/expiration";
        /// <summary>
        /// The security identifier claim.
        /// </summary>
        public const string Sid = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid";
        /// <summary>
        /// Email address claim
        /// </summary>
        public const string Email = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        /// <summary>
        /// Telephone address claim
        /// </summary>
        public const string Telephone = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone";
        
        /// <summary>
        /// Default name claim
        /// </summary>
        public const string DefaultNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        /// <summary>
        /// Default role claim
        /// </summary>
        public const string DefaultRoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        /// <summary>
        /// Granted policy claim
        /// </summary>
        public const string SanteDBGrantedPolicyClaim = "scope";

        /// <summary>
        /// Device identifier claim
        /// </summary>
        public const string SanteDBDeviceIdentifierClaim = "devid";

        /// <summary>
        /// Identifier of the application
        /// </summary>
        public const string SanteDBApplicationIdentifierClaim = "appid";

        /// <summary>
        /// Secret claim
        /// </summary>
        public const string SanteDBTfaSecretClaim = "tfa";

        /// <summary>
        /// TFA secret expiry
        /// </summary>
        public const string SanteDBTfaSecretExpiry = "tfa-exp";

        /// <summary>
        /// TFA secret expiry
        /// </summary>
        public const string SanteDBScopeClaim = "scope";

        /// <summary>
        /// Override claim
        /// </summary>
        public const string SanteDBOverrideClaim = "http://santedb.org/claims/override";
        
        /// <summary>
        /// Patient identifier claim
        /// </summary>
        public const string XUAPatientIdentifierClaim = "urn:oasis:names:tc:xacml:2.0:resource:resource-id";

        /// <summary>
        /// Purpose of use claim
        /// </summary>
        public const string XspaPurposeOfUseClaim = "urn:oasis:names:tc:xacml:2.0:action:purpose";
        /// <summary>
        /// Purpose of use claim
        /// </summary>
        public const string XspaUserRoleClaim = "urn:oasis:names:tc:xacml:2.0:subject:role";
        /// <summary>
        /// Facility id claim
        /// </summary>
        public const string XspaFacilityUrlClaim = "urn:oasis:names:tc:xspa:1.0:subject:facility";
        /// <summary>
        /// Organization name claim
        /// </summary>
        public const string XspaOrganizationNameClaim = "urn:oasis:names:tc:xspa:1.0:subject:organization-id";
        /// <summary>
        /// User identifier claim
        /// </summary>
        public const string XspaUserIdentifierClaim = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";

        /// <summary>
        /// Passwordless authentication (allow authentication without password)
        /// </summary>
        public const string SanteDBPasswordlessAuth = "http://santedb.org/claims/auth-nopassword";

        /// <summary>
        /// Session id claim
        /// </summary>
        public const string SanteDBSessionIdClaim = "http://santedb.org/claims/session-id";

        /// <summary>
        /// Name claims
        /// </summary>
        public const string Name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

        /// <summary>
        /// Actor
        /// </summary>
        public const string Actor = "http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor";

        /// <summary>
        /// Name identifier
        /// </summary>
        public const string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        /// <summary>
        /// Is persistent
        /// </summary>
        public const string IsPersistent = "http://santedb.org/claims/persistent";
    }
}
