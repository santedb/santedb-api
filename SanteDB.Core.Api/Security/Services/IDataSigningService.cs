/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.i18n;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Services;
using System;
using System.Security.Cryptography.X509Certificates;
using ZstdSharp;

namespace SanteDB.Core.Security.Services
{

    /// <summary>
    /// Signature settings
    /// </summary>
    public class SignatureSettings
    {

        /// <summary>
        /// Private ctor
        /// </summary>
        private SignatureSettings(SignatureAlgorithm algorithm,
            byte[] rawKeyData,
            X509Certificate2 certificate)
        {
            this.Algorithm = algorithm;
            this.RawKeyData = rawKeyData;
            this.Certificate = certificate;
        }

        /// <summary>
        /// Create HS256 settings with the specified <paramref name="key"/>
        /// </summary>
        public static SignatureSettings HS256(byte[] key) => new SignatureSettings(SignatureAlgorithm.HS256, key, null);

        /// <summary>
        /// Create RS256 settings with specified <paramref name="certificate"/>
        /// </summary>
        /// <returns></returns>
        public static SignatureSettings RSA(SignatureAlgorithm algorithm, X509Certificate2 certificate)
        {
            switch (algorithm)
            {
                case SignatureAlgorithm.RS256:
                case SignatureAlgorithm.RS512:
                    return new SignatureSettings(algorithm, null, certificate);
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm));
            }
        }

        /// <summary>
        /// Create signature settings from configuration
        /// </summary>
        public static SignatureSettings FromConfiguration(SecuritySignatureConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            else if(configuration.Certificate?.NotAfter < DateTimeOffset.Now || configuration.Certificate?.NotBefore > DateTimeOffset.Now)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.CERTIFICATE_TIME_CONSTRAINT, configuration.Certificate));
            }
            else
            {
                return new SignatureSettings(configuration.Algorithm, configuration.GetSecret(), configuration.Certificate);
            }
        }
        /// <summary>
        /// Get the algorithm
        /// </summary>
        public SignatureAlgorithm Algorithm { get; }

        /// <summary>
        /// Get the raw key data
        /// </summary>
        public byte[] RawKeyData { get; }

        /// <summary>
        /// Get the x509 certificate
        /// </summary>
        public X509Certificate2 Certificate { get; }

    }

    /// <summary>
    /// Contract for services which can sign data using configured digital signature algorithms
    /// </summary>
    /// <remarks>
    /// <para>Implementers of this service contract are responsible for computing and validating
    /// digital signatures against arbitrary data streams. Implementers of this service are responsible for 
    /// maintaining (or acquiring) a master list of keys which can be used for data signing, and validating 
    /// digital signatures.</para>
    /// <para>Implementers should also use the <see cref="IDataSigningCertificateManagerService"/> to support key identifiers which are indicated as a 
    /// secure application/device identifier</para>
    /// </remarks>
    [System.ComponentModel.Description("Data Signing Service")]
    public interface IDataSigningService : IServiceImplementation
    {

        /// <summary>
        /// Get the siganture algorithm for the system configured key
        /// </summary>
        SignatureSettings GetNamedSignatureSettings(string systemKeyId);

        /// <summary>
        /// Get the signature algorithm for the configured thumbprint
        /// </summary>
        SignatureSettings GetSignatureSettings(byte[] certificateThumbprint, SignatureAlgorithm signatureAlgorithm = SignatureAlgorithm.RS256);

        /// <summary>
        /// Sign <paramref name="data"/> with the configured system key <paramref name="systemKeyId"/>
        /// </summary>
        /// <param name="data">The data to be signed</param>
        /// <param name="systemKeyId">The system key to use</param>
        /// <returns>The signed data</returns>
        byte[] SignData(byte[] data, string systemKeyId = null);

        /// <summary>
        /// Signs the specified data using the service's configured signing key
        /// </summary>
        /// <param name="data">The data to be signed</param>
        /// <param name="settings">The signature configuration to use</param>
        /// <returns>The digital signature</returns>
        byte[] SignData(byte[] data, SignatureSettings settings);

        /// <summary>
        /// Verifies the digital signature of the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature">The digital signature to be verified</param>
        /// <param name="systemKeyId">The identifier of the key to use for verification</param>
        /// <returns>True if the signature is valid</returns>
        bool Verify(byte[] data, byte[] signature, string systemKeyId = null);

        /// <summary>
        /// Verifies the digital signature of the data with a provided configuration
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature">The digital signature to be verified</param>
        /// <param name="settings">The configuration to use</param>
        /// <returns>True if the signature is valid</returns>
        bool Verify(byte[] data, byte[] signature, SignatureSettings settings);

    }
}