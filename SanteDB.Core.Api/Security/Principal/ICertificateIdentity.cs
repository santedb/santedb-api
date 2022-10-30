using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Principal
{
    /// <summary>
    /// An identity which was created and authenticated using a certificate
    /// </summary>
    public interface ICertificateIdentity : IIdentity
    {

        /// <summary>
        /// Gets the certificate used to authenticate this identity
        /// </summary>
        X509Certificate2 AuthenticationCertificate { get; }

    }
}
