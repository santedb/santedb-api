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
 * Date: 2022-7-25
 */
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Implementation of a session token resolver service which produces a simple pointer to a session identifier
    /// </summary>
    public class SimpleSessionTokenResolver : ISessionTokenResolverService
    {

        private readonly ISessionProviderService m_sessionTokenProvider;
        private readonly IDataSigningService m_dataSigningService;
        private readonly ILocalizationService m_localizationService;

        /// <summary>
        /// DI constructor
        /// </summary>
        public SimpleSessionTokenResolver(ISessionProviderService providerService, IDataSigningService dataSigningService, ILocalizationService localizationService)
        {
            this.m_sessionTokenProvider = providerService;
            this.m_dataSigningService = dataSigningService;
            this.m_localizationService = localizationService;
        }

        /// <summary>
        /// The service name
        /// </summary>
        public string ServiceName => "Simple Session Token Resolver";

        /// <summary>
        /// Resolve the 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ISession Resolve(string token)
        {
            var authenticationTokenParts = token.Split('.').Select(o => o.HexDecode()).ToArray();
            // Validate signature?
            if(authenticationTokenParts.Length > 1 && !this.m_dataSigningService.Verify(authenticationTokenParts[0], authenticationTokenParts[1]))
            {
                throw new SecurityException(this.m_localizationService.GetString(ErrorMessageStrings.SIGNATURE_INVALID));
            }

            return this.m_sessionTokenProvider.Get(authenticationTokenParts[0]);
                
        }

        /// <summary>
        /// Serialize a session to a token
        /// </summary>
        public string Serialize(ISession session)
        {
            var signature = this.m_dataSigningService.SignData(session.Id);
            return $"{session.Id.HexEncode()}.{signature.HexEncode()}";
        }
    }
}
