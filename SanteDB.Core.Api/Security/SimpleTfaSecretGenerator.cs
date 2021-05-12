using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Simple TFA secret generator
    /// </summary>
    public class SimpleTfaSecretGenerator : ITwoFactorSecretGenerator
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Simple TFA Secret Generator";

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name
        {
            get
            {
                return "Simple TFA generator";
            }
        }

        /// <summary>
        /// Generate the TFA secret
        /// </summary>
        public string GenerateTfaSecret()
        {
            var secretInt = DateTime.Now.Ticks % 9999;
            return String.Format("{0:000000}", secretInt);
        }

        /// <summary>
        /// Validate the secret
        /// </summary>
        public bool Validate(string secret)
        {
            int toss;
            return secret.Length == 6 && Int32.TryParse(secret, out toss);
        }
    }
}
