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
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a symmetric cryptographic provider based on AES
    /// </summary>
    public class AesSymmetricCrypographicProvider : ISymmetricCryptographicProvider
    {
        internal const int IV_SIZE = 16;

        // Context key
        private byte[] m_contextKey;

        // Configuration
        private SecurityConfigurationSection m_configuration;

        /// <summary>
        /// AES Symmetric crypto provider
        /// </summary>
        public AesSymmetricCrypographicProvider(IConfigurationManager configurationManager)
        {
            this.m_configuration = configurationManager.GetSection<SecurityConfigurationSection>();
        }

        /// <summary>
        /// Service name
        /// </summary>
        public String ServiceName => "AES Symmetric Cryptographic Provider";

        private Aes CreateAlgorithm() => Aes.Create();

        /// <summary>
        /// Decrypt the specified data using the specified key and iv
        /// </summary>
        public byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            key = this.CorrectKey(key);
            using (var aes = CreateAlgorithm())
            {
                var decryptor = aes.CreateDecryptor(key, iv);
                return decryptor.TransformFinalBlock(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Ensure the key is the right length
        /// </summary>
        private byte[] CorrectKey(byte[] key)
        {
            byte[] retVal = new byte[key.Length * 3];
            for (int i = 0; i < 32; i += key.Length)
            {
                Array.Copy(key, 0, retVal, i, key.Length);
            }

            return retVal.Take(32).ToArray();
        }

        /// <summary>
        /// Encrypt the specified data using the specified key and iv
        /// </summary>
        public byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            key = this.CorrectKey(key);
            using (var aes = CreateAlgorithm())
            {
                var encryptor = aes.CreateEncryptor(key, iv);
                var retVal = encryptor.TransformFinalBlock(data, 0, data.Length);
                return retVal;
            }
        }

        /// <summary>
        /// Generate a random IV
        /// </summary>
        public byte[] GenerateIV()
        {
            using (var aes = CreateAlgorithm())
            {
                aes.GenerateIV();
                return aes.IV;
            }
        }

        /// <summary>
        /// Generate key
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateKey()
        {
            using (var aes = CreateAlgorithm())
            {
                aes.GenerateKey();
                return aes.Key;
            }
        }

        /// <summary>
        /// Get the context default key
        /// </summary>
        /// <remarks>
        /// This method is used during the decryption of secrests in the configuration. This could result in a stack overflow
        /// due to this method attempting to get secrets, and the configuration decrypting them. Care should be taken if this method is updated.
        /// </remarks>
        public byte[] GetContextKey()
        {
            // TODO: Is it possible to pull from CPU?
            if (this.m_contextKey == null)
            {
                var defaultKey = this.m_configuration.Signatures.FirstOrDefault(o => String.IsNullOrEmpty(o.KeyName) || o.KeyName == "default");

                if(defaultKey.Algorithm == SignatureAlgorithm.HS256)
                {
                    this.m_contextKey = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(defaultKey?.HmacSecret ?? throw new NotSupportedException("Default key is of type HMAC but does not have a secret set.") /*"DEFAULTKEY"*/));
                }
                else
                {
                    this.m_contextKey = this.ReadContextKey(defaultKey);
                }
            }
            return this.m_contextKey;
        }

        /// <summary>
        /// Read the context key which is stored in the DataDirectory encrypted on disk
        /// </summary>
        private byte[] ReadContextKey(SecuritySignatureConfiguration key)
        {
            var contextKeyFile = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "ctxkey.enc");
            if(!File.Exists(contextKeyFile))
            {
                //TODO: Don't use the D parameter as it is not always equal when exported and reimported. https://github.com/dotnet/runtime/commit/700a07cae19fe64649c2fb4c6c10e6b9aa85dc29
                var keyData = SHA256.Create().ComputeHash(key.Certificate.GetRSAPrivateKey().ExportParameters(true).D);
                using(var fs = File.Create(contextKeyFile))
                {
                    var buffer = key.Certificate.GetRSAPublicKey().Encrypt(keyData, RSAEncryptionPadding.Pkcs1);
                    fs.Write(buffer, 0, buffer.Length);
                }
                return keyData;
            }
            else
            {
                using(var fs = File.OpenRead(contextKeyFile))
                {
                    var buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            }
        }

        /// <inheritdoc/>    
        public string Encrypt(string plainText)
        {
            return this.Encrypt(Encoding.UTF8.GetBytes(plainText)).Base64UrlEncode();
        }

        /// <inheritdoc/>    
        public string Decrypt(string cipherText)
        {
            return Encoding.UTF8.GetString(this.Decrypt(cipherText.ParseBase64UrlEncode()));
        }

        /// <inheritdoc/>    
        public byte[] Encrypt(byte[] plainText)
        {
            var key = this.GetContextKey();
            var iv = new byte[IV_SIZE];
            System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(iv);

            var ciphertext = this.Encrypt(plainText, key, iv);

            var result = new byte[IV_SIZE + ciphertext.Length];

            Buffer.BlockCopy(iv, 0, result, 0, IV_SIZE);
            Buffer.BlockCopy(ciphertext, 0, result, IV_SIZE, ciphertext.Length);

            return result;
        }

        /// <inheritdoc/>    
        public byte[] Decrypt(byte[] cipherText)
        {

            var key = this.GetContextKey();
            var iv = new byte[IV_SIZE];
            var encrypteddata = new byte[cipherText.Length - IV_SIZE];
            Buffer.BlockCopy(cipherText, 0, iv, 0, IV_SIZE);
            Buffer.BlockCopy(cipherText, IV_SIZE, encrypteddata, 0, encrypteddata.Length);

            return this.Decrypt(encrypteddata, key, iv);
        }

        /// <inheritdoc/>
        public Stream CreateEncryptingStream(Stream underlyingStream, byte[] key, byte[] iv)
        {
            return new CryptoStream(underlyingStream, this.CreateAlgorithm().CreateEncryptor(key, iv), CryptoStreamMode.Write);
        }

        /// <inheritdoc/>
        public Stream CreateDecryptingStream(Stream underlyingStream, byte[] key, byte[] iv)
        {
            return new CryptoStream(underlyingStream, this.CreateAlgorithm().CreateDecryptor(key, iv), CryptoStreamMode.Read);

        }
    }
}