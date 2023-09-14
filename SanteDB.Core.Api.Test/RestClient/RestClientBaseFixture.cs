using Newtonsoft.Json;
using SanteDB.Core.Http;
using SanteDB.Core.Http.Description;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test.RestClient
{
    internal class RestClientBaseFixture : RestClientBase
    {
        public RestClientBaseFixture() : base() { }
        public RestClientBaseFixture(IRestClientDescription configuration): base(configuration) { } 

        protected override TResult InvokeInternal<TBody, TResult>(string method, string url, string contentType, WebHeaderCollection requestHeaders, out WebHeaderCollection responseHeaders, TBody body, NameValueCollection query)
        {
            var tresult = typeof(TResult);

            if (tresult == typeof(RestClientTestResponse) || tresult == typeof(RestClientTestResponse<TBody>)) 
            {
                var response = new RestClientTestResponse<TBody>()
                {
                    Body = body,
                    ContentType = contentType,
                    Method = method,
                    Query = query,
                    Url = url
                };

                foreach(var header in requestHeaders.AllKeys)
                {
                    var values = requestHeaders.GetValues(header);
                    response.RequestHeaders.Add(header, values.ToList());
                }

                responseHeaders = CreateResponseHeaders(contentType);

                return (TResult)(object)response;
            }
            else if (typeof(TBody) == typeof(byte[]) && tresult == typeof(byte[]))
            {
                var response = new RestClientTestResponse<TBody>()
                {
                    Body = body,
                    ContentType = contentType,
                    Method = method,
                    Query = query,
                    Url = url
                };

                

                foreach (var header in requestHeaders.AllKeys)
                {
                    var values = requestHeaders.GetValues(header);
                    response.RequestHeaders.Add(header, values.ToList());
                }

                responseHeaders = CreateResponseHeaders(contentType);

                return (TResult)(object)Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            }
            else
            {
                throw new ArgumentException("Invalid TResult type. Must be RestClientResponse<TBody>");
            }
        }

        private WebHeaderCollection CreateResponseHeaders(string contentType)
        {
            var headers = new WebHeaderCollection
            {
                { HttpResponseHeader.Server, nameof(RestClientBaseFixture) },
                { HttpResponseHeader.Date, DateTimeOffset.UtcNow.ToString("u") },
                { HttpResponseHeader.ContentType, contentType }
            };

            return headers;
        }


        public new Uri CreateCorrectRequestUri(string resourceNameOrUrl, NameValueCollection query)
        {
            return base.CreateCorrectRequestUri(resourceNameOrUrl, query);
        }


        public new WebRequest CreateHttpRequest(string resourceNameOrUrl, NameValueCollection query)
        {
            return base.CreateHttpRequest(resourceNameOrUrl, query);
        }


        public new RestRequestCredentials GetRequestCredentials()
        {
            return base.GetRequestCredentials();
        }


        public new void SetRequestAcceptEncoding(HttpWebRequest webrequest)
        {
            base.SetRequestAcceptEncoding(webrequest);
        }

        public new TResult Invoke<TBody, TResult>(string method, string url, string contentType, TBody body, NameValueCollection query, out WebHeaderCollection responseHeaders)
        {
            return base.Invoke<TBody, TResult>(method, url, contentType, body, query, out responseHeaders);
        }
    }
}
