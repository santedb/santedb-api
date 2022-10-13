using Newtonsoft.Json;
using SanteDB.Core.Http.Compression;
using SanteDB.Core.Http.Description;
using System;
using System.Collections.Generic;
using System.IO;
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
        SignatureMismatch
    }
    /// <summary>
    /// Web signature data
    /// </summary>
    public class JsonWebSignature
    {

        // JWS format regex
        private static readonly Regex m_jwsFormat = new Regex(@"^((.*?)\.(.*?))\.(.*?)$");

        // The data signing service
        private readonly IDataSigningService m_dataSigningService;

        // The parsed token
        private string m_token;

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
        public static JsonWebSignature Create(dynamic payload, IDataSigningService dataSigningService)
        {
            return new JsonWebSignature(dataSigningService)
            {
                Payload = payload,
                Header = new { }
            };
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
            if(!jwsMatch.Success)
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
            parsedWebSignature.Header = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(headerBytes));

            // First, validate the signature
            var keyId = parsedWebSignature.Header.kid?.ToString();
            var alg = parsedWebSignature.Header.alg?.ToString();
            var result = JsonWebSignatureParseResult.Success;
            if (String.IsNullOrEmpty(keyId))
            {
                result = JsonWebSignatureParseResult.MissingKeyId;
            }
            else if (String.IsNullOrEmpty(alg))
            {
                result = JsonWebSignatureParseResult.MissingAlgorithm;
            }
            else if (!alg.Equals(dataSigningService.GetSignatureAlgorithm(keyId)))
            {
                result = JsonWebSignatureParseResult.AlgorithmAndKeyMismatch;
            }
            else if (!dataSigningService.Verify(Encoding.UTF8.GetBytes(jwsMatch.Groups[1].Value), signatureBytes, keyId))
            {
                result = JsonWebSignatureParseResult.SignatureMismatch;
            }

            // Continue to parse the data
            using(var ms = new MemoryStream(bodyBytes)) 
            using(var compressionStream = parsedWebSignature.GetJwsCompressor().CreateDecompressionStream(ms))
            using(var textReader = new StreamReader(compressionStream))
            using(var jsonReader = new JsonTextReader(textReader))
            {
                parsedWebSignature.Payload = JsonSerializer.Create().Deserialize(jsonReader);
            }

            return result;

        }

        /// <summary>
        /// Get the header data
        /// </summary>
        public dynamic Header { get; private set;  }

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
            if (this.Header.zip == null)
            {
                switch(scheme)
                {
                    case HttpCompressionAlgorithm.Deflate:
                        this.Header.zip = "DEF";
                        break;
                    case HttpCompressionAlgorithm.Gzip:
                        this.Header.zip = "GZ";
                        break;
                    case HttpCompressionAlgorithm.Bzip2:
                        this.Header.zip = "BZ2";
                        break;
                    case HttpCompressionAlgorithm.Lzma:
                        this.Header.zip = "LZ7";
                        break;
                }
            }
            return this;
        }

        /// <summary>
        /// JSON web signature data with key
        /// </summary>
        public JsonWebSignature WithKey(String keyId)
        {
            if (this.Header.alg == null)
            {
                this.Header.alg = this.m_dataSigningService.GetSignatureAlgorithm(keyId);
                this.Header.kid = this.m_dataSigningService.GetPublicKeyIdentifier(keyId);
            }
            return this;
        }

        /// <summary>
        /// With type
        /// </summary>
        public JsonWebSignature WithType(String type)
        {
            if(this.Header.typ == null)
            {
                this.Header.typ = type;
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
            using(var ms = new MemoryStream())
            {
                ICompressionScheme scheme = this.GetJwsCompressor();
                

                using(var compressStream = scheme.CreateCompressionStream(ms))
                using (var textWriter = new StreamWriter(compressStream))
                using(var jsonWriter = new JsonTextWriter(textWriter))
                {
                    JsonSerializer.Create().Serialize(jsonWriter, this.Payload);
                }

                retVal.AppendFormat(".{0}", ms.ToArray().Base64UrlEncode());
            }

            this.Signature = this.m_dataSigningService.SignData(Encoding.UTF8.GetBytes(retVal.ToString()), this.Header.kid?.ToString());
            retVal.AppendFormat(".{0}", this.Signature.Base64UrlEncode());
            this.m_token= retVal.ToString();
            return this;
        }

        /// <summary>
        /// Get the JWS compressor
        /// </summary>
        private ICompressionScheme GetJwsCompressor()
        {
            switch (this.Header.zip.ToString())
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
