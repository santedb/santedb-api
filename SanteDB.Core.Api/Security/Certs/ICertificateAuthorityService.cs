/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.Services;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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
