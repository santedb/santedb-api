using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System.Linq;
using System.Security;

namespace SanteDB.Core.Security
{
    internal class SimpleSessionTokenEncodingService : ISessionTokenEncodingService
    {
        readonly IDataSigningService _DataSigningService;
        readonly ILocalizationService _LocalizationService;
        readonly Tracer _TraceSource;

        public SimpleSessionTokenEncodingService(IDataSigningService dataSigningService, ILocalizationService localizationService)
        {
            _TraceSource = new Tracer(SanteDBConstants.ServiceTraceSourceName);
            _DataSigningService = dataSigningService;
            _LocalizationService = localizationService;
        }

        public string ServiceName => "Simple Session Token Encoding Service";

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
            var signature = _DataSigningService.SignData(token);
            return $"{token.HexEncode()}.{signature.HexEncode()}";
        }

        public bool TryDecode(string encodedToken, out byte[] token)
        {
            if (string.IsNullOrWhiteSpace(encodedToken))
            {
                _TraceSource.TraceVerbose("SimpleSessionTokenEncodingService - Empty token passed to TryDecode().");
                token = null;
                return false;
            }

            var tokenparts = encodedToken.Split('.').Select(o => o.HexDecode()).ToArray();

            if (tokenparts.Length != 2)
            {
                _TraceSource.TraceVerbose("SimpleSessionTokenEncodingService - Malformed token passed to TryDecode().");
                token = null;
                return false;
            }

            if (!_DataSigningService.Verify(tokenparts[0], tokenparts[1]))
            {
                _TraceSource.TraceVerbose("SimpleSessionTokenEncodingService - Validation failed in TryDecode().");
                token = null;
                return false;
            }

            token = tokenparts[0];
            return true;


        }
    }
}
