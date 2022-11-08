using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security.Certs
{
    /// <summary>
    /// Extended key usage oids
    /// </summary>
    public static class ExtendedKeyUsageOids
    {
        /// <summary>
        /// Server authentication
        /// </summary>
        public const string ServerAuthentication = "1.3.6.1.5.5.7.3.1";
        /// <summary>
        /// Client authentication
        /// </summary>
        public const string ClientAuthentication = "1.3.6.1.5.5.7.3.2";
        /// <summary>
        /// Code signing certificates
        /// </summary>
        public const string CodeSigning = "1.3.6.1.5.5.7.3.3";

    }
}
