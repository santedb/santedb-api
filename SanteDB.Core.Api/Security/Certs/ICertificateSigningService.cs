/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Services;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

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
