using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a service which manages associations between various identities 
    /// </summary>
    public interface IDataSigningCertificateManagerService : IServiceImplementation
    {
        /// <summary>
        /// Adds a signing certificate to the identity
        /// </summary>
        /// <param name="identity">The identity to add signing credentials for</param>
        /// <param name="x509Certificate">The certificate name</param>
        /// <param name="principal">The principal performing the operation</param>
        void AddSigningCertificate(IIdentity identity, X509Certificate2 x509Certificate, IPrincipal principal);

        /// <summary>
        /// Removes a signing certificate from an identity
        /// </summary>
        /// <param name="identity">The identity to remove the signing credentials for</param>
        /// <param name="x509Certificate">The certificate to remove</param>
        /// <param name="principal">The principal performing the operation</param>
        void RemoveSigningCertificate(IIdentity identity, X509Certificate2 x509Certificate, IPrincipal principal);

        /// <summary>
        /// Gets signing certificates associated with <paramref name="identity"/>
        /// </summary>
        /// <param name="identity">The identity for which signing credentials should be obtained</param>
        /// <returns>Registered signing credentials</returns>
        IEnumerable<X509Certificate2> GetSigningCertificates(IIdentity identity);

        /// <summary>
        /// Get any configured signing certificate based on the thumbprint
        /// </summary>
        bool TryGetSigningCertificateByThumbprint(String x509Thumbprint, out X509Certificate2 certificate);

        /// <summary>
        /// Get configured certificate by hash
        /// </summary>
        bool TryGetSigningCertificateByHash(byte[] x509hash, out X509Certificate2 certificate);

    }
}
