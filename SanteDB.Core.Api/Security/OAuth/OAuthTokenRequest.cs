/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2025-2-5
 */
using SanteDB.Core.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.OAuth
{
    /// <summary>
    /// Represents an OAUTH token request to be sent to an OAUTH server
    /// </summary>
    public class OAuthTokenRequest
    {
        /// <summary>
        /// The type of grant to use
        /// </summary>
        [FormElement("grant_type")]
        public string GrantType { get; set; }
        /// <summary>
        /// Gets or sets the username to send to the server
        /// </summary>
        [FormElement("username")]
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets the password to send to the oauth server
        /// </summary>
        [FormElement("password")]
        public string Password { get; set; }
        /// <summary>
        /// Gets or sets the client identification 
        /// </summary>
        [FormElement("client_id")]
        public string ClientId { get; set; }
        /// <summary>
        /// Gets or sets the client secret for secure clients
        /// </summary>
        [FormElement("client_secret")]
        public string ClientSecret { get; set; }
        /// <summary>
        /// Gets or sets the nonce 
        /// </summary>
        [FormElement("nonce")]
        public string Nonce { get; set; }
        /// <summary>
        /// Gets or sets the refresh token for refresh grants
        /// </summary>
        [FormElement("refresh_token")]
        public string RefreshToken { get; set; }
        /// <summary>
        /// Gets or sets the MFA response provided by the user
        /// </summary>
        [FormElement("x_mfa")]
        public string MfaCode { get; set; }
        /// <summary>
        /// Gets or sets the challenge identifier for the user
        /// </summary>
        [FormElement("challenge")]
        public String Challenge { get; set; }
        /// <summary>
        /// Gets orsets the challenge response collected from the user
        /// </summary>
        [FormElement("response")]
        public String Response { get; set; }
        /// <summary>
        /// Gets or sets the SCOPE(s) the client is requesting 
        /// </summary>
        [FormElement("scope")]
        public string Scope { get; set; }

    }
}
