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

        /// <summary>
        /// Get the certificates that this service can use to sign requests
        /// </summary>
        /// <returns>The signing certificate data</returns>
        IEnumerable<X509Certificate2> GetSigningCertificates();

        /// <summary>
        /// Parse the signing request and return the DN
        /// </summary>
        /// <param name="request">The request to be parsed</param>
        /// <returns>The distinguished name n the certificate request</returns>
        X500DistinguishedName GetSigningRequestDN(byte[] request);

    }
}
