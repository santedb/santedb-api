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
using SanteDB.Core.Services;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a crypto service provider that encrypts things using symmetric encryption
    /// </summary>
    [System.ComponentModel.Description("Symmetric Encryption Provider")]
    public interface ISymmetricCryptographicProvider : IServiceImplementation
    {

        /// <summary>
        /// Generates an initialization vector
        /// </summary>
        byte[] GenerateIV();

        /// <summary>
        /// Generates a key
        /// </summary>
        byte[] GenerateKey();

        /// <summary>
        /// Gets the context key
        /// </summary>
        byte[] GetContextKey();

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
    }
}
