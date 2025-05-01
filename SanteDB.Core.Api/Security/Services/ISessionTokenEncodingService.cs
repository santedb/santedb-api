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

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Service which can encode session id and refresh tokens into an opaque format suitable to be roundtripped through an untrusted context
    /// </summary>
    public interface ISessionTokenEncodingService : IServiceImplementation
    {
        /// <summary>
        /// Encodes a token such that the result is an opaque value that is tamper resistant and suitable for transport through an unsecure context.
        /// </summary>
        /// <param name="token">A token to encode</param>
        /// <returns>A string containing the encoded token.</returns>
        string Encode(byte[] token);
        /// <summary>
        /// Attempts to decode a token. Will return a decoded token in <paramref name="token"/> if the result is true, <c>null</c> otherwise.
        /// </summary>
        /// <param name="encodedToken">An encoded token that was generated through <see cref="Encode(byte[])"/>.</param>
        /// <param name="token">The resulting decoded token when decoding is successful.</param>
        /// <returns><c>true</c> When decoding succeeds, false otherwise.</returns>
        bool TryDecode(string encodedToken, out byte[] token);
        /// <summary>
        /// Attempts to decode a token. Will return the decoded token or throw an exception if the encoded token is invalid.
        /// </summary>
        /// <param name="encodedToken"></param>
        /// <returns></returns>
        byte[] Decode(string encodedToken);

        /// <summary>
        /// Extract the identifier of the session from the encoded token without performing validation
        /// </summary>
        /// <param name="encodedToken">The encoded token</param>
        /// <returns>The extracted session identity</returns>
        /// <remarks>Allows unconfigured instances to extract the identifier portion of the access token</remarks>
        byte[] ExtractSessionIdentity(string encodedToken);
    }
}
