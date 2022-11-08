/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Security.Certs;
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
        private readonly ConcurrentDictionary<String, SecuritySignatureConfiguration> m_usedForSignature = new ConcurrentDictionary<string, SecuritySignatureConfiguration>();

        /// <summary>
        /// Default data signing service DI constructor
        /// </summary>
        public DefaultDataSigningService(IConfigurationManager configurationManager)
        {
            this.m_configuration = configurationManager.GetSection<SecurityConfigurationSection>();

        }

        /// <summary>
        /// Get the service name
        /// </summary>
        public string ServiceName => "Default Data Signing Service";

        /// <summary>
        /// Add a signing key to the global configuration for signing
        /// </summary>
        public void AddSigningKey(string keyId, byte[] keyData, SignatureAlgorithm signatureAlgorithm)
        {
            var current = this.m_configuration.Signatures.Find(o => o.KeyName == keyId);
            if (current == null) // No current key - add it
            {
                current = new SecuritySignatureConfiguration()
                {
                    KeyName = keyId,
                };
                this.m_configuration.Signatures.Add(current);
            }
            current.Algorithm = signatureAlgorithm;
            current.FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint;
            current.FindTypeSpecified = signatureAlgorithm != SignatureAlgorithm.HS256;
            current.FindValue = signatureAlgorithm != SignatureAlgorithm.HS256 ? BitConverter.ToString(keyData).Replace("-", "") : null;
            current.Secret = signatureAlgorithm == SignatureAlgorithm.HS256 ? keyData : null;
            current.StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine;
            current.StoreLocationSpecified = signatureAlgorithm != SignatureAlgorithm.HS256;
            current.StoreName = System.Security.Cryptography.X509Certificates.StoreName.My;
            current.StoreNameSpecified = signatureAlgorithm != SignatureAlgorithm.HS256;
        }

        /// <summary>
        /// Get all keys
        /// </summary>
        public IEnumerable<string> GetKeys()
        {
            return this.m_configuration.Signatures.Select(o => o.KeyName);
        }

        /// <summary>
        /// Get the signature algorithm for the specified key
        /// </summary>
        /// <param name="keyId">The key identifier</param>
        public SignatureAlgorithm? GetSignatureAlgorithm(string keyId = null) => this.m_configuration.Signatures.Find(o => o.KeyName == (keyId ?? "default"))?.Algorithm;

        /// <summary>
        /// Get the public key identifier for the object
        /// </summary>
        public string GetPublicKeyIdentifier(string keyId = null) => this.m_configuration.Signatures.Find(o => o.KeyName == (keyId ?? "default"))?.Certificate?.Thumbprint ?? keyId;

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

            // Sign the data
            switch (configuration.Algorithm)
            {
                case SignatureAlgorithm.HS256:
                    {
                        var key = configuration.GetSecret();
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
            if(!this.m_usedForSignature.TryGetValue(keyId, out var keyUsedForSigning))
            {
                keyUsedForSigning = this.m_configuration.Signatures.Find(o => o.KeyName == keyId);
                this.m_usedForSignature.TryAdd(keyId, keyUsedForSigning);
            }
            return keyUsedForSigning;
        }

        /// <summary>
        /// Verify data
        /// </summary>
        /// <param name="data">The data to verify</param>
        /// <param name="signature">The signature to validate</param>
        /// <param name="keyId">The key to use</param>
        /// <returns>True if the signature matches</returns>
        public bool Verify(byte[] data, byte[] signature, string keyId = null)
        {
            var configuration = this.GetSigningKey(keyId ?? "default");
            if (configuration == null)
            {
                throw new KeyNotFoundException($"Could not find signing credentials {keyId}");
            }

            // Configuration algorithm
            switch (configuration.Algorithm)
            {
                case SignatureAlgorithm.HS256:
                    {
                        var key = configuration.GetSecret();
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
    }
}