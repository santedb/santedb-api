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
 */
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SanteDB.Core.Security.OAuth
{
    /// <summary>
    /// Represents a well-formed openid configuration discovery document. OIDC Discovery protocol servers typically expose this at {server}/.well-known/openid-configuration.
    /// </summary>
    [JsonObject(nameof(OpenIdConnectDiscoveryDocument))]
    public class OpenIdConnectDiscoveryDocument
    {
        /// <summary>
        /// Gets or sets the issuer of the token
        /// </summary>
        [JsonProperty("issuer")]
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the auth endont
        /// </summary>
        [JsonProperty("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the token endpoint
        /// </summary>
        [JsonProperty("token_endpoint")]
        public string TokenEndpoint { get; set; }

        /// <summary>
        /// Get the user information endpoint
        /// </summary>
        [JsonProperty("userinfo_endpoint")]
        public string UserInfoEndpoint { get; set; }

        /// <summary>
        /// The JWKS URI
        /// </summary>
        [JsonProperty("jwks_uri")]
        public string SigningKeyEndpoint { get; set; }

        /// <summary>
        /// Gets the scopes supported
        /// </summary>
        [JsonProperty("scopes_supported")]
        public List<string> ScopesSupported { get; set; }

        /// <summary>
        /// Gets or sets the response types supported
        /// </summary>
        [JsonProperty("response_types_supported")]
        public List<string> ResponseTypesSupported { get; set; }

        /// <summary>
        /// Grant types supported
        /// </summary>
        [JsonProperty("grant_types_supported")]
        public List<string> GrantTypesSupported { get; set; }

        /// <summary>
        /// Gets the subject types supported
        /// </summary>
        [JsonProperty("subject_types_supported")]
        public List<string> SubjectTypesSupported { get; set; }

        /// <summary>
        /// Gets the signing algorithms
        /// </summary>
        [JsonProperty("id_token_signing_alg_values_supported")]
        public List<string> IdTokenSigning { get; set; }

        /// <summary>
        /// Gets or sets the endpoint that is used to sign out of the currently established session.
        /// </summary>
        [JsonProperty("end_session_endpoint")]
        public string SignoutEndpoint { get; set; }
    }
}
