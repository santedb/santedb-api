using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test.RestClient
{
    internal class RequestResponse
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public string Host { get; set; }
        public string RequestContentType { get; set; }
        public string RequestContent { get; set; }
        public Dictionary<string, string> RequestHeaders = new Dictionary<string, string>();
        public string QueryString { get; set; }

        public RequestResponse()
        {

        }
        public RequestResponse(HttpListenerRequest request)
        {
            Path = request.Url.AbsolutePath;
            Method = request.HttpMethod;
            Host = request.Headers["Host"];
            RequestContentType = request.Headers["Content-Type"];
            QueryString = request.Url.Query;
            foreach(var header in request.Headers.AllKeys)
            {
                RequestHeaders.Add(header, request.Headers[header]);
            }
        }
    }
}
