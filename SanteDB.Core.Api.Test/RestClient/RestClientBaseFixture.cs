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
 * Date: 2024-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Http;
using SanteDB.Core.Http.Description;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace SanteDB.Core.Api.Test.RestClient
{
    internal class RestClientBaseFixture : RestClientBase
    {
        public RestClientBaseFixture() : base() { }
        public RestClientBaseFixture(IRestClientDescription configuration) : base(configuration) { }

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

                foreach (var header in requestHeaders.AllKeys)
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
