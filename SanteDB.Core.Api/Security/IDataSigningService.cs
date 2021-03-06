﻿/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security
{

    /// <summary>
    /// Represents a service which can sign arbitrary data 
    /// </summary>
    public interface IDataSigningService : IServiceImplementation
    {

        /// <summary>
        /// Get the keys identifiers registered for the signature service
        /// </summary>
        IEnumerable<String> GetKeys();

        /// <summary>
        /// Get the siganture algorithm this service would use to sign w/the specified key
        /// </summary>
        string GetSignatureAlgorithm(String keyId = null);

        /// <summary>
        /// Register a key with the provider
        /// </summary>
        /// <param name="keyId">The key identifier to register</param>
        /// <param name="keyData">The key data (passphrase, or any other structured key data)</param>
        /// <param name="signatureAlgorithm">The signature algorithm</param>
        void AddSigningKey(string keyId, byte[] keyData, String signatureAlgorithm);

        /// <summary>
        /// Signs the specified data using the service's configured signing key
        /// </summary>
        /// <param name="data">The data to be signed</param>
        /// <param name="keyId">The numeric identifier of the key to use</param>
        /// <returns>The digital signature</returns>
        byte[] SignData(byte[] data, string keyId = null);

        /// <summary>
        /// Verifies the digital signature of the data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature">The digital signature to be verified</param>
        /// <param name="keyId">The identifier of the key to use for verification</param>
        /// <returns>True if the signature is valid</returns>
        bool Verify(byte[] data, byte[] signature, string keyId = null);
    }
}
