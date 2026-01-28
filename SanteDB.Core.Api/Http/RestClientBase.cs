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
 * Date: 2023-6-21
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Http.Description;
using SanteDB.Core.Services;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.LZMA;
using SharpCompress.IO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents a simple rest client
    /// </summary>
    /// <remarks>This class represets a base class from which specific implementations of a REST client can be implemented</remarks>
    public abstract class RestClientBase : IRestClient
    {

        // Get tracer
        private static Tracer s_tracer = Tracer.GetTracer(typeof(RestClientBase));

        /// <summary>
        /// Fired on request
        /// </summary>
        public event EventHandler<RestRequestEventArgs> Requesting;

        /// <summary>
        /// Fired on response
        /// </summary>
        public event EventHandler<RestResponseEventArgs> Responded;

        /// <summary>
        /// Fired on response
        /// </summary>
        public event EventHandler<RestResponseEventArgs> Responding;

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public string Accept { get; set; }

        readonly string _TaskIdentifier;

        /// <summary>
        /// Convert headers
        /// </summary>
        private IDictionary<String, String> ConvertHeaders(WebHeaderCollection headers)
        {
            Dictionary<String, String> retVal = new Dictionary<string, string>();
            foreach (var k in headers.AllKeys)
            {
                retVal.Add(k, headers[k]);
            }

            return retVal;
        }

        /// <summary>
        /// Fire that progress has changed
        /// </summary>
        protected void FireProgressChanged(String state, float progress)
        {
            ProgressChangedEventArgs e = new ProgressChangedEventArgs(_TaskIdentifier, progress, state);
            this.ProgressChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRestClient"/> class.
        /// </summary>
        public RestClientBase()
        {
            _TaskIdentifier = this.GetType()?.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRestClient"/> class.
        /// </summary>
        /// <param name="config">The configuraiton of this client</param>
        public RestClientBase(IRestClientDescription config)
            : this()
        {
            this.Description = config;
            this.Accept = config.Accept;
        }

        /// <summary>
        /// Create the query string from a list of query parameters
        /// </summary>
        /// <param name="query">The query to be sent to the server</param>
        /// <returns>The query string</returns>
        public static String CreateQueryString(NameValueCollection query) => query.ToHttpString();

        /// <summary>
        /// Create the HTTP request
        /// </summary>
        /// <param name="query">The query which should be executed on the server</param>
        /// <param name="resourceNameOrUrl">The name of the resource at the base URL to be executed</param>
        /// <exception cref="InvalidOperationException">Thrown when the description (configuration) does not contain any endpoints.</exception>
        protected virtual WebRequest CreateHttpRequest(String resourceNameOrUrl, NameValueCollection query)
        {
            // URL is relative to base address
            if (this.Description?.Endpoint?.IsNullOrEmpty() ?? true)
            {
                //TODO: Translate this error message string.
                throw new InvalidOperationException("No endpoints found, is the interface configured properly?");
            }

            if (!Uri.TryCreate(resourceNameOrUrl, UriKind.Absolute, out Uri uri)
                || uri.Scheme == "file") //TODO: fyfej: Should this be || (uri.Scheme != "http" && uri.Scheme != "https") instead?
            {
                //TODO: Translate this tracer message.
                s_tracer.TraceVerbose("Original resource {0} is not absolute or is wrong scheme - building service", resourceNameOrUrl);
                uri = CreateCorrectRequestUri(resourceNameOrUrl, query);
            }
            else
            {
                //TODO: Translate this tracer message.
                //s_tracer.TraceVerbose("Constructed URI : {0}", uri);
            }


            //TODO: Translate this tracer message.
            // Log
            s_tracer.TraceVerbose("Constructing WebRequest to {0}", uri);

            //Create request object.
            var webrequest = (HttpWebRequest)HttpWebRequest.Create(uri.ToString());

            GetRequestCredentials()?.SetCredentials(webrequest);

            // Set appropriate header
            SetRequestAcceptEncoding(webrequest);

            // HACK: If we are posting something other than application/xml or application/json (like multi-part data) we need to tell the server
            // we want our response in the default
            if (!String.IsNullOrEmpty(this.Accept))
            {
                var contentType = new ContentType(this.Accept);
                if (contentType.MediaType.StartsWith("multipart/form-data"))
                {
                    webrequest.Accept = "application/xml";
                }
                else
                {
                    webrequest.Accept = this.Accept;
                }
            }

            return webrequest;
        }

        /// <summary>
        /// Create a uri for a request when the request uri is not valid.
        /// </summary>
        /// <param name="resourceNameOrUrl">The type of resource object to fetch.</param>
        /// <param name="query">The query parameters of the request.</param>
        /// <returns>A uri that is valid for the request and can be passed to a <see cref="HttpWebRequest"/>.</returns>
        protected virtual Uri CreateCorrectRequestUri(string resourceNameOrUrl, NameValueCollection query)
        {
            Uri uri;
            var baseUrl = new Uri(this.Description.Endpoint.First().Address);
            UriBuilder uriBuilder = new UriBuilder(baseUrl);

            if (!String.IsNullOrEmpty(resourceNameOrUrl))
            {
                uriBuilder.Path += "/" + resourceNameOrUrl;
            }

            // HACK:
            uriBuilder.Path = uriBuilder.Path.Replace("//", "/");
            // Add query string
            if (query != null && query.AllKeys.Any())
            {
                uriBuilder.Query = CreateQueryString(query);
            }

            uri = uriBuilder.Uri;
            return uri;
        }

        /// <summary>
        /// Sets the request Accept-Encoding header based on the <see cref="Description"/> for the client.
        /// </summary>
        /// <param name="webrequest">The request to set the header for.</param>
        protected virtual void SetRequestAcceptEncoding(HttpWebRequest webrequest)
        {
            StringBuilder acceptBuilder = new StringBuilder();
            if (this.Description.Binding.OptimizationMethod.HasFlag(HttpCompressionAlgorithm.Lzma))
            {
                acceptBuilder.Append(",lzma");
            }

            if (this.Description.Binding.OptimizationMethod.HasFlag(HttpCompressionAlgorithm.Bzip2))
            {
                acceptBuilder.Append(",bzip2");
            }

            if (this.Description.Binding.OptimizationMethod.HasFlag(HttpCompressionAlgorithm.Gzip))
            {
                acceptBuilder.Append(",gzip");
            }

            if (this.Description.Binding.OptimizationMethod.HasFlag(HttpCompressionAlgorithm.Deflate))
            {
                acceptBuilder.Append(",deflate");
            }

            if (acceptBuilder.Length > 1)
            {
                acceptBuilder.Remove(0, 1);
                webrequest.Headers[HttpRequestHeader.AcceptEncoding] = acceptBuilder.ToString();
            }
        }

        /// <summary>
        /// Tries to retrieve the credentials that should be used for a <see cref="HttpWebRequest"/> request. Override this method to provide credentials to requests.
        /// </summary>
        /// <returns>An instance of <see cref="RestRequestCredentials"/> or <c>null</c> if no credentials are available.</returns>
        protected virtual RestRequestCredentials GetRequestCredentials()
        {
            if (this.Credentials == null &&
                            this.Description.Binding.Security?.CredentialProvider != null &&
                            this.Description.Binding.Security?.PreemptiveAuthentication == true)
            {
                this.Credentials = this.Description.Binding.Security.CredentialProvider.GetCredentials(this);
            }

            return this.Credentials;
        }

        #region IRestClient implementation

        /// <summary>
        /// Gets the specified item
        /// </summary>
        /// <param name="url">The resource URL which should be fetched</param>
        /// <typeparam name="TResult">The expected response from the server</typeparam>
        public TResult Get<TResult>(string url)
        {
            return this.Get<TResult>(url, (NameValueCollection)null);
        }

        /// <summary>
        /// Fetches the specified result from the server
        /// </summary>
        /// <param name="query">The query to be executed on th eserver</param>
        /// <param name="url">The resource URL to fetch from the server</param>
        /// <typeparam name="TResult">The expected result from the server</typeparam>
        public TResult Get<TResult>(string url, params KeyValuePair<string, string>[] query) => this.Get<TResult>(url, query.ToNameValueCollection());

        /// <summary>
        /// Get the specified <typeparamref name="TResult"/> 
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="url">The URL from which the result should be fetched</param>
        /// <param name="query">The query </param>
        /// <returns>The result</returns>
        public TResult Get<TResult>(string url, NameValueCollection query)
        {
            return this.Invoke<Object, TResult>("GET", url, null, null, query, out _);
        }

        /// <inheritdoc/>
        public byte[] Get(String url) => this.Get(url, null);

        /// <inheritdoc />
        public byte[] Get(String url, NameValueCollection query)
        {
            return this.Invoke<byte[], byte[]>("GET", url, null, null, query, out _);
        }

#if DEBUG

        private byte[] GetOld(String url, NameValueCollection query)
        {

            try
            {
                var requestEventArgs = new RestRequestEventArgs("GET", url, query, null, null);
                this.Requesting?.Invoke(this, requestEventArgs);
                if (requestEventArgs.Cancel)
                {
                    s_tracer.TraceVerbose("HTTP request cancelled");
                    return null;
                }

                // Invoke
                var httpWebReq = this.CreateHttpRequest(url, requestEventArgs.Query);
                httpWebReq.Method = "GET";

                // Get the responst
                byte[] retVal = null;
                WebHeaderCollection headers = null;
                Exception requestException = null;
                var httpTask = httpWebReq.GetResponseAsync().ContinueWith(o =>
                {
                    if (o.IsFaulted)
                    {
                        requestException = o.Exception.InnerExceptions.First();
                    }
                    else
                    {
                        try
                        {
                            headers = o.Result.Headers;
                            this.Responding?.Invoke(this, new RestResponseEventArgs("GET", url, requestEventArgs.Query, o.Result.ContentType, null, 200, o.Result.ContentLength, this.ConvertHeaders(headers)));

                            byte[] buffer = new byte[2048];
                            int br = 1;
                            using (var ms = new MemoryStream())
                            using (var httpStream = o.Result.GetResponseStream())
                            {
                                while (br > 0)
                                {
                                    br = httpStream.Read(buffer, 0, 2048);
                                    ms.Write(buffer, 0, br);
                                    // Raise event
                                    this.FireProgressChanged(o.Result.ContentType, ms.Length / (float)o.Result.ContentLength);
                                }

                                ms.Seek(0, SeekOrigin.Begin);

                                switch (o.Result.Headers["Content-Encoding"])
                                {
                                    case "deflate":
                                        using (var dfs = new DeflateStream(NonDisposingStream.Create(ms), CompressionMode.Decompress))
                                        using (var oms = new MemoryStream())
                                        {
                                            dfs.CopyTo(oms);
                                            retVal = oms.ToArray();
                                        }
                                        break;

                                    case "gzip":
                                        using (var gzs = new GZipStream(NonDisposingStream.Create(ms), CompressionMode.Decompress))
                                        using (var oms = new MemoryStream())
                                        {
                                            gzs.CopyTo(oms);
                                            retVal = oms.ToArray();
                                        }
                                        break;

                                    case "bzip2":
                                        using (var lzmas = new BZip2Stream(NonDisposingStream.Create(ms), CompressionMode.Decompress, false))
                                        using (var oms = new MemoryStream())
                                        {
                                            lzmas.CopyTo(oms);
                                            retVal = oms.ToArray();
                                        }
                                        break;

                                    case "lzma":
                                        using (var lzmas = new LZipStream(NonDisposingStream.Create(ms), CompressionMode.Decompress))
                                        using (var oms = new MemoryStream())
                                        {
                                            lzmas.CopyTo(oms);
                                            retVal = oms.ToArray();
                                        }
                                        break;

                                    default:
                                        retVal = ms.ToArray();
                                        break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            s_tracer.TraceError("Error downloading {0}: {1}", url, e.Message);
                        }
                    }
                }, TaskContinuationOptions.LongRunning);
                httpTask.Wait();
                if (requestException != null)
                {
                    throw requestException;
                }

                this.Responded?.Invoke(this, new RestResponseEventArgs("GET", url, null, null, null, 200, 0, this.ConvertHeaders(headers)));

                return retVal;
            }
            catch (WebException e)
            {

                throw new RestClientException<byte[]>(
                                    null,
                                    e,
                                    e.Status,
                                    e.Response);

            }
            catch (Exception e)
            {
                s_tracer.TraceError("Error invoking HTTP: {0}", e.Message);
                this.Responded?.Invoke(this, new RestResponseEventArgs("GET", url, null, null, null, 500, 0, null));
                throw;
            }
        }
#endif

        /// <summary>
        /// Invokes the specified method against the URL provided
        /// </summary>
        /// <param name="method">The HTTP method to be executed</param>
        /// <param name="body">The body to be sent to the server</param>
        /// <typeparam name="TBody">The type of the <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected response from the server</typeparam>
        /// <param name="url">The URL on which the invokation should occur</param>
        public TResult Invoke<TBody, TResult>(string method, string url, TBody body)
        {
            return this.Invoke<TBody, TResult>(method, url, this.Accept, body, null, out _);
        }

        /// <summary>
        /// Invokes the specified method against the URL provided
        /// </summary>
        /// <param name="method">The HTTP method to be executed</param>
        /// <param name="contentType">Content type of <paramref name="body"/></param>
        /// <param name="body">The body to be sent to the server</param>
        /// <typeparam name="TBody">The type of the <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected response from the server</typeparam>
        /// <param name="url">The URL on which the invokation should occur</param>
        public TResult Invoke<TBody, TResult>(string method, string url, string contentType, TBody body)
        {
            return this.Invoke<TBody, TResult>(method, url, contentType, body, null, out _);
        }

        /// <summary>
        /// Invoke the specified method against the server
        /// </summary>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected response type from the server</typeparam>
        /// <param name="method">The HTTP method to be executed</param>
        /// <param name="url">The resource URL to be executed against</param>
        /// <param name="contentType">The content/type of <paramref name="body"/></param>
        /// <param name="body">The contents of the request to send to the server</param>
        /// <param name="query">The query to append to the URL</param>
        /// <returns>The server response</returns>
        public TResult Invoke<TBody, TResult>(string method, string url, string contentType, TBody body, NameValueCollection query)
        {
            return this.Invoke<TBody, TResult>(method, url, contentType, body, query, out _);
        }

        /// <summary>
        /// Invoke the specified method against the server
        /// </summary>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected response type from the server</typeparam>
        /// <param name="method">The HTTP method to be executed</param>
        /// <param name="url">The resource URL to be executed against</param>
        /// <param name="contentType">The content/type of <paramref name="body"/></param>
        /// <param name="body">The contents of the request to send to the server</param>
        /// <param name="query">The query to append to the URL</param>
        /// <param name="responseHeaders"></param>
        /// <returns>The server response</returns>
        protected virtual TResult Invoke<TBody, TResult>(string method, string url, string contentType, TBody body, NameValueCollection query, out WebHeaderCollection responseHeaders)
        {
            responseHeaders = null;
            try
            {


                var requestEventArgs = new RestRequestEventArgs(method, url, query, contentType ?? this.Accept, body);
                this.Requesting?.Invoke(this, requestEventArgs);
                if (requestEventArgs.Cancel)
                {
                    s_tracer.TraceVerbose("HTTP request cancelled");
                    return default(TResult);
                }

                // Invoke
                var retVal = this.InvokeInternal<TBody, TResult>(requestEventArgs.Method, requestEventArgs.Url, requestEventArgs.ContentType, requestEventArgs.AdditionalHeaders, out responseHeaders, body, requestEventArgs.Query);
                this.Responded?.Invoke(this, new RestResponseEventArgs(requestEventArgs.Method, requestEventArgs.Url, requestEventArgs.Query, responseHeaders[HttpRequestHeader.ContentType], retVal, 200, Int32.Parse(responseHeaders[HttpRequestHeader.ContentLength]), this.ConvertHeaders(responseHeaders)));
                return retVal;
            }
            catch (Exception e)
            {
                s_tracer.TraceError("Error invoking HTTP: {0}", e.Message);
                this.Responded?.Invoke(this, new RestResponseEventArgs(method, url, query, contentType ?? this.Accept, null, 500, 0, null));
                throw;
            }
        }

        /// <summary>
        /// Invokes the request. Implementations of <see cref="RestClientBase"/> must provide the implementation of <see cref="InvokeInternal{TBody, TResult}(string, string, string, WebHeaderCollection, out WebHeaderCollection, TBody, NameValueCollection)"/>.
        /// </summary>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected response (response hint) from the server</typeparam>
        /// <param name="method">The method to invoke on the server</param>
        /// <param name="url">The resource URL to be invoked</param>
        /// <param name="contentType">The content/type of <paramref name="body"/></param>
        /// <param name="requestHeaders">Additional request headers to be sent to the server</param>
        /// <param name="responseHeaders">Response headers from the server</param>
        /// <param name="body">The body / contents to be submitted to the server. If this is <c>default(TBody)</c>, no body is sent to the server.</param>
        /// <param name="query">The query to be affixed to the URL</param>
        /// <returns>The response from the server</returns>
        protected abstract TResult InvokeInternal<TBody, TResult>(string method, string url, string contentType, WebHeaderCollection requestHeaders, out WebHeaderCollection responseHeaders, TBody body, NameValueCollection query);

        /// <summary>
        /// Execute a post against the <paramref name="url"/>
        /// </summary>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected response type from the server</typeparam>
        /// <param name="url">The resource URL</param>
        /// <param name="body">The body contents to be submitted to the server</param>
        /// <returns>The result from the server</returns>
        public TResult Post<TBody, TResult>(string url, TBody body)
        {
            return this.Invoke<TBody, TResult>("POST", url, this.Accept, body);
        }

        /// <summary>
        /// Execute a post against the <paramref name="url"/>
        /// </summary>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected response type from the server</typeparam>
        /// <param name="url">The resource URL</param>
        /// <param name="contentType">The content/type of <paramref name="body"/></param>
        /// <param name="body">The body contents to be submitted to the server</param>
        /// <returns>The result from the server</returns>
        public TResult Post<TBody, TResult>(string url, string contentType, TBody body)
        {
            return this.Invoke<TBody, TResult>("POST", url, contentType, body);
        }

        /// <summary>
        /// Executes an HTTP delete operation on the server
        /// </summary>
        /// <typeparam name="TResult">The expected response type from the server</typeparam>
        /// <param name="url">The resource URL which should be executed against</param>
        /// <returns>The response from the server</returns>
        public TResult Delete<TResult>(string url)
        {
            return this.Invoke<Object, TResult>("DELETE", url, null, null);
        }

        /// <summary>
        /// Executes an HTTP PUT against the server
        /// </summary>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected result from the server</typeparam>
        /// <param name="url">The resource URL to be executed against</param>
        /// <param name="body">The content to be submitted to the server</param>
        /// <returns>The response from th eserver</returns>
        public TResult Put<TBody, TResult>(string url, TBody body)
        {
            return this.Invoke<TBody, TResult>("PUT", url, this.Accept, body);
        }

        /// <summary>
        /// Executes an HTTP PUT against the server
        /// </summary>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected result from the server</typeparam>
        /// <param name="url">The resource URL to be executed against</param>
        /// <param name="contentType">The content/type to use to serialize <paramref name="body"/></param>
        /// <param name="body">The content to be submitted to the server</param>
        /// <returns>The response from th eserver</returns>
        public TResult Put<TBody, TResult>(string url, string contentType, TBody body)
        {
            return this.Invoke<TBody, TResult>("PUT", url, contentType, body);
        }

        /// <summary>
        /// Executes an HTTP options against the server
        /// </summary>
        /// <typeparam name="TResult">The expected result type from the server</typeparam>
        /// <param name="url">The reosurce url to execute options against</param>
        /// <returns>The result from the server</returns>
        public TResult Options<TResult>(string url)
        {
            return this.Invoke<Object, TResult>("OPTIONS", url, null, null);
        }

        /// <summary>
        /// Gets or sets the credentials to be used for this client
        /// </summary>
        public RestRequestCredentials Credentials
        {
            get;
            set;
        }

        /// <summary>
        /// Get the description (configuration) of this service
        /// </summary>
        public IRestClientDescription Description { get; set; }

        #endregion IRestClient implementation

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Patches the specified resource at <paramref name="url"/> with <paramref name="patch"/> when <paramref name="ifMatch"/> is true
        /// </summary>
        /// <param name="url">The resource URL to patch</param>
        /// <param name="ifMatch">Identifies the If-Match header</param>
        /// <param name="patch">The patch contents</param>
        /// <returns>The new ETAG of the patched resource</returns>
        public String Patch<TPatch>(string url, String ifMatch, TPatch patch)
        {
            return this.Patch<TPatch>(url, this.Accept, ifMatch, patch);
        }

        /// <summary>
        /// Patches the specified resource at <paramref name="url"/> with <paramref name="patch"/> when <paramref name="ifMatch"/> is true
        /// </summary>
        /// <param name="url">The resource URL to patch</param>
        /// <param name="contentType">The content/type of the patch (dictates serialization)</param>
        /// <param name="ifMatch">Identifies the If-Match header</param>
        /// <param name="patch">The patch contents</param>
        /// <returns>The new ETAG of the patched resource</returns>
        public String Patch<TPatch>(string url, string contentType, String ifMatch, TPatch patch)
        {
            try
            {
                var requestEventArgs = new RestRequestEventArgs("PATCH", url, null, contentType, patch);
                this.Requesting?.Invoke(this, requestEventArgs);
                if (requestEventArgs.Cancel)
                {
                    s_tracer.TraceVerbose("HTTP request cancelled");
                    return null;
                }

                WebHeaderCollection requestHeaders = requestEventArgs.AdditionalHeaders ?? new WebHeaderCollection(),
                    responseHeaders = null;
                if (!String.IsNullOrEmpty(ifMatch))
                {
                    requestHeaders[HttpRequestHeader.IfMatch] = ifMatch;
                }

                // Invoke
                this.InvokeInternal<TPatch, Object>("PATCH", url, contentType, requestHeaders, out responseHeaders, patch, null);

                // Return the ETag of the
                return responseHeaders["ETag"];
            }
            catch (Exception e)
            {
                s_tracer.TraceError("Error invoking HTTP: {0}", e.Message);
                this.Responded?.Invoke(this, new RestResponseEventArgs("PATCH", url, null, contentType, null, 500, 0, null));
                throw;
            }
        }

        /// <inheritdoc/>
        public IDictionary<string, string> Head(string url) => this.Head(url, null);

        /// <inheritdoc />
        public IDictionary<string, string> Head(string url, NameValueCollection query)
        {
            _ = Invoke<byte[], byte[]>("HEAD", url, null, null, query, out var responseheaders);
            return this.ConvertHeaders(responseheaders);

            //try
            //{

            //    var requestEventArgs = new RestRequestEventArgs("HEAD", resourceName, query, null, null);
            //    this.Requesting?.Invoke(this, requestEventArgs);
            //    if (requestEventArgs.Cancel)
            //    {
            //        s_tracer.TraceVerbose("HTTP request cancelled");
            //        return null;
            //    }

            //    // Invoke
            //    var httpWebReq = this.CreateHttpRequest(resourceName, requestEventArgs.Query);
            //    httpWebReq.Method = "HEAD";

            //    // Get the responst
            //    Dictionary<String, String> retVal = new Dictionary<string, string>();
            //    Exception fault = null;
            //    var httpTask = httpWebReq.GetResponseAsync().ContinueWith(o =>
            //    {
            //        if (o.IsFaulted)
            //        {
            //            fault = o.Exception.InnerExceptions.First();
            //        }
            //        else
            //        {
            //            this.Responding?.Invoke(this, new RestResponseEventArgs("HEAD", resourceName, query, null, null, 200, o.Result.ContentLength, this.ConvertHeaders(o.Result.Headers)));
            //            foreach (var itm in o.Result.Headers.AllKeys)
            //            {
            //                retVal.Add(itm, o.Result.Headers[itm]);
            //            }
            //        }
            //    }, TaskContinuationOptions.LongRunning);
            //    httpTask.Wait();
            //    if (fault != null)
            //    {
            //        throw fault;
            //    }

            //    this.Responded?.Invoke(this, new RestResponseEventArgs("HEAD", resourceName, query, null, null, 200, 0, retVal));

            //    return retVal;
            //}
            //catch (Exception e)
            //{
            //    s_tracer.TraceError("Error invoking HTTP: {0}", e.Message);
            //    this.Responded?.Invoke(this, new RestResponseEventArgs("HEAD", resourceName, query, null, null, 500, 0, null));
            //    throw;
            //}
        }

        /// <summary>
        /// Fire responding event
        /// </summary>
        protected void FireResponding(RestResponseEventArgs args)
        {
            this.Responding?.Invoke(this, args);
        }

        /// <summary>
        /// Perform a lock on the specified resource
        /// </summary>
        /// <param name="url">The URL of the resource to lock</param>
        /// <returns>The result of the lock operation from the server</returns>
        /// <typeparam name="TResult">Expected result type</typeparam>
        public TResult Lock<TResult>(string url)
        {
            return this.Invoke<Object, TResult>("LOCK", url, null, null, null, out _);
        }

        /// <summary>
        /// Perform an unlock
        /// </summary>
        /// <param name="url">The resource URL to unlock</param>
        /// <returns>The result of the unlock from the server</returns>
        /// <typeparam name="TResult">The expected type of result</typeparam>
        public TResult Unlock<TResult>(string url)
        {
            return this.Invoke<Object, TResult>("UNLOCK", url, null, null, null, out _);
        }
    }

    /// <summary>
    /// Service client error type
    /// </summary>
    public enum ServiceClientErrorType
    {
        /// <summary>
        /// The service client response is valid
        /// </summary>
        Ok,

        /// <summary>
        /// The service client encountered a general error
        /// </summary>
        GenericError,

        /// <summary>
        /// The service client's authentication scheme does not match the server
        /// </summary>
        AuthenticationSchemeMismatch,

        /// <summary>
        /// The service client encountered a security error
        /// </summary>
        SecurityError,

        /// <summary>
        /// The service client is contacting the wrong realm
        /// </summary>
        RealmMismatch,

        /// <summary>
        /// The service client was not ready
        /// </summary>
        NotReady
    }
}