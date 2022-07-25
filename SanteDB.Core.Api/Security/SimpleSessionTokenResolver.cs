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
