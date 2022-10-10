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
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Security.Configuration
{
    /// <summary>
    /// Represents the type of signature algorithms
    /// </summary>
    [XmlType(nameof(SignatureAlgorithm), Namespace = "http://santedb.org/configuration")]
    public enum SignatureAlgorithm
    {
        /// <summary>
        /// The desired signature algorithm is RSA+SHA256 (i.e. an X.509 cert)
        /// </summary>
        [XmlEnum("rs256")]
        RS256,

        /// <summary>
        /// The desired signature algorithm is HMAC256
        /// </summary>
        [XmlEnum("hmac")]
        HS256,

        /// <summary>
        /// The desired signature algorithm is RSA+SHA512
        /// </summary>
        [XmlEnum("rs512")]
        RS512
    }

    /// <summary>
    /// Represents a signature collection
    /// </summary>
    [XmlType(nameof(SecuritySignatureConfiguration), Namespace = "http://santedb.org/configuration")]
    public class SecuritySignatureConfiguration : X509ConfigurationElement
    {
        // Algorithm
        private SignatureAlgorithm m_algorithm = SignatureAlgorithm.HS256;

        // When true don't disclose secrets
        private bool m_forDisclosure = false;

        // HMAC key
        private string m_plainTextSecret = null;

        private byte[] m_secret = null;
        private byte[] m_decrypedSecret = null;

        /// <summary>
        /// Gets or sets the key name
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        [DisplayName("Key ID")]
        [Description("The identifier for the signature key")]
        public string KeyName { get; set; }

        /// <summary>
        /// The unique name for the signer
        /// </summary>
        [XmlAttribute("iss"), JsonProperty("iss")]
        [DisplayName("Issuer")]
        [Description("The name of the signature authority this represents")]
        public string IssuerName { get; set; }

        /// <summary>
        /// Signature mode
        /// </summary>
        [XmlAttribute("alg"), JsonProperty("alg")]
        [DisplayName("Signing Algorithm")]
        [Description("The type of signature algorithm to use")]
        public SignatureAlgorithm Algorithm
        {
            get => this.m_algorithm;
            set
            {
                this.m_algorithm = value;
                this.FindTypeSpecified = this.StoreLocationSpecified = this.StoreNameSpecified = this.m_algorithm == SignatureAlgorithm.RS256;
                if (value == SignatureAlgorithm.HS256)
                {
                    this.FindValue = null;
                }
                else
                {
                    this.HmacSecret = null;
                }
            }
        }

        /// <summary>
        /// When using HMAC256 signing this represents the server's secret
        /// </summary>
        [XmlAttribute("hmacKey"), JsonProperty("hmacKey")]
        [DisplayName("HMAC256 Key")]
        [ReadOnly(true)]
        public byte[] Secret
        {
            get
            {
                return this.m_secret;
            }
            set => this.m_secret = value;
        }

        /// <summary>
        /// Should serialize the secret
        /// </summary>
        public bool ShouldSerializeSecret() => !this.m_hideSecrets;

        /// <summary>
        /// Plaintext editor for secret
        /// </summary>
        [XmlAttribute("hmacSecret"), JsonProperty("hmacSecret")]
        [Description("When using HS256 signing the secret to use")]
        [DisplayName("HMAC256 Secret")]
        [PasswordPropertyText(true)]
        public string HmacSecret
        {
            get => this.m_plainTextSecret;
            set
            {
                this.m_secret = null;
                this.m_plainTextSecret = value;

            }
        }

        /// <summary>
        /// Should serialize the secret
        /// </summary>
        public bool ShouldSerializeHmacSecret() => this.m_secret == null && !this.m_forDisclosure;

        /// <summary>
        /// Get the HMAC secret
        /// </summary>
        public byte[] GetSecret()
        {
            if (this.m_decrypedSecret != null)
            {
                return this.m_decrypedSecret;
            }

            if (this.Secret == null)
            {
                // Perhaps the plain text secret is set?
                if (!String.IsNullOrEmpty(this.m_plainTextSecret))
                {
                    this.SetSecret(Encoding.UTF8.GetBytes(this.m_plainTextSecret));
                }
                else
                {
                    return null;
                }
            }

            if (this.m_decrypedSecret == null)
            {
                var cryptoService = ApplicationServiceContext.Current.GetService<ISymmetricCryptographicProvider>();
                var ivLength = this.Secret[0];
                var iv = this.Secret.Skip(1).Take(ivLength).ToArray();
                var data = this.Secret.Skip(1 + ivLength).ToArray();
                this.m_decrypedSecret = cryptoService.Decrypt(data, cryptoService.GetContextKey(), iv);
            }
            return this.m_decrypedSecret;

        }

        /// <summary>
        /// Set the secret
        /// </summary>
        public bool SetSecret(byte[] secret)
        {
            this.m_decrypedSecret = secret;

            var cryptoService = ApplicationServiceContext.Current?.GetService<ISymmetricCryptographicProvider>();
            if (cryptoService == null)
            {
                return false;
            }

            var iv = cryptoService.GenerateIV();
            var key = cryptoService.GetContextKey();

            var data = cryptoService.Encrypt(secret, key, iv);

            this.m_secret = new byte[data.Length + iv.Length + 1];
            this.m_secret[0] = (byte)iv.Length;
            Array.Copy(iv, 0, this.m_secret, 1, iv.Length);
            Array.Copy(data, 0, this.m_secret, 1 + iv.Length, data.Length);
            //this.m_plainTextSecret = String.Empty;
            return true;
        }

        /// <summary>
        /// Represent as a string
        /// </summary>
        public override string ToString() => this.KeyName;

        public SecuritySignatureConfiguration ForDisclosure() => new SecuritySignatureConfiguration()
        {
            Algorithm = this.Algorithm,
            Certificate = this.Certificate,
            FindType = this.FindType,
            FindTypeSpecified = this.FindTypeSpecified,
            FindValue = this.FindValue,
            HmacSecret = this.HmacSecret,
            IssuerName = this.IssuerName,
            KeyName = this.KeyName,
            Secret = this.Secret,
            StoreLocation = this.StoreLocation,
            StoreLocationSpecified = this.StoreLocationSpecified,
            StoreName = this.StoreName,
            StoreNameSpecified = this.StoreNameSpecified,
            ValidationOnly = this.ValidationOnly,
            m_forDisclosure = true
        };
    }
}