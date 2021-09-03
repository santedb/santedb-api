/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Http.Description;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.LZMA;
using SharpCompress.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents a simple rest client
    /// </summary>
    /// <remarks>This class represets a base class from which specific implementations of a REST client can be implemented</remarks>
    public abstract class RestClientBase : IRestClient
    {
        // Configuration
        private IRestClientDescription m_configuration;
        
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

        /// <summary>
        /// Convert headers
        /// </summary>
        private IDictionary<String, String> ConvertHeaders(WebHeaderCollection headers)
        {
            Dictionary<String, String> retVal = new Dictionary<string, string>();
            foreach (var k in headers.AllKeys)
                retVal.Add(k, headers[k]);
            return retVal;
        }

        /// <summary>
        /// Fire that progress has changed
        /// </summary>
        protected void FireProgressChanged(object state, float progress)
        {
            ProgressChangedEventArgs e = new ProgressChangedEventArgs(progress, state);
            this.ProgressChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRestClient"/> class.
        /// </summary>
        public RestClientBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IRestClient"/> class.
        /// </summary>
        /// <param name="config">The configuraiton of this client</param>
        public RestClientBase(IRestClientDescription config)
        {
            this.m_configuration = config;
        }

        /// <summary>
        /// Create the query string from a list of query parameters
        /// </summary>
        /// <param name="query">The query to be sent to the server</param>
        /// <returns>The query string</returns>
        public static String CreateQueryString(NameValueCollection query)
        {
            String queryString = String.Empty;
            foreach (var kv in query)
            {
                foreach (var v in kv.Value)
                {
                    if (v == null) continue;
                    queryString += String.Format("{0}={1}&", kv.Key, Uri.EscapeDataString(v));
                }
            }
            if (queryString.Length > 0)
                return queryString.Substring(0, queryString.Length - 1);
            else
                return queryString;
        }

        /// <summary>
        /// Create the HTTP request
        /// </summary>
        /// <param name="query">The query which should be executed on the server</param>
        /// <param name="resourceNameOrUrl">The name of the resource at the base URL to be executed</param>
        protected virtual WebRequest CreateHttpRequest(String resourceNameOrUrl, NameValueCollection query)
        {
            // URL is relative to base address
            if (this.Description.Endpoint.Count == 0)
                throw new InvalidOperationException("No endpoints found, is the interface configured properly?");

            if (!Uri.TryCreate(resourceNameOrUrl, UriKind.Absolute, out Uri uri)
                || uri.Scheme == "file")
            {
                s_tracer.TraceVerbose("Original resource {0} is not absolute or is wrong scheme - building service", resourceNameOrUrl);
                var baseUrl = new Uri(this.Description.Endpoint[0].Address);
                UriBuilder uriBuilder = new UriBuilder(baseUrl);

                if (!String.IsNullOrEmpty(resourceNameOrUrl))
                    uriBuilder.Path += "/" + resourceNameOrUrl;

                // HACK:
                uriBuilder.Path = uriBuilder.Path.Replace("//", "/");
                // Add query string
                if (query != null)
                    uriBuilder.Query = CreateQueryString(query);

                uri = uriBuilder.Uri;
            }
            else
                s_tracer.TraceVerbose("Constructed URI : {0}", uri);

            // Log
            s_tracer.TraceVerbose("Constructing WebRequest to {0}", uri);

            // Add headers
            HttpWebRequest retVal = (HttpWebRequest)HttpWebRequest.Create(uri.ToString());

            if (this.Credentials == null &&
                this.Description.Binding.Security?.CredentialProvider != null &&
                this.Description.Binding.Security?.PreemptiveAuthentication == true)
                this.Credentials = this.Description.Binding.Security.CredentialProvider.GetCredentials(this);

            if (this.Credentials != null)
            {
                foreach (var kv in this.Credentials.GetHttpHeaders())
                {
                    s_tracer.TraceVerbose("Adding header {0}:{1}", kv.Key, kv.Value);
                    retVal.Headers[kv.Key] = kv.Value;
                }
            }

            // Compress?
            if (this.Description.Binding.Optimize)
                retVal.Headers[HttpRequestHeader.AcceptEncoding] = "lzma,bzip2,gzip,deflate";

            // Return type?
            if (!String.IsNullOrEmpty(this.Accept))
            {
                s_tracer.TraceVerbose("Accepts {0}", this.Accept);
                retVal.Accept = this.Accept;
            }

            return retVal;
        }

        #region IRestClient implementation



        /// <summary>
        /// Gets the specified item
        /// </summary>
        /// <param name="url">The resource URL which should be fetched</param>
        /// <typeparam name="TResult">The expected response from the server</typeparam>
        public TResult Get<TResult>(string url)
        {
            return this.Get<TResult>(url, null);
        }

        /// <summary>
        /// Fetches the specified result from the server
        /// </summary>
        /// <param name="query">The query to be executed on th eserver</param>
        /// <param name="url">The resource URL to fetch from the server</param>
        /// <typeparam name="TResult">The expected result from the server</typeparam>
        public TResult Get<TResult>(string url, params KeyValuePair<string, object>[] query)
        {
            return this.Invoke<Object, TResult>("GET", url, null, null, query);
        }

        /// <summary>
        /// Retrieves a raw byte array of data from the specified location
        /// </summary>
        /// <param name="url">The resource URL to fetch from the server</param>
        /// <param name="query">The query (as key=value) to send on the GET request</param>
        public byte[] Get(String url, params KeyValuePair<string, object>[] query)
        {
            NameValueCollection parameters = new NameValueCollection();

            try
            {


                var requestEventArgs = new RestRequestEventArgs("GET", url, new NameValueCollection(query), null, null);
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
                        requestException = o.Exception.InnerExceptions.First();
                    else
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
                                        using (var dfs = new DeflateStream(new NonDisposingStream(ms), CompressionMode.Decompress))
                                        using (var oms = new MemoryStream())
                                        {
                                            dfs.CopyTo(oms);
                                            retVal = oms.ToArray();
                                        }
                                        break;
                                    case "gzip":
                                        using (var gzs = new GZipStream(new NonDisposingStream(ms), CompressionMode.Decompress))
                                        using (var oms = new MemoryStream())
                                        {
                                            gzs.CopyTo(oms);
                                            retVal = oms.ToArray();
                                        }
                                        break;
                                    case "bzip2":
                                        using (var lzmas = new BZip2Stream(new NonDisposingStream(ms), CompressionMode.Decompress, false))
                                        using (var oms = new MemoryStream())
                                        {
                                            lzmas.CopyTo(oms);
                                            retVal = oms.ToArray();
                                        }
                                        break;
                                    case "lzma":
                                        using (var lzmas = new LZipStream(new NonDisposingStream(ms), CompressionMode.Decompress))
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
                }, TaskContinuationOptions.LongRunning);
                httpTask.Wait();
                if (requestException != null)
                    throw requestException;


                this.Responded?.Invoke(this, new RestResponseEventArgs("GET", url, null, null, null, 200, 0, this.ConvertHeaders(headers)));

                return retVal;
            }
            catch (WebException e)
            {
                switch (this.CategorizeResponse(e.Response))
                {
                    case ServiceClientErrorType.Ok:
                        return this.Get(url);
                    default:
                        throw new RestClientException<byte[]>(
                                            null,
                                            e,
                                            e.Status,
                                            e.Response);
                }
            }
            catch (Exception e)
            {
                s_tracer.TraceError("Error invoking HTTP: {0}", e.Message);
                this.Responded?.Invoke(this, new RestResponseEventArgs("GET", url, null, null, null, 500, 0, null));
                throw;
            }
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
            return this.Invoke<TBody, TResult>(method, url, contentType, body, null);
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
        public TResult Invoke<TBody, TResult>(string method, string url, string contentType, TBody body, params KeyValuePair<string, object>[] query)
        {
            NameValueCollection parameters = new NameValueCollection();

            try
            {
                if (query != null)
                {
                    parameters = new NameValueCollection(query);
                }

                var requestEventArgs = new RestRequestEventArgs(method, url, parameters, contentType, body);
                this.Requesting?.Invoke(this, requestEventArgs);
                if (requestEventArgs.Cancel)
                {
                    s_tracer.TraceVerbose("HTTP request cancelled");
                    return default(TResult);
                }

                // Invoke
                WebHeaderCollection responseHeaders = null;
                var retVal = this.InvokeInternal<TBody, TResult>(requestEventArgs.Method, requestEventArgs.Url, requestEventArgs.ContentType, requestEventArgs.AdditionalHeaders, out responseHeaders, body, requestEventArgs.Query);
                this.Responded?.Invoke(this, new RestResponseEventArgs(requestEventArgs.Method, requestEventArgs.Url, requestEventArgs.Query, requestEventArgs.ContentType, retVal, 200, 0, this.ConvertHeaders(responseHeaders)));
                return retVal;
            }
            catch (Exception e)
            {
                s_tracer.TraceError("Error invoking HTTP: {0}", e.Message);
                this.Responded?.Invoke(this, new RestResponseEventArgs(method, url, parameters, contentType, null, 500, 0, null));
                throw;
            }
        }

        /// <summary>
        /// Implementation specific implementatoin of invoke
        /// </summary>
        /// <typeparam name="TBody">The type of <paramref name="body"/></typeparam>
        /// <typeparam name="TResult">The expected response (response hint) from the server</typeparam>
        /// <param name="method">The method to invoke on the server</param>
        /// <param name="url">The resource URL to be invoked</param>
        /// <param name="contentType">The content/type of <paramref name="body"/></param>
        /// <param name="requestHeaders">Additional request headers to be sent to the server</param>
        /// <param name="responseHeaders">Response headers from the server</param>
        /// <param name="body">The body / contents to be submitted to the server</param>
        /// <param name="query">The query to be affixed to the URL</param>
        /// <returns>The response from the server</returns>
        protected abstract TResult InvokeInternal<TBody, TResult>(string method, string url, string contentType, WebHeaderCollection requestHeaders, out WebHeaderCollection responseHeaders, TBody body, NameValueCollection query);

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
        public Credentials Credentials
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of acceptable response formats
        /// </summary>
        /// <value>The accept.</value>
        public string Accept
        {
            get;
            set;
        }

        /// <summary>
        /// Get the description (configuration) of this service
        /// </summary>
        public IRestClientDescription Description { get { return this.m_configuration; } set { this.m_configuration = value; } }

        #endregion IRestClient implementation

        /// <summary>
        /// Validate the response
        /// </summary>
        /// <returns>The type of error that was categorized</returns>
        /// <param name="response">The WebResponse from the server</param>
        protected virtual ServiceClientErrorType CategorizeResponse(WebResponse response)
        {
            if (response is HttpWebResponse)
            {
                var httpResponse = response as HttpWebResponse;
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        {
                            if (response.Headers["WWW-Authenticate"]?.StartsWith(this.Description.Binding.Security.Mode.ToString(), StringComparison.CurrentCultureIgnoreCase) == false)
                                return ServiceClientErrorType.AuthenticationSchemeMismatch;
                            else
                            {
                                // Validate the realm
                                // TODO: Refactor this to use REGEX
                                string wwwAuth = response.Headers["WWW-Authenticate"];
                                int realmStart = wwwAuth.IndexOf("realm=\"");
                                if (realmStart < 0)
                                    return ServiceClientErrorType.SecurityError; // No realm
                                realmStart += 7;// skip realm
                                string realm = wwwAuth.Substring(realmStart, wwwAuth.IndexOf('"', realmStart) - realmStart);

                                if (!String.IsNullOrEmpty(this.Description.Binding.Security.AuthRealm) &&
                                    !this.Description.Binding.Security.AuthRealm.Equals(realm))
                                {
                                    s_tracer.TraceWarning("Warning: REALM mismatch, authentication may fail. Server reports {0} but client configured for {1}", realm, this.Description.Binding.Security.AuthRealm);
                                }

                                int errorStart = wwwAuth.IndexOf("error=\"");
                                if (errorStart > -1)
                                { // Error is provided
                                    errorStart += 7;// skip realm
                                    string errorCode = wwwAuth.Substring(errorStart, wwwAuth.IndexOf('"', errorStart) - errorStart);
                                    if (errorCode.Equals("insufficient_scope", StringComparison.OrdinalIgnoreCase)) // We have invalid scope, we need to challenge
                                    {
                                        var scopeStart = wwwAuth.IndexOf("scope=\"");
                                        if (scopeStart > -1) // Scope is declared
                                        {
                                            scopeStart += 7;// skip realm
                                            string scope = wwwAuth.Substring(scopeStart, wwwAuth.IndexOf('"', scopeStart) - scopeStart);
                                            throw new PolicyViolationException(this.Credentials.Principal, scope, Model.Security.PolicyGrantType.Elevate);
                                        }
                                    }
                                }


                                // Credential provider
                                if (this.Description.Binding.Security.CredentialProvider != null)
                                {
                                    try
                                    {
                                        this.Credentials = this.Description.Binding.Security.CredentialProvider?.Authenticate(this);
                                        if (this.Credentials != null) // We have authentication, just needs elevation?
                                        {

                                            return ServiceClientErrorType.Ok;
                                        }
                                        else
                                            return ServiceClientErrorType.SecurityError;
                                    }
                                    catch
                                    {
                                        return ServiceClientErrorType.SecurityError;
                                    }
                                }
                                else
                                    return ServiceClientErrorType.SecurityError;
                            }
                        }
                    case HttpStatusCode.ServiceUnavailable:
                        return ServiceClientErrorType.NotReady;
                    case HttpStatusCode.OK:
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.NotModified:
                    case HttpStatusCode.Created:
                    case HttpStatusCode.Redirect:
                    case HttpStatusCode.Moved:
                    case HttpStatusCode.RedirectKeepVerb:
                    case HttpStatusCode.RedirectMethod:
                        return ServiceClientErrorType.Ok;
                    default:
                        return ServiceClientErrorType.GenericError;
                }
            }
            else
                return ServiceClientErrorType.GenericError;
        }

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
                requestHeaders[HttpRequestHeader.IfMatch] = ifMatch;

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

        /// <summary>
        /// Perform a head operation against the specified url
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <param name="resourceName">The name of the resource (url)</param>
        /// <returns>The HTTP headers (result of the HEAD operation)</returns>
        public IDictionary<string, string> Head(string resourceName, params KeyValuePair<String, Object>[] query)
        {
            NameValueCollection parameters = new NameValueCollection();

            try
            {
                if (query != null)
                {
                    parameters = new NameValueCollection(query);
                }

                var requestEventArgs = new RestRequestEventArgs("HEAD", resourceName, parameters, null, null);
                this.Requesting?.Invoke(this, requestEventArgs);
                if (requestEventArgs.Cancel)
                {
                    s_tracer.TraceVerbose("HTTP request cancelled");
                    return null;
                }

                // Invoke
                var httpWebReq = this.CreateHttpRequest(resourceName, requestEventArgs.Query);
                httpWebReq.Method = "HEAD";

                // Get the responst
                Dictionary<String, String> retVal = new Dictionary<string, string>();
                Exception fault = null;
                var httpTask = httpWebReq.GetResponseAsync().ContinueWith(o =>
                {
                    if (o.IsFaulted)
                        fault = o.Exception.InnerExceptions.First();
                    else
                    {
                        this.Responding?.Invoke(this, new RestResponseEventArgs("HEAD", resourceName, parameters, null, null, 200, o.Result.ContentLength, this.ConvertHeaders(o.Result.Headers)));
                        foreach (var itm in o.Result.Headers.AllKeys)
                            retVal.Add(itm, o.Result.Headers[itm]);
                    }
                }, TaskContinuationOptions.LongRunning);
                httpTask.Wait();
                if (fault != null)
                    throw fault;
                this.Responded?.Invoke(this, new RestResponseEventArgs("HEAD", resourceName, parameters, null, null, 200, 0, retVal));

                return retVal;
            }
            catch (Exception e)
            {
                s_tracer.TraceError("Error invoking HTTP: {0}", e.Message);
                this.Responded?.Invoke(this, new RestResponseEventArgs("HEAD", resourceName, parameters, null, null, 500, 0, null));
                throw;
            }
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
            return this.Invoke<Object, TResult>("LOCK", url, null, null, null);
        }

        /// <summary>
        /// Perform an unlock
        /// </summary>
        /// <param name="url">The resource URL to unlock</param>
        /// <returns>The result of the unlock from the server</returns>
        /// <typeparam name="TResult">The expected type of result</typeparam>
        public TResult Unlock<TResult>(string url)
        {
            return this.Invoke<Object, TResult>("UNLOCK", url, null, null, null);
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