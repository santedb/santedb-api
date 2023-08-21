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
using SanteDB.Core.i18n;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Default data signing service
    /// </summary>
    /// <remarks>
    /// <para>This digital signature service uses the keys configured in the <see cref="SecurityConfigurationSection"/>
    /// to sign data based on the type of signature algorithm in the <see cref="SecurityConfigurationSection"/>. Supported signature 
    /// algorithms are:</para>
    /// <list type="bullet">
    ///     <item>HMAC256 (HMAC + SHA256) using shared secrets</item>
    ///     <item>RS256 (RSA+SHA256) using X.509 certificates (generation of a signature requires private key)</item>
    ///     <item>RS512 (RSA+SHA512)</item>
    /// </list>
    /// </remarks>
    public class DefaultDataSigningService : IDataSigningService
    {

        // Security configuration
        private readonly SecurityConfigurationSection m_configuration;
        private readonly IDataSigningCertificateManagerService m_certificateManager;
        private readonly ConcurrentDictionary<String, SecuritySignatureConfiguration> m_usedForSignature = new ConcurrentDictionary<string, SecuritySignatureConfiguration>();

        /// <summary>
        /// Default data signing service DI constructor
        /// </summary>
        public DefaultDataSigningService(IConfigurationManager configurationManager, IDataSigningCertificateManagerService dataSigningCertificateManagerService = null)
        {
            this.m_configuration = configurationManager.GetSection<SecurityConfigurationSection>();
            this.m_certificateManager = dataSigningCertificateManagerService;
        }

        /// <summary>
        /// Get the service name
        /// </summary>
        public string ServiceName => "Default Data Signing Service";

        /// <summary>
        /// Sign data with the specified key data
        /// </summary>
        public byte[] SignData(byte[] data, string keyId = null)
        {
            var configuration = this.GetSigningKey(keyId ?? "default");
            if (configuration == null)
            {
                throw new KeyNotFoundException($"Signing credentials {keyId} not found");
            }
            return this.SignData(data, SignatureSettings.FromConfiguration(configuration));
        }

        /// <inheritdoc/>
        public byte[] SignData(byte[] data, SignatureSettings configuration)
        {
            // Sign the data
            switch (configuration.Algorithm)
            {
                case SignatureAlgorithm.HS256:
                    {
                        var key = configuration.RawKeyData;
                        // Ensure 128 bit
                        while (key.Length < 16)
                        {
                            key = key.Concat(key).ToArray();
                        }

                        var hmac = new System.Security.Cryptography.HMACSHA256(key);
                        return hmac.ComputeHash(data);
                    }
                case SignatureAlgorithm.RS256:
                case SignatureAlgorithm.RS512:
                    {
                        if (!configuration.Certificate.HasPrivateKey)
                        {
                            throw new InvalidOperationException("You must have the private key to sign data with this certificate");
                        }

                        using (var csp = configuration.Certificate.GetRSAPrivateKey())
                        {
                            var halgname = configuration.Algorithm == SignatureAlgorithm.RS256 ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA512;
                            var halg = HashAlgorithm.Create(halgname.Name);
                            var hashtext = halg.ComputeHash(data);
                            return csp.SignHash(hashtext, halgname, RSASignaturePadding.Pkcs1);
                        }
                    }
                default:
                    throw new InvalidOperationException($"Cannot generate digital signature {configuration.Algorithm}");
            }
        }

        /// <summary>
        /// Get signing key configuration - this is used as on initial startup the configuration can change
        /// </summary>
        private SecuritySignatureConfiguration GetSigningKey(string keyId)
        {
            if (!this.m_usedForSignature.TryGetValue(keyId, out var keyUsedForSigning))
            {
                keyUsedForSigning = this.m_configuration.Signatures.Find(o => o.KeyName == keyId);
                this.m_usedForSignature.TryAdd(keyId, keyUsedForSigning);
            }
            return keyUsedForSigning;
        }

        /// <inheritdoc/>
        public bool Verify(byte[] data, byte[] signature, string keyId = null)
        {
            var configuration = this.GetSigningKey(keyId ?? "default");
            if (configuration == null)
            {
                throw new KeyNotFoundException($"Could not find signing credentials {keyId}");
            }
            return this.Verify(data, signature, SignatureSettings.FromConfiguration(configuration));
        }

        /// <inheritdoc/>
        public bool Verify(byte[] data, byte[] signature, SignatureSettings configuration)
        {
            // Configuration algorithm
            switch (configuration.Algorithm)
            {
                case SignatureAlgorithm.HS256:
                    {
                        var key = configuration.RawKeyData;
                        // Ensure 128 bit
                        while (key.Length < 16)
                        {
                            key = key.Concat(key).ToArray();
                        }

                        var hmac = new System.Security.Cryptography.HMACSHA256(key);
                        return hmac.ComputeHash(data).SequenceEqual(signature);
                    }
                case SignatureAlgorithm.RS256:
                case SignatureAlgorithm.RS512:
                    {
                        var csp = System.Security.Cryptography.X509Certificates.RSACertificateExtensions.GetRSAPublicKey(configuration.Certificate);
                        var halgname = configuration.Algorithm == SignatureAlgorithm.RS256 ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA512;
                        var halg = HashAlgorithm.Create(halgname.Name);
                        var hashtext = halg.ComputeHash(data);
                        return csp.VerifyHash(hashtext, signature, halgname, RSASignaturePadding.Pkcs1);
                    }
                default:
                    throw new InvalidOperationException("Cannot validate digital signature");
            }
        }

        /// <summary>
        /// Get siganture settings from the named system key
        /// </summary>
        public SignatureSettings GetNamedSignatureSettings(string systemKeyId)
        {
            if (String.IsNullOrEmpty(systemKeyId))
            {
                throw new ArgumentNullException(nameof(systemKeyId));
            }
            return SignatureSettings.FromConfiguration(this.m_configuration.Signatures.Find(o => o.KeyName == systemKeyId || o.Certificate?.Thumbprint == systemKeyId));
        }

        /// <summary>
        /// Get signature settings from a certificate thumbprint
        /// </summary>
        public SignatureSettings GetSignatureSettings(byte[] certificateThumbprint, SignatureAlgorithm signatureAlgorithm = SignatureAlgorithm.RS256)
        {
            if (certificateThumbprint == null)
            {
                throw new ArgumentNullException(nameof(certificateThumbprint));
            }
            // First - check for system configured
            var candidate = this.m_configuration.Signatures.Find(o => o.FindType == X509FindType.FindByThumbprint && o.FindValue?.Equals(certificateThumbprint.HexEncode(), StringComparison.OrdinalIgnoreCase) == true);
            if (candidate != null)
            {
                return SignatureSettings.FromConfiguration(candidate);
            }
            else if (X509CertificateUtils.GetPlatformServiceOrDefault().TryGetCertificate(X509FindType.FindByThumbprint, certificateThumbprint, out var certificate))
            {
                return SignatureSettings.RSA(signatureAlgorithm, certificate);
            }
            else if (this.m_certificateManager.TryGetSigningCertificateByHash(certificateThumbprint, out certificate))
            {
                return SignatureSettings.RSA(signatureAlgorithm, certificate);
            }
            else
            {
                throw new KeyNotFoundException(ErrorMessages.CERTIFICATE_NOT_FOUND);
            }
        }
    }
}