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
using Newtonsoft.Json;
using SanteDB.Core.Security.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        private byte[] m_secret = null;

        /// <summary>
        /// Security configuration section
        /// </summary>
        public SecuritySignatureConfiguration()
        {

        }

        /// <summary>
        /// Create configuration with HMAC secret
        /// </summary>
        public SecuritySignatureConfiguration(String name, String secret)
        {
            this.KeyName = name;
            this.HmacSecret = secret;
            this.Algorithm = SignatureAlgorithm.HS256;
        }

        /// <summary>
        /// Security signature with certificates
        /// </summary>
        public SecuritySignatureConfiguration(String name, StoreLocation storeLocation, StoreName storeName, X509Certificate2 certificate)
            : base(storeLocation, storeName, X509FindType.FindByThumbprint, certificate.Thumbprint)
        {
            this.KeyName = name;
            this.Algorithm = SignatureAlgorithm.RS256;
        }

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
            get;
            set;
        }


        /// <summary>
        /// Plaintext editor for secret
        /// </summary>
        [XmlAttribute("hmacSecret"), JsonProperty("hmacSecret")]
        [Description("When using HS256 signing the secret to use")]
        [DisplayName("HMAC256 Secret")]
        [PasswordPropertyText(true)]
        public string HmacSecret
        {
            get;
            set;
        }

        /// <summary>
        /// Get the HMAC secret
        /// </summary>
        public byte[] GetSecret()
        {
            // Determine if the secret was provided in configuration as a binary value or as a plaintext value
            if(String.IsNullOrEmpty(this.HmacSecret) && this.Secret == null)
            {
                return null;
            } 
            else if(this.Secret != null)
            {
                return this.Secret;
            }
            else if (this.m_secret == null)
            {
                this.m_secret = Encoding.UTF8.GetBytes(this.HmacSecret);
                return this.m_secret;
            }
            else 
            {
                return this.m_secret;
            }
        }

        /// <summary>
        /// Represent as a string
        /// </summary>
        public override string ToString() => this.KeyName;

        /// <summary>
        /// Modify the security signature collection for disclosure to a REST API
        /// </summary>
        /// <returns>The created configuration</returns>
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
            StoreLocation = this.StoreLocation,
            StoreLocationSpecified = this.StoreLocationSpecified,
            StoreName = this.StoreName,
            StoreNameSpecified = this.StoreNameSpecified,
            ValidationOnly = this.ValidationOnly,
            m_forDisclosure = true
        };
    }
}