/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Services;
using System;
using System.Security;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Implementation of a session token resolver service which produces a simple pointer to a session identifier
    /// </summary>
    public class SimpleSessionTokenResolver : ISessionTokenResolverService
    {

        private readonly ISessionProviderService m_SessionProvider;
        private readonly ISessionTokenEncodingService m_sessionTokenEncoder;
        private readonly ILocalizationService m_localizationService;

        private readonly Tracer m_traceSource = new Tracer(SanteDBConstants.ServiceTraceSourceName);

        /// <summary>
        /// DI constructor
        /// </summary>
        public SimpleSessionTokenResolver(ISessionProviderService providerService, ISessionTokenEncodingService tokenEncoder, ILocalizationService localizationService)
        {
            this.m_SessionProvider = providerService;
            this.m_sessionTokenEncoder = tokenEncoder;
            this.m_localizationService = localizationService;
        }

        /// <summary>
        /// The service name
        /// </summary>
        public string ServiceName => "Simple Session Token Resolver";

        /// <inheritdoc cref="ISessionTokenResolverService.GetEncodedIdToken(ISession)"/>
        public string GetEncodedIdToken(ISession session)
        {
            if (null == session)
            {
                throw new ArgumentNullException(nameof(session), m_localizationService.GetString(ErrorMessageStrings.ARGUMENT_NULL));
            }

            return m_sessionTokenEncoder.Encode(session.Id);
        }

        /// <inheritdoc cref="ISessionTokenResolverService.GetEncodedRefreshToken(ISession)"/>
        public string GetEncodedRefreshToken(ISession session)
        {
            if (null == session)
            {
                throw new ArgumentNullException(nameof(session), m_localizationService.GetString(ErrorMessageStrings.ARGUMENT_NULL));
            }

            return m_sessionTokenEncoder.Encode(session.RefreshToken);
        }

        /// <inheritdoc cref="ISessionTokenResolverService.GetSessionFromBearerToken(string)"/>
        /// <exception cref="SecurityException"></exception>
        public ISession GetSessionFromBearerToken(string encodedToken)
        {
            if (m_sessionTokenEncoder.TryDecode(encodedToken, out var sessionId))
            {
                return m_SessionProvider.Get(sessionId);
            }

            throw new SecurityException(m_localizationService.GetString(ErrorMessageStrings.SESSION_TOKEN_INVALID));
        }

        /// <inheritdoc cref="ISessionTokenResolverService.ExtendSessionWithRefreshToken(string)"/>
        /// <exception cref="SecurityException"></exception>
        public ISession ExtendSessionWithRefreshToken(string encodedToken)
        {
            if (m_sessionTokenEncoder.TryDecode(encodedToken, out var refreshToken))
            {
                return m_SessionProvider.Extend(refreshToken); //TODO: Need to have a GetByRefreshToken on the AdoSessionProvider impl or rename to ExtendWithRefreshToken for semantic equality.
            }

            throw new SecurityException(m_localizationService.GetString(ErrorMessageStrings.SESSION_TOKEN_INVALID)); //TODO: Need to have a new error message string for this.
        }
    }
}
