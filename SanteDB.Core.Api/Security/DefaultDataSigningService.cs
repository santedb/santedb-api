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
using SanteDB.Core.Diagnostics;
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
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultDataSigningService));

        /// <summary>
        /// Default data signing service DI constructor
        /// </summary>
        public DefaultDataSigningService(IConfigurationManager configurationManager, IDataSigningCertificateManagerService dataSigningCertificateManagerService = null)
        {
            this.m_configuration = configurationManager.GetSection<SecurityConfigurationSection>();
            this.m_certificateManager = dataSigningCertificateManagerService;

            if (this.m_certificateManager != null)
            {
                this.InstallConfiguredCertificatesToManager();
            }
        }

        /// <summary>
        /// Install configured certificates into the certificate manager
        /// </summary>
        private void InstallConfiguredCertificatesToManager()
        {
            try
            {
                foreach (var certConfig in this.m_configuration.Signatures.Where(o => o.Algorithm != SignatureAlgorithm.HS256))
                {
                    if (!this.m_certificateManager.GetCertificateIdentities(certConfig.Certificate).Any(i => AuthenticationContext.SystemPrincipal.Identity.Name.Equals(i.Name)))
                    {
                        this.m_certificateManager.AddSigningCertificate(AuthenticationContext.SystemPrincipal.Identity, certConfig.Certificate, AuthenticationContext.SystemPrincipal);
                    }
                }
            }
            catch (Exception e)
            {
                this.m_tracer.TraceWarning("Could not install certificates to data signing repository");
            }
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
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.m_tracer.TraceInfo("Signing {0} bytes of data using algorithm {1} key {2}",
                data.Length, configuration.Algorithm, configuration.RawKeyData?.Length.ToString() ?? configuration.Certificate?.Thumbprint);
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
                        if (configuration.Certificate?.HasPrivateKey != true)
                        {
                            throw new InvalidOperationException("You must have the private key to sign data with this certificate");
                        }

                        using (var csp = configuration.Certificate.GetRSAPrivateKey())
                        {
                            if (csp == null)
                            {
                                throw new InvalidOperationException("Cannot sign data - no private key cryptographic service provider present");
                            }

                            var halgname = configuration.Algorithm == SignatureAlgorithm.RS256 ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA512;
                            HashAlgorithm halg = configuration.Algorithm == SignatureAlgorithm.RS256 ? (HashAlgorithm)SHA256.Create() : (HashAlgorithm)SHA512.Create();
                            if (halg == null)
                            {
                                throw new NotSupportedException($"Hash algorithm {configuration.Algorithm} is not supported on this platform");
                            }
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
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            else if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            else if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

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
                        using (var csp = System.Security.Cryptography.X509Certificates.RSACertificateExtensions.GetRSAPublicKey(configuration.Certificate))
                        {
                            if (csp == null)
                            {
                                throw new InvalidOperationException("Cannot sign data - no private key cryptographic service provider present");
                            }
                            
                            var halgname = configuration.Algorithm == SignatureAlgorithm.RS256 ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA512;
                            HashAlgorithm halg = configuration.Algorithm == SignatureAlgorithm.RS256 ? (HashAlgorithm)SHA256.Create() : (HashAlgorithm)SHA512.Create();
                            if (halg == null)
                            {
                                throw new NotSupportedException($"Hash algorithm {configuration.Algorithm} is not supported on this platform");
                            }
                            var hashtext = halg.ComputeHash(data);
                            return csp.VerifyHash(hashtext, signature, halgname, RSASignaturePadding.Pkcs1);
                        }
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
                return null;
            }
        }
    }
}