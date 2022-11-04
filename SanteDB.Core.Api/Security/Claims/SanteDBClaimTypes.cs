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
using System;
using System.Data;
using System.Security.Principal;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// SanteDB Claim Types
    /// </summary>
    public static class SanteDBClaimTypes
    {

        /// <summary>
        /// Get the specified claim
        /// </summary>
        public static String GetClaimValue(this IPrincipal me, String claim)
        {
            if (me is IClaimsPrincipal icp)
            {
                return icp.FindFirst(claim)?.Value;
            }
            else
            {
                return null;
            }
        }

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
        /// The subject string of the certificate used for authentication.
        /// </summary>
        public const string AuthenticationCertificateSubject = "x509sub";

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
        /// The specific user identifier claim issued by an OAuth implementation in SanteDB.
        /// </summary>
        public const string SanteDBUserIdentifierClaim = "usrid";

        /// <summary>
        /// Claim for conveying the one time password / access code
        /// </summary>
        public const string SanteDBOTAuthCode = "otac";

        /// <summary>
        /// TFA secret expiry
        /// </summary>
        public const string SanteDBScopeClaim = "scope";

        /// <summary>
        /// Override claim
        /// </summary>
        public const string SanteDBOverrideClaim = "urn:santedb:org:override";

        /// <summary>
        /// Patient identifier claim
        /// </summary>
        public const string ResourceId = "ResourceId";

        /// <summary>
        /// Purpose of use claim
        /// </summary>
        public const string PurposeOfUse = "urn:oasis:names:tc:xacml:2.0:action:purpose";
        /// <summary>
        /// Purpose of use claim
        /// </summary>
        public const string UserRoleClaim = "SubjectRole";
        /// <summary>
        /// Facility id claim
        /// </summary>
        public const string XspaFacilityUrlClaim = "urn:oasis:names:tc:xspa:1.0:subject:facility";
        /// <summary>
        /// Organization name claim
        /// </summary>
        public const string OrganizationId = "SubjectOrganizationID";
        /// <summary>
        /// User identifier claim
        /// </summary>
        public const string SubjectId = "SubjectId";

        /// <summary>
        /// Identifies the realm that the identity is located in. Any valid realm identifier can be used. If this claim is missing, it means the identity is local to the calling system.
        /// </summary>
        public const string Realm = "http://santedb.org/claims/realm";
        
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
        /// Indicates that the session is temporary and should have a shorter lifetime than longer running
        /// interactive sessions (this is used when the user is switching roles to perform a single request)
        /// </summary>
        public const string TemporarySession = "http://santedb.org/claims/temporarySession";

        /// <summary>
        /// Language claim
        /// </summary>
        public const string Language = "http://santedb.org/claims/language";

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
        /// Organization name claim
        /// </summary>
        public const string XspaOrganizationNameClaim = "urn:oasis:names:tc:xspa:1.0:subject:organization-id";
        /// <summary>
        /// User identifier claim
        /// </summary>
        public const string XspaUserIdentifierClaim = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";

        /// <summary>
        /// HTTP header for client claims
        /// </summary>
        public const string BasicHttpClientClaimHeaderName = "X-SanteDBClient-Claim";

        /// <summary>
        /// Claim which indicates that the client MUST reset its password
        /// </summary>
        public const string ForceResetPassword = "http://santedb.org/claims/force-reset-secret";
        /// <summary>
        /// A claim that identifies a security entity with a local only perspective to the system.
        /// </summary>
        public const string LocalOnly = "http://santedb.org/claims/localonly";
    }
}
