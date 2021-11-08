using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// SHA256 password generator service
    /// </summary>
    [ServiceProvider("SHA256 Password Encoding Service")]
    public class SHA256PasswordHashingService : IPasswordHashingService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "SHA256 Password Encoding Service";

        /// <summary>
        /// Encode a password using the SHA256 encoding
        /// </summary>
        public string ComputeHash(string password)
        {
            SHA256 hasher = SHA256.Create();
            return BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower();
        }
    }
}