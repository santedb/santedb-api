using SanteDB.Core.Configuration;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Data signing service that uses HMAC256
    /// </summary>
    public class HmacDataSigningService : IDataSigningService
    {
        /// <summary>
        /// Keys for the signing of data
        /// </summary>
        private ConcurrentDictionary<String, SecuritySignatureConfiguration> m_keyData = new ConcurrentDictionary<string, SecuritySignatureConfiguration>();

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "HS256 Data Signature Service";

        /// <summary>
        /// Get whether the signature is symmetric
        /// </summary>
        public bool IsSymmetric => true;

        // Configuration
        private SecurityConfigurationSection m_configuration;

        /// <summary>
        /// Create HS256 signer
        /// </summary>
        public HmacDataSigningService(IConfigurationManager configurationManager)
        {
            this.m_configuration = configurationManager.GetSection<SecurityConfigurationSection>();
            // Key data
            this.m_keyData = new ConcurrentDictionary<string, SecuritySignatureConfiguration>(this.m_configuration.Signatures.ToDictionary(o => o.KeyName, o => o));
        }

        /// <summary>
        /// Try to get the specified key
        /// </summary>
        private bool TryGetKey(String key, out SecuritySignatureConfiguration config)
        {
            if (!this.m_keyData.TryGetValue(key, out config))
            {
                config = this.m_configuration?.Signatures.FirstOrDefault(k => k.KeyName == key);
                if (config != null)
                {
                    this.m_keyData.TryAdd(config.KeyName, config);
                    return true;
                }
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Add a signing key
        /// </summary>
        public void AddSigningKey(string keyId, byte[] keyData, string signatureAlgorithm)
        {
            if (!this.m_keyData.ContainsKey(keyId))
            {
                var keyConfig = new SecuritySignatureConfiguration()
                {
                    KeyName = keyId,
                    Algorithm = (SignatureAlgorithm)Enum.Parse(typeof(SignatureAlgorithm), signatureAlgorithm),
                    FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint,
                    FindValue = signatureAlgorithm != "HS256" ? BitConverter.ToString(keyData).Replace("-", "") : null,
                    StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                    StoreName = System.Security.Cryptography.X509Certificates.StoreName.My,
                    FindTypeSpecified = signatureAlgorithm != "HS256",
                    StoreLocationSpecified = signatureAlgorithm != "HS256",
                    StoreNameSpecified = signatureAlgorithm != "HS256"
                };

                if (signatureAlgorithm == "HS256")
                    keyConfig.SetSecret(keyData);

                this.m_keyData.TryAdd(keyId, keyConfig);
            }
        }

        /// <summary>
        /// Get all keys
        /// </summary>
        public IEnumerable<string> GetKeys()
        {
            // Force keys to load
            var configuredKeys = this.m_configuration?.Signatures;
            configuredKeys.ForEach(o => this.TryGetKey(o.KeyName, out SecuritySignatureConfiguration _));
            return this.m_keyData.Keys;
        }

        /// <summary>
        /// Gets the signature algorithm
        /// </summary>
        public string GetSignatureAlgorithm(string keyId = null)
        {
            if (this.TryGetKey(keyId ?? "default", out SecuritySignatureConfiguration config))
                return config.Algorithm.ToString();
            return null;
        }

        /// <summary>
        /// Sign the specified data 
        /// </summary>
        public byte[] SignData(byte[] data, string keyId = null)
        {
            // Fetch the key from the repository
            if (!this.TryGetKey(keyId ?? "default", out SecuritySignatureConfiguration configuration))
                throw new InvalidOperationException($"Key {keyId ?? "default"} not found");

            switch (configuration.Algorithm)
            {
                case SignatureAlgorithm.HS256:
                    {
                        var key = configuration.GetSecret();
                        // Ensure 128 bit
                        while (key.Length < 16)
                            key = key.Concat(key).ToArray();

                        var hmac = new System.Security.Cryptography.HMACSHA256(key);
                        return hmac.ComputeHash(data);
                    }
                case SignatureAlgorithm.RS256:
                case SignatureAlgorithm.RS512:
                    {

                        if (!configuration.Certificate.HasPrivateKey)
                            throw new InvalidOperationException("You must have the private key to sign data with this certificate");
                        var csp = (RSACryptoServiceProvider)configuration.Certificate.PrivateKey;
                        RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(csp);
                        formatter.SetHashAlgorithm(configuration.Algorithm == SignatureAlgorithm.RS256 ? "SHA256" : "SHA512");
                        return formatter.CreateSignature(data);
                    }
                default:
                    throw new InvalidOperationException("Cannot generate digital signature");
            }
        }

        /// <summary>
        /// Verify the input data against the specified signature
        /// </summary>
        public bool Verify(byte[] data, byte[] signature, string keyId = null)
        {
            // Fetch the key from the repository
            if (!this.TryGetKey(keyId ?? "default", out SecuritySignatureConfiguration configuration))
                throw new InvalidOperationException($"Key {keyId ?? "default"} not found");

            switch (configuration.Algorithm)
            {
                case SignatureAlgorithm.HS256:
                    {
                        var key = configuration.GetSecret();
                        // Ensure 128 bit
                        while (key.Length < 16)
                            key = key.Concat(key).ToArray();

                        var hmac = new System.Security.Cryptography.HMACSHA256(key);
                        return hmac.ComputeHash(data).SequenceEqual(signature);
                    }
                case SignatureAlgorithm.RS256:
                case SignatureAlgorithm.RS512:
                    {
                        var csp = (RSACryptoServiceProvider)configuration.Certificate.PublicKey.Key;
                        RSAPKCS1SignatureDeformatter formatter = new RSAPKCS1SignatureDeformatter(csp);
                        formatter.SetHashAlgorithm(configuration.Algorithm == SignatureAlgorithm.RS256 ? "SHA256" : "SHA512");
                        // Compute SHA hash
                        HashAlgorithm sha = configuration.Algorithm == SignatureAlgorithm.RS256 ? (HashAlgorithm)SHA256.Create() : (HashAlgorithm)SHA512.Create();
                        var hashValue = sha.ComputeHash(data);
                        return formatter.VerifySignature(hashValue, signature);
                    }
                default:
                    throw new InvalidOperationException("Cannot generate digital signature");
            }
        }
    }
}
