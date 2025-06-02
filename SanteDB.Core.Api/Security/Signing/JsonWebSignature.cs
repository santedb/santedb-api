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
using Newtonsoft.Json;
using SanteDB.Core.Http.Compression;
using SanteDB.Core.Http.Description;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Services;
using System;
using System.Dynamic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace SanteDB.Core.Security.Signing
{
    /// <summary>
    /// Results of parsing a JWS
    /// </summary>
    public enum JsonWebSignatureParseResult
    {
        /// <summary>
        /// Success
        /// </summary>
        Success,
        /// <summary>
        /// Invalid format of token
        /// </summary>
        InvalidFormat,
        /// <summary>
        /// Missing alg header
        /// </summary>
        MissingAlgorithm,
        /// <summary>
        /// Missing key identifier
        /// </summary>
        MissingKeyId,
        /// <summary>
        /// KID cannot be used to validate ALG
        /// </summary>
        AlgorithmAndKeyMismatch,
        /// <summary>
        /// Signature doesn't match
        /// </summary>
        SignatureMismatch,
        /// <summary>
        /// Algorithm is not supported
        /// </summary>
        UnsupportedAlgorithm
    }

    /// <summary>
    /// JSON Web signature header
    /// </summary>
    public class JsonWebSignatureHeader
    {
        /// <summary>
        /// Algorithm for signature
        /// </summary>
        [JsonProperty("alg")]
        public String Algorithm { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [JsonProperty("typ")]
        public String Type { get; set; }

        /// <summary>
        /// Get identifier
        /// </summary>
        [JsonProperty("kid")]
        public String KeyId { get; set; }

        /// <summary>
        /// Get identifier
        /// </summary>
        [JsonProperty("x5t")]
        public String KeyThumbprint { get; set; }

        /// <summary>
        /// Compression algorithm
        /// </summary>
        [JsonProperty("zip")]
        public String Zip { get; set; }

        /// <summary>
        /// Content type
        /// </summary>
        [JsonProperty("cty")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets the issuer of the JWT
        /// </summary>
        [JsonProperty("iss")]
        public string Issuer { get; set; }
    }
    /// <summary>
    /// Web signature data
    /// </summary>
    public class JsonWebSignature
    {


        // JWS format regex
        private static readonly Regex m_jwsFormat = new Regex(@"^((.*?)\.(.*?))\.(.*?)$", RegexOptions.Compiled);

        // The data signing service
        private readonly IDataSigningService m_dataSigningService;

        // The parsed token
        private string m_token;
        private SignatureSettings m_signingSettings;

        /// <summary>
        /// Create a new web signature with the specified data
        /// </summary>
        private JsonWebSignature(IDataSigningService dataSigningService)
        {
            this.m_dataSigningService = dataSigningService;
        }

        /// <summary>
        /// Create a new json web signature structure with the specified payload
        /// </summary>
        /// <param name="payload">The payload to set</param>
        /// <param name="dataSigningService">The data signing service to use</param>
        public static JsonWebSignature Create(object payload, IDataSigningService dataSigningService)
        {
            return new JsonWebSignature(dataSigningService)
            {
                Payload = payload,
                Header = new JsonWebSignatureHeader()
            };
        }

        /// <summary>
        /// Parse the specified <paramref name="detachedSignature"/> and return the appropriate <see cref="JsonWebSignature"/>
        /// </summary>
        /// <param name="detachedSignature">The detached signature to parse</param>
        /// <param name="parsedWebSignature">The parsed web signature</param>
        /// <returns>The result of the parse</returns>
        public static JsonWebSignatureParseResult TryParseDetached(String detachedSignature, out JsonWebSignature parsedWebSignature)
        {
            var jwsMatch = m_jwsFormat.Match(detachedSignature);
            if (!jwsMatch.Success)
            {
                parsedWebSignature = null;
                return JsonWebSignatureParseResult.InvalidFormat;
            }

            parsedWebSignature = new JsonWebSignature(null);
            parsedWebSignature.m_token = detachedSignature;

            // Get the parts of the header
            byte[] headerBytes = jwsMatch.Groups[2].Value.ParseBase64UrlEncode(),
                signatureBytes = jwsMatch.Groups[4].Value.ParseBase64UrlEncode();

            // Now lets parse the JSON objects
            parsedWebSignature.Header = JsonConvert.DeserializeObject<JsonWebSignatureHeader>(Encoding.UTF8.GetString(headerBytes));
            parsedWebSignature.Signature = signatureBytes;
            return JsonWebSignatureParseResult.Success;
        }

        /// <summary>
        /// Parse the specified <paramref name="webSignature"/> and create the <see cref="JsonWebSignature"/>
        /// </summary>
        /// <param name="webSignature">The web signature to parse</param>
        /// <param name="dataSigningService">The signing service to use </param>
        /// <param name="parsedWebSignature">The parsed web signature</param>
        /// <returns>True if the parse was successful and the signature on the web token is valid</returns>
        public static JsonWebSignatureParseResult TryParse(String webSignature, IDataSigningService dataSigningService, out JsonWebSignature parsedWebSignature)
        {
            var jwsMatch = m_jwsFormat.Match(webSignature);
            if (!jwsMatch.Success)
            {
                parsedWebSignature = null;
                return JsonWebSignatureParseResult.InvalidFormat;
            }

            parsedWebSignature = new JsonWebSignature(dataSigningService);
            parsedWebSignature.m_token = webSignature;

            // Get the parts of the header
            byte[] headerBytes = jwsMatch.Groups[2].Value.ParseBase64UrlEncode(),
                bodyBytes = jwsMatch.Groups[3].Value.ParseBase64UrlEncode(),
                signatureBytes = jwsMatch.Groups[4].Value.ParseBase64UrlEncode();
            
            // Now lets parse the JSON objects
            parsedWebSignature.Header = JsonConvert.DeserializeObject<JsonWebSignatureHeader>(Encoding.UTF8.GetString(headerBytes));
            parsedWebSignature.Signature = signatureBytes;

            // Parse the data
            using (var ms = new MemoryStream(bodyBytes))
            using (var compressionStream = parsedWebSignature.GetJwsCompressor().CreateDecompressionStream(ms))
            using (var textReader = new StreamReader(compressionStream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                parsedWebSignature.Payload = JsonSerializer.Create().Deserialize<ExpandoObject>(jsonReader);
            }

            // First, validate the signature
            if (!dataSigningService.TryGetSignatureSettings(parsedWebSignature.Header, out var signatureSettings))
            {
                return JsonWebSignatureParseResult.MissingKeyId;
            }

            var alg = parsedWebSignature.Header.Algorithm?.ToString();
            var result = JsonWebSignatureParseResult.Success;
            if (String.IsNullOrEmpty(alg))
            {
                result = JsonWebSignatureParseResult.MissingAlgorithm;
            }
            else if (!Enum.TryParse<SignatureAlgorithm>(alg, true, out var signatureAlgorithm))
            {
                result = JsonWebSignatureParseResult.UnsupportedAlgorithm;
            }
            else if (signatureSettings.Algorithm != signatureAlgorithm)
            {
                result = JsonWebSignatureParseResult.AlgorithmAndKeyMismatch;
            }
            else if (!dataSigningService.Verify(Encoding.UTF8.GetBytes(jwsMatch.Groups[1].Value), signatureBytes, signatureSettings))
            {
                result = JsonWebSignatureParseResult.SignatureMismatch;
            }

            return result;

        }

        /// <summary>
        /// Get the header data
        /// </summary>
        public JsonWebSignatureHeader Header { get; private set; }

        /// <summary>
        /// Get the payload
        /// </summary>
        public dynamic Payload { get; private set; }

        /// <summary>
        /// Gets the signature of the data
        /// </summary>
        public byte[] Signature { get; private set; }

        /// <summary>
        /// Get the token
        /// </summary>
        public String Token
        {
            get => this.m_token ?? this.AsSigned().Token;
        }


        /// <summary>
        /// Compress the JWS
        /// </summary>
        public JsonWebSignature WithCompression(HttpCompressionAlgorithm scheme)
        {
            if (this.Header.Zip == null)
            {
                switch (scheme)
                {
                    case HttpCompressionAlgorithm.Deflate:
                        this.Header.Zip = "DEF";
                        break;
                    case HttpCompressionAlgorithm.Gzip:
                        this.Header.Zip = "GZ";
                        break;
                    case HttpCompressionAlgorithm.Bzip2:
                        this.Header.Zip = "BZ2";
                        break;
                    case HttpCompressionAlgorithm.Lzma:
                        this.Header.Zip = "LZ7";
                        break;
                }
            }
            return this;
        }

        /// <summary>
        /// JSON web signature data with thumbprint
        /// </summary>
        public JsonWebSignature WithCertificate(X509Certificate2 certificate)
        {
            if (String.IsNullOrEmpty(this.Header.KeyThumbprint))
            {
                this.Header.Algorithm = "RS256";
                this.Header.KeyId = certificate.Thumbprint;
                this.Header.KeyThumbprint = certificate.GetCertHash().Base64UrlEncode();
                this.m_signingSettings = SignatureSettings.RSA(SignatureAlgorithm.RS256, certificate);
            }
            return this;
        }

        /// <summary>
        /// JSON web signature data with a named key
        /// </summary>
        public JsonWebSignature WithSystemKey(String keyId)
        {
            if (String.IsNullOrEmpty(this.Header.KeyId))
            {
                this.m_signingSettings = this.m_dataSigningService.GetNamedSignatureSettings(keyId);
                this.Header.Algorithm = this.m_signingSettings.Algorithm.ToString();
                if (this.m_signingSettings.Algorithm == SignatureAlgorithm.HS256)
                {
                    this.Header.KeyId = keyId;
                }
                else if (this.m_signingSettings.Certificate != null)
                {
                    this.WithCertificate(this.m_signingSettings.Certificate);
                }
            }
            return this;
        }

        /// <summary>
        /// With type
        /// </summary>
        public JsonWebSignature WithType(String type)
        {
            if (String.IsNullOrEmpty(this.Header.Type))
            {
                this.Header.Type = type;
            }
            return this;
        }

        /// <summary>
        /// With type
        /// </summary>
        public JsonWebSignature WithContentType(String type)
        {
            if (String.IsNullOrEmpty(this.Header.ContentType))
            {
                this.Header.ContentType = type;
            }
            return this;
        }

        /// <summary>
        /// Append the issuer information to the web signature
        /// </summary>
        public JsonWebSignature WithIssuer(String issuer)
        {
            if(String.IsNullOrEmpty(this.Header.Issuer))
            {
                this.Header.Issuer = issuer;
            }
            return this;
        }

        /// <summary>
        /// Create the JSON web token
        /// </summary>
        public JsonWebSignature AsSigned()
        {
            StringBuilder retVal = new StringBuilder();
            retVal.Append(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.Header)).Base64UrlEncode());

            // Is the data compressed?
            using (var ms = new MemoryStream())
            {
                ICompressionScheme scheme = this.GetJwsCompressor();


                using (var compressStream = scheme.CreateCompressionStream(ms))
                using (var textWriter = new StreamWriter(compressStream))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    JsonSerializer.Create().Serialize(jsonWriter, this.Payload);
                }

                retVal.AppendFormat(".{0}", ms.ToArray().Base64UrlEncode());
            }

            this.Signature = this.m_dataSigningService.SignData(Encoding.UTF8.GetBytes(retVal.ToString()), this.m_signingSettings);
            retVal.AppendFormat(".{0}", this.Signature.Base64UrlEncode());
            this.m_token = retVal.ToString();
            return this;
        }

        /// <summary>
        /// Get the JWS compressor
        /// </summary>
        private ICompressionScheme GetJwsCompressor()
        {
            switch (this.Header.Zip?.ToString())
            {
                case "DEF":
                    return CompressionUtil.GetCompressionScheme(HttpCompressionAlgorithm.Deflate);
                case "GZ":
                    return CompressionUtil.GetCompressionScheme(HttpCompressionAlgorithm.Gzip);
                case "BZ2":
                    return CompressionUtil.GetCompressionScheme(HttpCompressionAlgorithm.Bzip2);
                case "LZ7":
                    return CompressionUtil.GetCompressionScheme(HttpCompressionAlgorithm.Lzma);
                default:
                    return CompressionUtil.GetCompressionScheme(HttpCompressionAlgorithm.None);
            }
        }
    }
}
