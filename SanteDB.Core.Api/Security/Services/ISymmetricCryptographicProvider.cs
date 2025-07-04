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
using SanteDB.Core.Services;
using System.IO;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a crypto service provider that encrypts things using symmetric encryption
    /// </summary>
    [System.ComponentModel.Description("Symmetric Encryption Provider")]
    public interface ISymmetricCryptographicProvider : IServiceImplementation
    {
        /// <summary>
        /// Gets the size of the IV this algorithm uses
        /// </summary>
        int IVSize { get; }

        /// <summary>
        /// Gets the context key
        /// </summary>
        /// <remarks>The context key is used by the symmetric encryption algorithm and is used to quickly encrypt data to/from the configuration file
        /// (particularly the HMAC keys) when a RSA certificate is not available for encryption</remarks>
        byte[] GetContextKey();

        /// <summary>
        /// Instructs the symmetric provider to rotate the key storage. 
        /// </summary>
        /// <remarks>This method is called to notify the encryption provider that a new security certificate configuration has been applied, and that
        /// the provider should re-persist the context key with the newly configured certificate (typically the <c>default</c> key)</remarks>
        /// <returns>True if the key was successfully rotated</returns>
        bool RotateContextKey();

        /// <summary>
        /// Generates an initialization vector
        /// </summary>
        byte[] GenerateIV();

        /// <summary>
        /// Generates a key
        /// </summary>
        byte[] GenerateKey();

        /// <summary>
        /// Encrypts the sepcified data
        /// </summary>
        byte[] Encrypt(byte[] data, byte[] key, byte[] iv);

        /// <summary>
        /// Decrypts the specified data
        /// </summary>
        byte[] Decrypt(byte[] data, byte[] key, byte[] iv);

        /// <summary>
        /// Encrypt <paramref name="data"/> with the IV embedded in the return
        /// </summary>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypt <paramref name="data"/> with the IV embedded in the return
        /// </summary>
        byte[] Decrypt(byte[] data);

        /// <summary>
        /// Encrypt the string data and return a Base64 encoded version
        /// </summary>
        string Encrypt(string data);

        /// <summary>
        /// Decrypt the string data and return a Base64 encoded version
        /// </summary>
        string Decrypt(string data);

        /// <summary>
        /// Create a decrypting stream
        /// </summary>
        /// <param name="underlyingStream">The underlying stream to wrap</param>
        /// <param name="key">The key to encrypt the stream</param>
        /// <param name="iv">The initialization vector</param>
        /// <returns>The wrapped stream</returns>
        Stream CreateEncryptingStream(Stream underlyingStream, byte[] key, byte[] iv);

        /// <summary>
        /// Create a decrypting stream
        /// </summary>
        /// <param name="underlyingStream">The underlying stream to wrap</param>
        /// <param name="key">The key to decrypt the stream</param>
        /// <param name="iv">The initialization vector</param>
        /// <returns>The wrapped stream</returns>
        Stream CreateDecryptingStream(Stream underlyingStream, byte[] key, byte[] iv);
    }
}
