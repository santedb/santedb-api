using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SanteDB.Core.Security.Certs
{
    /// <summary>
    /// Certificate signing service (can sign certificates with other certificates)
    /// </summary>
    public interface ICertificateSigningService : IServiceImplementation
    {

        /// <summary>
        /// Signs <paramref name="request"/> with <paramref name="signWithCertificate"/>
        /// </summary>
        /// <param name="request">The signing request to sign</param>
        /// <param name="signWithCertificate">The certificate to sign the request with</param>
        /// <returns>The signed certificate</returns>
        X509Certificate2 SignCertificateRequest(byte[] request, X509Certificate2 signWithCertificate);

    }
}
