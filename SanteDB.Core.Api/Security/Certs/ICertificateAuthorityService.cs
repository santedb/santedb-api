using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SanteDB.Core.Security.Certs
{
    /// <summary>
    /// Represents an integration to a certificate authority
    /// </summary>
    public interface ICertificateAuthorityService : IServiceImplementation
    {

        /// <summary>
        /// Submit a signing request to this authority
        /// </summary>
        ICertificateSigningRequest SubmitSigningRequest(byte[] csr);

        /// <summary>
        /// Get all signing requests for the CA service
        /// </summary>
        IEnumerable<ICertificateSigningRequest> GetSigningRequests(CertificateSigningRequestStatus status);

        /// <summary>
        /// Get a particular signing request by its identifier
        /// </summary>
        /// <param name="csrId">The identifier of the signing request</param>
        /// <returns>The signing request</returns>
        ICertificateSigningRequest GetSigningRequest(string csrId);

        /// <summary>
        /// Find a signing request by the specified identifier
        /// </summary>
        /// <param name="findType">The type of search operation</param>
        /// <param name="value">The value to filter on</param>
        /// <returns>The matching signing request</returns>
        ICertificateSigningRequest FindSigningRequest(X509FindType findType, object value);

        /// <summary>
        /// Delete a signing request by id
        /// </summary>
        /// <param name="csrId">The signing request identifier</param>
        /// <returns>The deleted signing request</returns>
        ICertificateSigningRequest DeleteSigningRequest(string csrId);

        /// <summary>
        /// Approve the certificate signing request
        /// </summary>
        /// <param name="request">The request which should be approved</param>
        /// <returns>The generated/signed X509 certificate</returns>
        X509Certificate2 Approve(ICertificateSigningRequest request);

        /// <summary>
        /// Reject the <paramref name="request"/>
        /// </summary>
        /// <param name="request">The request to be rejected</param>
        void Reject(ICertificateSigningRequest request);

        /// <summary>
        /// Get the issued certificates from this service
        /// </summary>
        /// <returns></returns>
        IEnumerable<X509Certificate2> GetCertificates();

        /// <summary>
        /// Revoke the certificate signing request
        /// </summary>
        /// <param name="certificate">The certificate to find</param>
        /// <returns>The certificate that was revoked</returns>
        X509Certificate2 Revoke(X509Certificate2 certificate);

        /// <summary>
        /// Renew the specified <paramref name="certificate"/>
        /// </summary>
        /// <param name="certificate">The certificate to be renewed</param>
        /// <returns>The renewed certificate</returns>
        X509Certificate2 Renew(X509Certificate2 certificate);

        /// <summary>
        /// Find the specified certificate in the 
        /// </summary>
        /// <param name="findType">The type of X509 find</param>
        /// <param name="findValue">The value to match</param>
        /// <returns>The list of matching certificates</returns>
        IEnumerable<X509Certificate2> Find(X509FindType findType, object findValue);

        /// <summary>
        /// Get the certificate that was generated for <paramref name="certRequest"/>
        /// </summary>
        X509Certificate2 GetCertificate(ICertificateSigningRequest certRequest);

        /// <summary>
        /// Get the complete revokation list
        /// </summary>
        /// <returns>The revoked certificates</returns>
        IEnumerable<X509Certificate2> GetRevokationList();
    }
}
