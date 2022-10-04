/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Http.Compression;
using SanteDB.Core.Http.Description;
using SanteDB.Core.i18n;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Services;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Represents an android enabled rest client
    /// </summary>
    public class RestClient : RestClientBase
    {

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(RestClient));

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient"/> class.
        /// </summary>
        public RestClient() : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient"/> class.
        /// </summary>
        public RestClient(IRestClientDescription config) : base(config)
        {
            this.m_tracer = Tracer.GetTracer(this.GetType());
            // Find the specified certificate
            if (config.Binding.Security?.ClientCertificate != null)
            {
                this.ClientCertificates = new X509Certificate2Collection();
                this.ClientCertificates.Add(config.Binding.Security?.ClientCertificate);
            }
        }

        /// <summary>
        /// Create HTTP Request object
        /// </summary>
        protected override WebRequest CreateHttpRequest(string url, NameValueCollection query)
        {
            var retVal = (HttpWebRequest)base.CreateHttpRequest(url, query);

            // Certs?
            if (this.ClientCertificates != null)
            {
                retVal.ClientCertificates.AddRange(this.ClientCertificates);
            }

            // Proxy?
            if (!String.IsNullOrEmpty(this.Description.ProxyAddress))
            {
                retVal.Proxy = new WebProxy(this.Description.ProxyAddress);
            }

            try
            {
                retVal.ServerCertificateValidationCallback = this.RemoteCertificateValidation;
            }
            catch
            {
                this.m_tracer.TraceWarning("Cannot assign certificate validtion callback, will set servicepointmanager");
                ServicePointManager.ServerCertificateValidationCallback = this.RemoteCertificateValidation;
            }

            // Set appropriate header
            if (this.Description.Binding.Optimize)
            {
                retVal.Headers[HttpRequestHeader.AcceptEncoding] = "lzma,bzip2,gzip,deflate";
            }

            // Set user agent
            var asm = Assembly.GetEntryAssembly() ?? typeof(RestClient).Assembly;
            retVal.UserAgent = String.Format("{0} {1} ({2})", asm.GetCustomAttribute<AssemblyTitleAttribute>()?.Title, asm.GetName().Version, asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

            retVal.AllowAutoRedirect = false;

            // Are we forwarding this request?
            var remoteData = RemoteEndpointUtil.Current.GetRemoteClient();
            if (remoteData != null)
            {
                var fwdInfo = remoteData.ForwardInformation;
                if (!String.IsNullOrEmpty(fwdInfo))
                {
                    fwdInfo += $", {remoteData.RemoteAddress}";
                }

                retVal.Headers.Add("X-Real-IP", remoteData.RemoteAddress);
                retVal.Headers.Add("X-Forwarded-For", fwdInfo);
            }

            return retVal;
        }

        /// <summary>
        /// Remote certificate validation errors
        /// </summary>
        protected virtual bool RemoteCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            else
            {
                this.m_tracer.TraceWarning("SSL validation {0} - {1}", sslPolicyErrors, certificate);
            }

            var securityConfiguration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<SecurityConfigurationSection>();
            return securityConfiguration.TrustedCertificates.Contains(certificate.Subject);
        }

        /// <summary>
        /// Invokes the specified method against the url provided
        /// </summary>
        /// <param name="method">Method.</param>
        /// <param name="url">URL.</param>
        /// <param name="contentType">Content type.</param>
        /// <param name="body">Body.</param>
        /// <param name="query">Query.</param>
        /// <param name="additionalHeaders"></param>
        /// <param name="responseHeaders"></param>
        /// <typeparam name="TBody">The 1st type parameter.</typeparam>
        /// <typeparam name="TResult">The 2nd type parameter.</typeparam>
        protected override TResult InvokeInternal<TBody, TResult>(string method, string url, string contentType, WebHeaderCollection additionalHeaders, out WebHeaderCollection responseHeaders, TBody body, NameValueCollection query)
        {
            if (String.IsNullOrEmpty(method))
            {
                throw new ArgumentNullException(nameof(method));
            }
            //if (String.IsNullOrEmpty(url))
            //    throw new ArgumentNullException(nameof(url));


            // Credentials provided ?
            HttpWebRequest requestObj = this.CreateHttpRequest(url, query) as HttpWebRequest;
            if (!String.IsNullOrEmpty(contentType))
            {
                requestObj.ContentType = contentType;
            }

            requestObj.Method = method;

            // Additional headers
            if (additionalHeaders != null)
            {
                foreach (var hdr in additionalHeaders.AllKeys)
                {
                    if (hdr == "If-Modified-Since")
                    {
                        requestObj.IfModifiedSince = DateTime.Parse(additionalHeaders[hdr]);
                    }
                    else
                    {
                        requestObj.Headers.Add(hdr, additionalHeaders[hdr]);
                    }
                }
            }

#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            // Get request object

            // Body was provided?
            try
            {
                // Try assigned credentials
                IBodySerializer serializer = null;
                if (body != null)
                {
                    // GET Stream,
                    Stream requestStream = null;
                    try
                    {
                        // Get request object
                        var cancellationTokenSource = new CancellationTokenSource();
                        cancellationTokenSource.CancelAfter(this.Description.Endpoint[0].Timeout);
                        using (var requestTask = Task.Run(async () => { return await requestObj.GetRequestStreamAsync(); }, cancellationTokenSource.Token))
                        {
                            try
                            {
                                requestStream = requestTask.Result;
                            }
                            catch (AggregateException e)
                            {
                                requestObj.Abort();
                                throw e.InnerExceptions.First();
                            }
                        }

                        if (contentType == null && typeof(TResult) != typeof(Object))
                        {
                            throw new ArgumentNullException(nameof(contentType));
                        }

                        serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(contentType, typeof(TBody));
                        // Serialize and compress with deflate
                        using (MemoryStream ms = new MemoryStream())
                        {
                            if (this.Description.Binding.Optimize)
                            {
                                using (var str = CompressionUtil.GetCompressionScheme(this.Description.Binding.OptimizationMethod).CreateCompressionStream(requestStream))
                                {
                                    serializer.Serialize(str, body);
                                }
                            }
                            else
                            {
                                serializer.Serialize(ms, body);
                            }

                            // Trace
                            if (this.Description.Trace)
                            {
                                this.m_tracer.TraceVerbose("HTTP >> {0}", Convert.ToBase64String(ms.ToArray()));
                            }

                            using (var nms = new MemoryStream(ms.ToArray()))
                            {
                                nms.CopyTo(requestStream);
                            }
                        }
                    }
                    finally
                    {
                        if (requestStream != null)
                        {
                            requestStream.Dispose();
                        }
                    }
                }

                // Response
                HttpWebResponse response = null;
                try
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(this.Description.Endpoint[0].Timeout);
                    using (var responseTask = Task.Run(async () => { return await requestObj.GetResponseAsync(); }, cancellationTokenSource.Token))
                    {
                        try
                        {
                            response = (HttpWebResponse)responseTask.Result;
                        }
                        catch (AggregateException e)
                        {
                            requestObj.Abort();
                            throw e.InnerExceptions.First();
                        }
                    }

                    responseHeaders = response.Headers;

                    // No content - does the result want a pointer maybe?
                    if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.Continue || response.StatusCode == HttpStatusCode.NotModified)
                    {
                        return default(TResult);
                    }
                    else if (response.StatusCode == HttpStatusCode.RedirectKeepVerb)
                    {
                        return this.InvokeInternal<TBody, TResult>(method, response.Headers[HttpResponseHeader.Location], contentType, additionalHeaders, out responseHeaders, body, query);
                    }
                    else if (response.StatusCode == HttpStatusCode.RedirectMethod)
                    {
                        return this.InvokeInternal<TBody, TResult>("GET", response.Headers[HttpResponseHeader.Location], contentType, additionalHeaders, out responseHeaders, default(TBody), query);
                    }
                    else
                    {
                        // De-serialize
                        var responseContentType = response.ContentType;
                        if (String.IsNullOrEmpty(responseContentType))
                        {
                            return default(TResult);
                        }

                        if (responseContentType.Contains(";"))
                        {
                            responseContentType = responseContentType.Substring(0, responseContentType.IndexOf(";"));
                        }

                        if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            return default(TResult);
                        }

                        serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(responseContentType, typeof(TResult));

                        TResult retVal = default(TResult);
                        // Compression?
                        using (MemoryStream ms = new MemoryStream())
                        {
                            if (this.Description.Trace)
                            {
                                this.m_tracer.TraceVerbose("Received response {0} : {1} bytes", response.ContentType, response.ContentLength);
                            }

                            response.GetResponseStream().CopyTo(ms);

                            ms.Seek(0, SeekOrigin.Begin);

                            // Trace
                            if (this.Description.Trace)
                            {
                                this.m_tracer.TraceVerbose("HTTP << {0}", Convert.ToBase64String(ms.ToArray()));
                            }

                            if (!String.IsNullOrEmpty(response.Headers[HttpResponseHeader.ContentEncoding]))
                            {
                                using (var str = CompressionUtil.GetCompressionScheme(response.Headers[HttpResponseHeader.ContentEncoding]).CreateDecompressionStream(ms))
                                {
                                    retVal = (TResult)serializer.DeSerialize(str);
                                }
                            }
                            else
                            {
                                retVal = (TResult)serializer.DeSerialize(ms);
                            }
                            //retVal = (TResult)serializer.DeSerialize(ms);
                        }

                        return retVal;
                    }
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                    //responseTask.Dispose();
                }
            }
            catch (TimeoutException e)
            {
                this.m_tracer.TraceError("Request timed out:{0}", e.Message);
                throw;
            }
            catch (WebException e) when (e.Response is HttpWebResponse errorResponse && errorResponse.StatusCode == HttpStatusCode.NotModified)
            {
                this.m_tracer.TraceInfo("Server indicates not modified {0} {1} : {2}", method, url, e.Message);
                responseHeaders = errorResponse?.Headers;
                return default(TResult);
            }
            catch (WebException e) when (e.Response is HttpWebResponse errorResponse && e.Status == WebExceptionStatus.ProtocolError)
            {
                this.m_tracer.TraceError("Error executing {0} {1} : {2}", method, url, e.Message);
                // Deserialize
                object errorResult = null;

                var responseContentType = errorResponse.ContentType;
                if (responseContentType.Contains(";"))
                {
                    responseContentType = responseContentType.Substring(0, responseContentType.IndexOf(";"));
                }

                var ms = new MemoryStream(); // copy response to memory
                errorResponse.GetResponseStream().CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                try
                {
                    var serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(responseContentType, typeof(TResult));

                    if (!String.IsNullOrEmpty(errorResponse.Headers[HttpResponseHeader.ContentEncoding]))
                    {
                        using (var str = CompressionUtil.GetCompressionScheme(errorResponse.Headers[HttpResponseHeader.ContentEncoding]).CreateDecompressionStream(ms))
                        {
                            errorResult = serializer.DeSerialize(str);
                        }
                    }
                    else
                    {
                        errorResult = serializer.DeSerialize(ms);
                    }
                }
                catch (Exception e2)
                {
                    throw new RestClientException<object>(ErrorMessages.COMMUNICATION_RESPONSE_FAILURE, e2);
                }

                Exception exception = null;
                if (errorResponse is TResult tr)
                {
                    exception = new RestClientException<TResult>(tr, e, e.Status, e.Response);
                }
                else
                {
                    exception = new RestClientException<object>(errorResult, e, e.Status, e.Response);
                }

                switch (errorResponse.StatusCode)
                {
                    case HttpStatusCode.Unauthorized: // Validate the response

                        throw exception;

                    case HttpStatusCode.NotModified:
                        responseHeaders = errorResponse?.Headers;
                        return default(TResult);

                    case (HttpStatusCode)422:
                        throw exception;

                    default:
                        throw exception;
                }
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.Timeout)
            {
                this.m_tracer.TraceError("Error executing {0} {1} : {2}", method, url, e.Message);
                throw new TimeoutException($"Timeout executing REST operation {method} {url}", e);
            }
            catch (WebException e) when (e.Status == WebExceptionStatus.ConnectFailure)
            {
                this.m_tracer.TraceError("Error executing {0} {1} : {2}", method, url, e.Message);
                if ((e.InnerException as SocketException)?.SocketErrorCode == SocketError.TimedOut)
                {
                    throw new TimeoutException();
                }
                else
                {
                    throw;
                }
            }
            catch (WebException e)
            {
                this.m_tracer.TraceError("Error executing {0} {1} : {2}", method, url, e.Message);
                throw;
            }
            catch (InvalidOperationException e)
            {
                this.m_tracer.TraceError("Invalid Operation: {0}", e.Message);
                throw;
            }

        }

        /// <summary>
        /// Gets or sets the client certificate
        /// </summary>
        /// <value>The client certificate.</value>
        public X509Certificate2Collection ClientCertificates
        {
            get;
            set;
        }
    }
}