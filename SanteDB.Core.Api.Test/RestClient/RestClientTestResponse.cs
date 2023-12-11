using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test.RestClient
{
    internal class RestClientTestResponse
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string ContentType { get; set; }
        public NameValueCollection Query { get; set; }
        public Dictionary<string, List<string>> RequestHeaders { get; set; } = new Dictionary<string, List<string>>();
    }

    internal class RestClientTestResponse<TBody> : RestClientTestResponse
    {
        public TBody Body { get; set; }
        

    }
}
