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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Linq;
using System.Security;

namespace SanteDB.Core.Security
{
    internal class SimpleSessionTokenEncodingService : ISessionTokenEncodingService
    {
        protected readonly IDataSigningService _DataSigningService;
        readonly ILocalizationService _LocalizationService;
        readonly Tracer _TraceSource;

        public SimpleSessionTokenEncodingService(IDataSigningService dataSigningService, ILocalizationService localizationService)
        {
            _TraceSource = new Tracer(SanteDBConstants.ServiceTraceSourceName);
            _DataSigningService = dataSigningService;
            _LocalizationService = localizationService;
        }

        public virtual string ServiceName => "Simple Session Token Encoding Service";

        public byte[] Decode(string encodedToken)
        {
            if (TryDecode(encodedToken, out var result))
            {
                return result;
            }

            _TraceSource.TraceInfo("SimpleSessionTokenEncodingService - Decoding failed for token.");

            throw new SecurityException(_LocalizationService.GetString(ErrorMessageStrings.SIGNATURE_INVALID));
        }

        public string Encode(byte[] token)
        {

            return $"{token.Base64UrlEncode()}.{this.EncodeSignatureBytes(token).Base64UrlEncode()}";
        }

        /// <summary>
        /// Compute the signature for <paramref name="token"/>
        /// </summary>
        protected virtual byte[] EncodeSignatureBytes(byte[] token) => _DataSigningService.SignData(token);

        /// <summary>
        /// Parse the signature bytes
        /// </summary>
        protected virtual byte[] DecodeSignatureBytes(byte[] signature) => signature;

        public bool TryDecode(string encodedToken, out byte[] token)
        {
            if (string.IsNullOrWhiteSpace(encodedToken))
            {
                _TraceSource.TraceVerbose("SimpleSessionTokenEncodingService - Empty token passed to TryDecode().");
                token = null;
                return false;
            }

            var tokenparts = encodedToken.Split('.').Select(o => o.ParseBase64UrlEncode()).ToArray();

            if (tokenparts.Length != 2)
            {
                _TraceSource.TraceVerbose("SimpleSessionTokenEncodingService - Malformed token passed to TryDecode().");
                token = null;
                return false;
            }

            if (!_DataSigningService.Verify(tokenparts[0], this.DecodeSignatureBytes(tokenparts[1])))
            {
                _TraceSource.TraceVerbose("SimpleSessionTokenEncodingService - Validation failed in TryDecode().");
                token = null;
                return false;
            }

            token = tokenparts[0];
            return true;


        }

        /// <inheritdoc/>
        public byte[] ExtractSessionIdentity(string encodedToken)
        {
            if (string.IsNullOrWhiteSpace(encodedToken))
            {
                throw new ArgumentNullException(nameof(encodedToken));
            }

            var tokenparts = encodedToken.Split('.').Select(o => o.ParseBase64UrlEncode()).ToArray();

            if (tokenparts.Length != 2)
            {
                throw new ArgumentException("Format of the Token is invalid.", nameof(encodedToken));
            }

            return tokenparts[0];
        }
    }
}
