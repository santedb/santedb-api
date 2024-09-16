/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.Services;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SanteDB.Core.Security.Certs
{

    /// <summary>
    /// Represents a service which is responsible for creating and enroling certificates
    /// </summary>
    public interface ICertificateGeneratorService : IServiceImplementation
    {
        /// <summary>
        /// Create a private key
        /// </summary>
        /// <param name="keyLength">The length of the key to generate</param>
        /// <returns>The generated private key</returns>
        RSAParameters CreateKeyPair(int keyLength);

        /// <summary>
        /// Create a signing request data
        /// </summary>
        /// <param name="dn">The distinguished name of the certificate</param>
        /// <param name="keyPair">The private key to generate the CSR for</param>
        /// <returns>The CMC signing request</returns>
        /// <param name="alternateNames">The laternate names to use</param>
        /// <param name="enhancedUsages">The enhanced uses of the key</param>
        /// <param name="usageFlags">The usage flags</param>
        byte[] CreateSigningRequest(RSAParameters keyPair, X500DistinguishedName dn, X509KeyUsageFlags usageFlags = X509KeyUsageFlags.None, String[] enhancedUsages = null, String[] alternateNames = null);

        /// <summary>
        /// Creates a self-signed certificate 
        /// </summary>
        /// <param name="dn">The distinguished name of the certificate</param>
        /// <param name="usageFlags">The intended use of the certificate</param>
        /// <param name="validityPeriod">The validity period</param>
        /// <param name="keyPair">The private/public key pair</param>
        /// <returns>The generated self-signed certificate</returns>
        /// <param name="enhancedUsages">The enhanced uses of the key</param>
        /// <param name="alternateNames">The subject alternate names</param>
        /// <param name="friendlyName">The friendly name of the certificate</param>
        X509Certificate2 CreateSelfSignedCertificate(RSAParameters keyPair, X500DistinguishedName dn, TimeSpan validityPeriod, X509KeyUsageFlags usageFlags = X509KeyUsageFlags.None, String[] enhancedUsages = null, String[] alternateNames = null, String friendlyName = null);

        /// <summary>
        /// Combines the <paramref name="certificate"/> with the <paramref name="keyParameters"/> 
        /// </summary>
        /// <param name="certificate">The certificate which was obtained from the upstream certificate store</param>
        /// <param name="keyParameters">The private key which matches the certificate</param>
        /// <param name="friendlyName">The friendly name for the output cert</param>
        /// <returns>The converted certificate</returns>
        X509Certificate2 Combine(X509Certificate2 certificate, RSAParameters keyParameters, string friendlyName = null);

    }
}
