﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.OAuth
{
    /// <summary>
    /// Serialization class for the OAUTH response
    /// </summary>
    public class OAuthTokenResponse
    {
        /// <summary>
        /// Gets or sets the access token generated by the oauth server
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        /// <summary>
        /// Gets or sets the identity token provided by the oauth server
        /// </summary>
        [JsonProperty("id_token")]
        public string IdToken { get; set; }
        /// <summary>
        /// Gets or sets the type of identity token provided
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        /// <summary>
        /// Gets or sets the refresh token issued by the oauth server
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        /// <summary>
        /// Gets or sets the time in seconds that the <see cref="AccessToken"/> expires
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        /// <summary>
        /// Gets or sets the nonce from the oauth server (echo from the client)
        /// </summary>
        [JsonProperty("nonce")]
        public string Nonce { get; set; }
        /// <summary>
        /// Gets or sets the error provided by the oauth server
        /// </summary>
        [JsonProperty("error")]
        public string Error { get; set; }
        /// <summary>
        /// Gets or sets the additional error description from the oauth server
        /// </summary>
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

    }
}
