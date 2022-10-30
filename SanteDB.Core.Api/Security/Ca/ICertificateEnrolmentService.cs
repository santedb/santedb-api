using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SanteDB.Core.Security.Ca
{

    /// <summary>
    /// Represents a service which is responsible for creating and enroling certificates
    /// </summary>
    public interface ICertificateEnrolmentService  : IServiceImplementation
    {

        /// <summary>
        /// Create a signing request data
        /// </summary>
        /// <param name="cnName">The common name on the certificate</param>
        /// <param name="ouName">The organization unit name</param>
        /// <param name="privateKey">The private key that was generated for the signing request</param>
        byte[] CreateSigningRequest(String cnName, String ouName, out RSAParameters privateKey);

        /// <summary>
        /// Convert the <paramref name="serializedCertificate"/> to a X509Certificate2
        /// </summary>
        /// <param name="serializedCertificate">The certificate which was obtained from the upstream certificate store</param>
        /// <param name="privateKey">The private key which matches the certificate</param>
        /// <returns>The converted certificate</returns>
        X509Certificate2 ConvertToCertificate(byte[] serializedCertificate, RSAParameters privateKey);

    }
}
