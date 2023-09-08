using SanteDB.Core.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test.RestClient
{
    internal class TestCredentialProvider : ICredentialProvider
    {
        public TestCredentialProvider()
        {

        }

        public TestCredentialProvider(RestRequestCredentials credentials)
        {
            Credentials = credentials;
        }

        public RestRequestCredentials Credentials { get; set; }
        public IRestClient RestClient { get; set; }
        public IPrincipal Principal { get; set; }

        public RestRequestCredentials GetCredentials(IRestClient context)
        {
            RestClient = context;
            return Credentials;
        }

        public RestRequestCredentials GetCredentials(IPrincipal principal)
        {
            Principal = principal;
            return Credentials;
        }
    }
}
