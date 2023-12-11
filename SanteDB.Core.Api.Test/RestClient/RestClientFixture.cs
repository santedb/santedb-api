using SanteDB.Core.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test.RestClient
{
    internal class RestClientFixture : Core.Http.RestClient
    {
        protected override WebRequest CreateHttpRequest(string url, NameValueCollection query)
        {
            return base.CreateHttpRequest(url, query);
        }
    }
}
