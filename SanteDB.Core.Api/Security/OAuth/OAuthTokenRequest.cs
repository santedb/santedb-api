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
