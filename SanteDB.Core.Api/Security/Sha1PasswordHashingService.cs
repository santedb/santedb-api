using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// SHA1 password generator service
    /// </summary>
    [ServiceProvider("SHA1 Password Encoding Service")]
    [Obsolete("Only use this when migrating a legacy deployment")]
    public class SHA1PasswordHashingService : IPasswordHashingService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "SHA1 Password Encoding Service";

        /// <summary>
        /// Encode a password using the SHA256 encoding
        /// </summary>
        public string ComputeHash(string password)
        {
            SHA1 hasher = SHA1.Create();
            return BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower();
        }
    }
}