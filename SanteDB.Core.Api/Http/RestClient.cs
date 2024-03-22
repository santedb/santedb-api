/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Http.Compression;
using SanteDB.Core.Http.Description;
using SanteDB.Core.i18n;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Services;
using SharpCompress.IO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
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
        static readonly string s_UserAgent;
        static readonly TimeSpan s_DefaultTimeout;
        static readonly TimeSpan s_InfiniteTimeout;
        /// <summary>
        /// Sets static objects in the <see cref="RestClient"/>.
        /// </summary>
        static RestClient()
        {
            var asm = Assembly.GetEntryAssembly() ?? typeof(RestClient).Assembly;
            if (null != asm)
            {
                s_UserAgent = String.Format("{0} {1} ({2})", asm.GetCustomAttribute<AssemblyTitleAttribute>()?.Title, asm.GetName().Version, asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
            }
            else
            {
                s_UserAgent = "SanteDB 0.0 (0.0.0.0)";
            }


            s_InfiniteTimeout = TimeSpan.FromMilliseconds(-1);
            //Set this to -1 for infinite timeout.
            //s_DefaultTimeout = TimeSpan.FromMilliseconds(-1);
            s_DefaultTimeout = TimeSpan.FromSeconds(120); //Matches the built in functionality.
        }

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(RestClient));

        readonly RemoteEndpointUtil _RemoteEndpointUtil;
        readonly IServiceProvider _Services;


        /// <summary>
        /// Initializes a new instance of the <see cref="RestClient"/> class.
        /// </summary>
        public RestClient() : base()
        {
            _RemoteEndpointUtil = RemoteEndpointUtil.Current;
            _Services = ApplicationServiceContext.Current;
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
                this.ClientCertificates = new X509Certificate2Collection(config.Binding.Security?.ClientCertificate);
                //this.ClientCertificates.Add(config.Binding.Security?.ClientCertificate);
            }

            _RemoteEndpointUtil = RemoteEndpointUtil.Current;
            _Services = ApplicationServiceContext.Current;
        }

        /// <inheritdoc />
        protected override WebRequest CreateHttpRequest(string url, NameValueCollection query)
        {
            var webrequest = (HttpWebRequest)base.CreateHttpRequest(url, query);

            SetRequestClientCertificate(webrequest);

            SetRequestProxy(webrequest);

            SetRequestCertificateValidationCallback(webrequest);

            // Set user agent
            webrequest.UserAgent = s_UserAgent;

            webrequest.AllowAutoRedirect = false;

            // Are we forwarding this request?
            SetRequestRemoteData(webrequest);

            return webrequest;
        }


        /// <summary>
        /// Checks if any remote client information is available from <see cref="RemoteEndpointUtil"/> and appends it as headers to the <paramref name="webrequest"/>.
        /// </summary>
        /// <param name="webrequest">The request to append headers to if they are available from <see cref="RemoteEndpointUtil"/>.</param>
        protected virtual void SetRequestRemoteData(HttpWebRequest webrequest)
        {
            try
            {
                var remoteData = _RemoteEndpointUtil?.GetRemoteClient();
                if (null != remoteData)
                {
                    var fwdInfo = remoteData.ForwardInformation;
                    if (!string.IsNullOrEmpty(fwdInfo))
                    {
                        fwdInfo = $"{fwdInfo}, {remoteData.RemoteAddress}";
                    }
                    else
                    {
                        fwdInfo = remoteData.RemoteAddress;
                    }

                    webrequest?.Headers?.Add("X-Real-IP", remoteData.RemoteAddress);
                    webrequest?.Headers?.Add("X-Forwarded-For", fwdInfo);
                }
            }
            catch (Exception ex)
            {
                m_tracer.TraceError("Exception setting remote data for web request: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Set the callback for certificate validation for the <paramref name="webrequest"/>.
        /// </summary>
        /// <param name="webrequest">The request to set the <see cref="HttpWebRequest.ServerCertificateValidationCallback"/> for.</param>
        /// <exception cref="PlatformNotSupportedException">The platform does not support setting this per-request. Previous versions of SanteDB would set the <see cref="ServicePointManager"/> global instance.</exception>
        protected virtual void SetRequestCertificateValidationCallback(HttpWebRequest webrequest)
        {
            try
            {
                webrequest.ServerCertificateValidationCallback = this.RemoteCertificateValidation;
            }
            catch
            {
                //TODO: Obsolete.
#if DEBUG
                throw new PlatformNotSupportedException(ErrorMessages.PLATFORM_NOT_SUPPORTED);
#endif
                this.m_tracer.TraceWarning("Cannot assign certificate validtion callback, will set servicepointmanager. This will be removed in the next release of SanteDB.");
                ServicePointManager.ServerCertificateValidationCallback = this.RemoteCertificateValidation;
            }
        }

        /// <summary>
        /// Sets the proxy information on the <paramref name="webrequest"/>.
        /// </summary>
        /// <param name="webrequest">The reuqest to set the proxy information for.</param>
        protected virtual void SetRequestProxy(HttpWebRequest webrequest)
        {
            if (!string.IsNullOrEmpty(this.Description?.ProxyAddress))
            {
                try
                {
                    webrequest.Proxy = new WebProxy(this.Description.ProxyAddress);
                }
                catch (UriFormatException)
                {
                    this.m_tracer.TraceError("The proxy configuration for \"{0}\" contains an invalid proxy address: \"{1}\".", webrequest?.Address?.ToString(), this.Description.ProxyAddress);
                }
            }
        }

        /// <summary>
        /// Sets the client certificates on the <paramref name="webrequest"/>.
        /// </summary>
        /// <param name="webrequest">The request to set the client certificates for.</param>
        protected virtual void SetRequestClientCertificate(HttpWebRequest webrequest)
        {
            // Certs?
            if (this.ClientCertificates != null)
            {
                webrequest.ClientCertificates.AddRange(this.ClientCertificates);
            }
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

            var certificateValidator = _Services.GetService<ICertificateValidator>();
            var securityConfiguration = _Services.GetService<IConfigurationManager>()?.GetSection<SecurityConfigurationSection>();
            return securityConfiguration?.TrustedCertificates.Contains(certificate.Subject) == true ||
                certificateValidator?.ValidateCertificate((X509Certificate2)certificate, chain) == true;
        }

        /// <summary>
        /// Gets the <see cref="TimeSpan"/> that represents the timeout for an <see cref="InvokeInternal{TBody, TResult}(string, string, string, WebHeaderCollection, out WebHeaderCollection, TBody, NameValueCollection)"/> operation.
        /// </summary>
        /// <returns>A valid TimeSpan for the operation, or a timespan that represents -1 for infinite.</returns>
        protected virtual TimeSpan GetConnectTimeout() => this.Description?.Endpoint?.First()?.ConnectTimeout ?? s_DefaultTimeout;

        /// <summary>
        /// Gets the <see cref="TimeSpan"/> that represents the timeout for entire <see cref="InvokeInternal{TBody, TResult}(string, string, string, WebHeaderCollection, out WebHeaderCollection, TBody, NameValueCollection)"/> operation including receive and deserialization.
        /// </summary>
        /// <returns>A valid TimeSpan for the operation, or a timespan that represents -1 for infinite.</returns>
        protected virtual TimeSpan GetReceiveTimeout() => this.Description?.Endpoint?.First()?.ReceiveTimeout ?? s_InfiniteTimeout;




        /// <inheritdoc />
        protected override TResult InvokeInternal<TBody, TResult>(string method, string url, string contentType, WebHeaderCollection requestHeaders, out WebHeaderCollection responseHeaders, TBody body, NameValueCollection query)
        {
            if (null == method)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (null == url)
            {
                throw new ArgumentNullException(nameof(url));
            }

            var webrequest = this.CreateHttpRequest(url, query) as HttpWebRequest;

            if (null == webrequest)
            {
                throw new InvalidOperationException($"{nameof(CreateHttpRequest)} returned null.");
            }

            using (var cts = new CancellationTokenSource())
            {
                var cancellationtoken = cts.Token;

                var processtask = Task.Run<(TResult result, WebHeaderCollection responseHeaders)>(async () =>
                {
                    HttpWebResponse response = null;

                    try
                    {

                        webrequest.Method = method;

                        SetRequestHeaders(webrequest, requestHeaders);

                        await WriteRequestBodyAsync(contentType, body, webrequest);

                        cancellationtoken.ThrowIfCancellationRequested();

                        var responsetask = webrequest.GetResponseAsync();
                        var connecttimeouttask = Task.Delay(GetConnectTimeout());
                        var resulttask = await Task.WhenAny(connecttimeouttask, responsetask);

                        if (resulttask == connecttimeouttask)
                        {
                            throw new TimeoutException(string.Format(ErrorMessageStrings.TIMEOUT, "rest operation"));
                        }

                        response = await responsetask as HttpWebResponse;

                        var responseheaders = CopyResponseHeaders(response);

                        if (IsEmptyResponseStatus(response))
                        {
                            return (default, responseheaders);
                        }
                        else if (IsRedirectWithKeepVerb(response))
                        {

                            //TODO: Optimize this when we have our InvokeInternalAsync() in place.
                            cancellationtoken.ThrowIfCancellationRequested();
                            var redirectresult = this.InvokeInternal<TBody, TResult>(method, response.Headers[HttpResponseHeader.Location], contentType, requestHeaders, out responseheaders, body, query);
                            return (redirectresult, responseheaders);
                        }
                        else if (IsRedirectMethod(response))
                        {
                            //TODO: Optimize this when we have our InvokeInternalAsync() in place.
                            cancellationtoken.ThrowIfCancellationRequested();
                            var redirectresult = this.InvokeInternal<TBody, TResult>("GET", response.Headers[HttpResponseHeader.Location], this.Accept, requestHeaders, out responseheaders, default, query);
                            return (redirectresult, responseheaders);
                        }
                        else if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            return (default, responseheaders);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(response.ContentType)) //No content, just return an empty result.
                            {
                                return (default, responseheaders);
                            }
                            else
                            {

                                return (result: (TResult)ReadResponseBody<TResult>(response), responseheaders);
                            }
                        }
                    }
                    catch (TimeoutException e)
                    {
                        try
                        {
                            webrequest.Abort();
                        }
                        catch
#if DEBUG
                        (Exception ex)
#endif 
                        {

                        }
                        this.m_tracer.TraceError("Request timed out: {0}", e.Message);
                        throw;
                    }
                    catch (WebException e) when (e.Response is HttpWebResponse errorresponse)
                    {
                        if (errorresponse.StatusCode == HttpStatusCode.NotModified)
                        {
                            this.m_tracer.TraceInfo("Server indicates not modified {0} {1} : {2}", method, url, e.Message);
                            return (default, CopyResponseHeaders(errorresponse));
                        }
                        else if (e.Status == WebExceptionStatus.ProtocolError)
                        {
                            this.m_tracer.TraceError("Error executing {0} {1} : {2}", method, url, e.Message);
                            // Deserialize
                            object errorResult = null;

                            try
                            {
                                errorResult = ReadResponseBody<TResult>(errorresponse);
                            }
                            catch (Exception e2)
                            {
                                // De-Serialize using 
                                throw new RestClientException<object>(ErrorMessages.COMMUNICATION_RESPONSE_FAILURE, e2, e.Status, e.Response);
                            }

                            Exception exception = null;
                            if (errorResult is TResult tr2)
                            {
                                exception = new RestClientException<TResult>(tr2, e, e.Status, e.Response);
                            }
                            else if (errorresponse is TResult tr) //In other words, is TResult WebRequest or HttpWebRequest.
                            {
                                exception = new RestClientException<TResult>(tr, e, e.Status, e.Response);
                            }
                            else
                            {
                                exception = new RestClientException<object>(errorResult, e, e.Status, e.Response);
                            }

                            //switch (errorresponse.StatusCode)
                            //{
                            //    case HttpStatusCode.Unauthorized: // Validate the response
                            //        throw exception;
                            //    case (HttpStatusCode)422:
                            //        throw exception;
                            //    default:
                            //        throw exception;
                            //}

                            throw exception;
                        }
                        else if (e.Status == WebExceptionStatus.Timeout)
                        {
                            this.m_tracer.TraceError("Error executing {0} {1} : {2}", method, url, e.Message);
                            throw new TimeoutException($"Timeout executing REST operation {method} {url}", e);
                        }
                        else if (e.Status == WebExceptionStatus.ConnectFailure)
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
                    finally
                    {
                        if (null != response)
                        {
                            response.Close();
                            response.Dispose();
                        }

                        webrequest.Abort();
                    }
                });

                var receivetimeouttask = Task.Delay(GetReceiveTimeout(), cancellationtoken);
                var whenanytask = Task.WhenAny(processtask, receivetimeouttask);

                whenanytask.Wait();

                if (whenanytask.Result == receivetimeouttask)
                {
                    cts.Cancel();
                    var errormessage = string.Format(ErrorMessageStrings.TIMEOUT, "rest operation");
                    this.m_tracer.TraceError(errormessage);
                    throw new TimeoutException(errormessage);
                }
                else
                {
                    try
                    {
                        var result = processtask.Result;

                        responseHeaders = result.responseHeaders;
                        return result.result;
                    }
                    catch (AggregateException agg)
                    {
                        var flattened = agg.Flatten();

                        if (flattened.InnerException is TaskCanceledException)
                        {
                            responseHeaders = null;
                            return default;
                        }
                        else
                        {
                            throw flattened.InnerException;
                        }
                    }
                }
            }
        }

        private long? TryGetContentLength(HttpWebResponse response)
        {
            try
            {
                var length = response?.ContentLength;
                return length > 0 ? length : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Uses the <see cref="System.IO.Stream.CopyTo(Stream)"/> method to copy a stream while running an asynchronous timer to report the progress of the operation using the <see cref="RestClientBase.FireProgressChanged(string, float)"/> method.
        /// </summary>
        /// <param name="fromStream">The stream to copy from.</param>
        /// <param name="toStream">The stream to copy to.</param>
        /// <param name="byteCountingStream">The stream which reports the bytes read. This can be the same stream as <paramref name="fromStream"/> if there are no intermediate streams (e.g. a decompression stream) involved.</param>
        /// <param name="contentLength">When the content length is known, this is used to calculate the percentage completion of the operation.</param>
        /// <param name="state">Optional state string that is reported with the progress of the operation.</param>
        private void CopyStreamWithProgressReport(Stream fromStream, Stream toStream, IO.ByteCountingStream byteCountingStream, long contentLength, string state = null)
        {
            using (var reporttimer = new Timer((_) =>
            {
                var progress = contentLength > 0 ? (float)byteCountingStream.BytesRead / (float)contentLength : -1;
                this.FireProgressChanged(state, progress);
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)))
            {
                fromStream.CopyTo(toStream);
            }
        }

        /// <summary>
        /// Read the response body from the from <see cref="HttpWebResponse"/>.
        /// </summary>
        /// <typeparam name="TResult">The desired type of the result.</typeparam>
        /// <param name="response">The web response to read the body from.</param>
        /// <returns>An object that is derived from the response of the request. If <typeparamref name="TResult"/> is <see cref="T:byte[]"/>, The response will be a byte-array. If <typeparamref name="TResult"/> is <see cref="Stream"/>, then the response will be a stream. Otherwise, the type is determined by the <see cref="IContentTypeMapper"/> as part of the configuration.</returns>
        /// <remarks>
        ///     <para>Special handling is provided for some types.</para>
        ///     <para>If <typeparamref name="TResult"/> is <see cref="T:byte[]"/>, then the body will be read through a buffer into a byte array and returned, regardless of the <see cref="IContentTypeMapper"/> configured for the client.</para>
        ///     <para>If <typeparamref name="TResult"/> is <see cref="Stream"/>, then the body will be read into a <see cref="MemoryStream"/> which will be returned. <b>Callers are responsible for disposing this stream.</b></para>
        /// </remarks>
        protected virtual object ReadResponseBody<TResult>(HttpWebResponse response)
        {
            if (null == response)
            {
                return null;
            }

            var tresult = typeof(TResult);

            var compressionscheme = CompressionUtil.GetCompressionScheme(response.Headers[HttpResponseHeader.ContentEncoding]);

            if (null != compressionscheme && compressionscheme.ImplementedMethod == HttpCompressionAlgorithm.None)
            {
                compressionscheme = null;
            }

            var responselength = response.ContentLength;

            if (responselength < 0)
            {
                responselength = 0; //Later this will trigger an indeterminate progress indicator.
            }

            if (tresult == typeof(byte[])) //Fast path for byte array
            {
                //TODO: Handle large payloads that shouldn't buffer in memory.
                using (var responsestream = response.GetResponseStream())
                {
                    using (var bytecountingstream = new IO.ByteCountingStream(responsestream, false))
                    {
                        using (var memorystream = new MemoryStream())
                        {
                            if (null != compressionscheme)
                            {
                                using (var compressionstream = compressionscheme.CreateDecompressionStream(bytecountingstream))
                                {
                                    CopyStreamWithProgressReport(compressionstream, memorystream, bytecountingstream, responselength);
                                    //compressionstream.CopyTo(memorystream);
                                }
                            }
                            else
                            {
                                CopyStreamWithProgressReport(bytecountingstream, memorystream, bytecountingstream, responselength);
                                //responsestream.CopyTo(memorystream);
                            }

                            var responsebytes = memorystream.ToArray();

                            if (this.Description.Trace)
                            {
                                this.m_tracer.TraceVerbose("HTTP << {0}", Convert.ToBase64String(responsebytes));
                            }

                            return responsebytes;
                        }
                    }
                }
            }
            else if (tresult == typeof(Stream)) //Fast path for stream
            {
                //TODO: Handle large payloads that shouldn't buffer in memory.
                using (var responsestream = response.GetResponseStream())
                {
                    using (var bytecountingstream = new IO.ByteCountingStream(responsestream, false))
                    {
                        var memorystream = new MemoryStream(); //Not in a using because we're going to be returning it. It will be the caller's responsibility to dispose of it.

                        if (null != compressionscheme)
                        {
                            using (var compressionstream = compressionscheme.CreateDecompressionStream(NonDisposingStream.Create(responsestream)))
                            {
                                //compressionstream.CopyTo(memorystream);
                                CopyStreamWithProgressReport(compressionstream, memorystream, bytecountingstream, responselength);
                            }
                        }
                        else
                        {
                            CopyStreamWithProgressReport(bytecountingstream, memorystream, bytecountingstream, responselength);
                            //responsestream.CopyTo(memorystream);
                        }

                        if (this.Description.Trace)
                        {
                            this.m_tracer.TraceVerbose("HTTP << {0}", Convert.ToBase64String(memorystream.ToArray()));
                        }

                        memorystream.Seek(0, SeekOrigin.Begin); //Reset the stream's position so that callers are ready to read the buffered response.
                        return memorystream;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(response.ContentType))
            {
                var responsemimetype = new ContentType(response.ContentType);

                var responseserializer = this.Description.Binding.ContentTypeMapper.GetSerializer(responsemimetype);

                using (var memorystream = new MemoryStream())
                {
                    if (this.Description.Trace)
                    {
                        this.m_tracer.TraceVerbose("Received response {0} : {1} bytes", response.ContentType, response.ContentLength);
                    }

                    using (var responsestream = response.GetResponseStream())
                    {
                        using (var bytecountingstream = new IO.ByteCountingStream(responsestream, false))
                        {
                            CopyStreamWithProgressReport(bytecountingstream, memorystream, bytecountingstream, responselength);
                            //responsestream.CopyTo(memorystream);
                        }
                    }

                    memorystream.Seek(0, SeekOrigin.Begin);

                    if (this.Description.Trace)
                    {
                        this.m_tracer.TraceVerbose("HTTP << {0}", Convert.ToBase64String(memorystream.ToArray()));
                    }

                    if (null != compressionscheme)
                    {
                        using (var compressionstream = compressionscheme.CreateDecompressionStream(NonDisposingStream.Create(memorystream)))
                        {
                            return responseserializer.DeSerialize(compressionstream, responsemimetype, typeof(TResult));
                        }
                    }
                    else
                    {
                        return responseserializer.DeSerialize(memorystream, responsemimetype, typeof(TResult));
                    }
                }
            }
            else
            {
                return null;
            }
        }

        private bool IsEmptyResponseStatus(HttpWebResponse response) =>
            response.StatusCode == HttpStatusCode.NoContent ||
            response.StatusCode == HttpStatusCode.Continue ||
            response.StatusCode == HttpStatusCode.NotModified;

        private bool IsRedirectWithKeepVerb(HttpWebResponse response) => response.StatusCode == HttpStatusCode.RedirectKeepVerb;
        private bool IsRedirectMethod(HttpWebResponse response) => response.StatusCode == HttpStatusCode.RedirectMethod;

        /// <summary>
        /// Copies the response headers to a separate object that is not tied to the response.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        protected virtual WebHeaderCollection CopyResponseHeaders(HttpWebResponse response)
        {
            var responseheaders = new WebHeaderCollection();

            foreach (string header in response.Headers)
            {
                var values = response.Headers.GetValues(header);
                foreach (string val in values)
                {
                    responseheaders.Add(header, val);
                }
            }

            return responseheaders;
        }

        /// <summary>
        /// Write the body of the request if provided to the <see cref="HttpWebRequest"/>.
        /// </summary>
        /// <typeparam name="TBody">The type of body that is provided.</typeparam>
        /// <param name="contentType">The content type header that will be the type of content to send in the request.</param>
        /// <param name="body">The object representing the body of the request.</param>
        /// <param name="webrequest">The request object to write the body to.</param>
        /// <returns>A task that can be awaited while the operation completes.</returns>
        /// <exception cref="InvalidOperationException">Can be thrown when no serializer is available for the <typeparamref name="TBody"/> type.</exception>
        /// <remarks>
        ///   <para>Special handling is provided for some types of <typeparamref name="TBody"/>:</para>
        ///   <para>If <typeparamref name="TBody"/> is <see cref="T:byte[]"/>, the body will be copied directly from the array to the request stream.</para>
        ///   <para>If <typeparamref name="TBody"/> is <see cref="Stream"/>, the body will be copied directly from the stream to the request stream.</para>
        /// </remarks>
        protected virtual async Task WriteRequestBodyAsync<TBody>(string contentType, TBody body, HttpWebRequest webrequest)
        {
            if (!string.IsNullOrEmpty(contentType) && !EqualityComparer<TBody>.Default.Equals(default(TBody), body))
            {
                var mimetype = new ContentType(contentType);

                var compressionscheme = this.Description?.Binding?.CompressRequests == true ? CompressionUtil.GetCompressionScheme(this.Description.Binding.OptimizationMethod) : null;

                if (null != compressionscheme && compressionscheme.ImplementedMethod == HttpCompressionAlgorithm.None) //We don't do anything for the no compression option.
                {
                    compressionscheme = null;
                }

                if (body is byte[] bytes) //Fast path for byte arrays when transfering blobs.
                {
                    using (var requeststream = await webrequest.GetRequestStreamAsync())
                    {
                        using (var memorystream = new MemoryStream(bytes))
                        {
                            memorystream.Seek(0, SeekOrigin.Begin);

                            if (null != compressionscheme)
                            {
                                AddRequestCompressionHeaders(webrequest, compressionscheme);

                                using (var compressionstream = compressionscheme.CreateCompressionStream(NonDisposingStream.Create(requeststream)))
                                {
                                    memorystream.CopyTo(compressionstream);
                                }
                            }
                            else
                            {
                                memorystream.CopyTo(requeststream);
                            }

                            if (this.Description.Trace)
                            {
                                this.m_tracer.TraceVerbose("HTTP >> {0}", Convert.ToBase64String(memorystream.ToArray()));
                            }
                        }
                    }
                }
                else if (body is Stream bodystream && bodystream.CanRead && bodystream.CanSeek) //Fast path for streams.
                {
                    using (bodystream) //TODO: Remove this if we need to reuse the body stream after copy.
                    {
                        using (var requeststream = await webrequest.GetRequestStreamAsync())
                        {
                            bodystream.Seek(0, SeekOrigin.Begin);

                            if (null != compressionscheme)
                            {
                                AddRequestCompressionHeaders(webrequest, compressionscheme);

                                using (var compressionstream = compressionscheme.CreateCompressionStream(NonDisposingStream.Create(requeststream)))
                                {
                                    bodystream.CopyTo(compressionstream);
                                }
                            }
                            else
                            {
                                bodystream.CopyTo(requeststream);
                            }
                        }

                        if (this.Description.Trace)
                        {
                            this.m_tracer.TraceVerbose("HTTP >> [Stream]");
                        }
                    }
                }
                else
                {
                    var serializer = this.Description?.Binding?.ContentTypeMapper?.GetSerializer(mimetype);

                    if (null == serializer)
                    {
                        throw new InvalidOperationException($"Serializer is missing for content type: {contentType}.");
                    }

                    using (var memorystream = new MemoryStream())
                    {
                        if (null != compressionscheme)
                        {
                            AddRequestCompressionHeaders(webrequest, compressionscheme);

                            using (var compressionstream = compressionscheme.CreateCompressionStream(NonDisposingStream.Create(memorystream)))
                            {
                                serializer.Serialize(compressionstream, body, out mimetype); //TODO: Question; Why do we provide mime this in both places - JF - This is so the body serializer can access extended parts of the mime type when needed (so: application/xml+XXYYZZ would use the XmlBodySerializer but may need to know XXYYZZ was passed as an extension)
                            }
                        }
                        else
                        {
                            serializer.Serialize(NonDisposingStream.Create(memorystream), body, out mimetype);
                        }

                        if (this.Description.Trace)
                        {
                            this.m_tracer.TraceVerbose("HTTP >> {0}", Convert.ToBase64String(memorystream.ToArray()));
                        }

                        memorystream.Seek(0, SeekOrigin.Begin);

                        webrequest.ContentType = mimetype.ToString();

                        using (var requeststream = await webrequest.GetRequestStreamAsync())
                        {
                            await memorystream.CopyToAsync(requeststream);
                        }
                    }
                }
            }
        }

        private static void AddRequestCompressionHeaders(HttpWebRequest webrequest, ICompressionScheme compressionscheme)
        {
            webrequest.Headers.Add(HttpRequestHeader.ContentEncoding, compressionscheme.AcceptHeaderName);
            webrequest.Headers.Add("X-CompressRequestStream", compressionscheme.AcceptHeaderName);
        }

#if DEBUG
        /// <inheritdoc />
        protected TResult InvokeInternalOld<TBody, TResult>(string method, string url, string contentType, WebHeaderCollection additionalHeaders, out WebHeaderCollection responseHeaders, TBody body, NameValueCollection query)
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
            SetRequestHeaders(requestObj, additionalHeaders);

#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            // Get request object

            // Body was provided?
            try
            {
                var mimeContentType = new ContentType(contentType);
                // Try assigned credentials
                IBodySerializer serializer = null;
                if (body != null)
                {
                    if (contentType == null && typeof(TResult) != typeof(Object))
                    {
                        throw new ArgumentNullException(nameof(contentType));
                    }

                    serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(mimeContentType);
                    // Serialize and compress with deflate
                    using (MemoryStream ms = new MemoryStream())
                    {
                        if (this.Description.Binding.CompressRequests)
                        {
                            var compressionScheme = CompressionUtil.GetCompressionScheme(this.Description.Binding.OptimizationMethod);
                            if (compressionScheme.ImplementedMethod != HttpCompressionAlgorithm.None)
                            {
                                requestObj.Headers.Add("Content-Encoding", compressionScheme.AcceptHeaderName);
                                requestObj.Headers.Add("X-CompressRequestStream", compressionScheme.AcceptHeaderName);
                            }

                            using (var str = compressionScheme.CreateCompressionStream(NonDisposingStream.Create(ms)))
                            {
                                serializer.Serialize(str, body, out mimeContentType);
                            }
                        }
                        else
                        {
                            serializer.Serialize(ms, body, out mimeContentType);
                        }

                        // Trace
                        if (this.Description.Trace)
                        {
                            this.m_tracer.TraceVerbose("HTTP >> {0}", Convert.ToBase64String(ms.ToArray()));
                        }

                        // Get request object
                        ms.Seek(0, SeekOrigin.Begin);
                        requestObj.ContentType = mimeContentType.ToString();
                        using (var requestTask = Task.Run(async () => { return await requestObj.GetRequestStreamAsync(); }))
                        {
                            try
                            {
                                using (requestTask.Result)
                                {
                                    ms.CopyTo(requestTask.Result);
                                }
                            }
                            catch (AggregateException e)
                            {
                                requestObj.Abort();
                                throw e.InnerExceptions.First();
                            }
                        }
                    }
                }

                // Response
                HttpWebResponse response = null;
                try
                {
                    var cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.CancelAfter(this.Description.Endpoint[0].ConnectTimeout);
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
                        if (String.IsNullOrEmpty(response.ContentType))
                        {
                            return default(TResult);
                        }
                        var responseContentType = new ContentType(response.ContentType);


                        if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            return default(TResult);
                        }

                        serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(responseContentType);

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
                                    retVal = (TResult)serializer.DeSerialize(str, responseContentType, typeof(TResult));
                                }
                            }
                            else
                            {
                                retVal = (TResult)serializer.DeSerialize(ms, responseContentType, typeof(TResult));
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

                var responseContentType = new ContentType(e.Response.ContentType);

                var ms = new MemoryStream(); // copy response to memory
                using (var str = CompressionUtil.GetCompressionScheme(errorResponse.Headers[HttpResponseHeader.ContentEncoding]).CreateDecompressionStream(errorResponse.GetResponseStream()))
                {
                    str.CopyTo(ms);
                }
                ms.Seek(0, SeekOrigin.Begin);

                try
                {
                    var serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(responseContentType);

                    if (!String.IsNullOrEmpty(errorResponse.Headers[HttpResponseHeader.ContentEncoding]))
                    {
                        errorResult = serializer.DeSerialize(ms, responseContentType, typeof(TResult));
                    }
                    else
                    {
                        errorResult = serializer.DeSerialize(ms, responseContentType, typeof(TResult));
                    }
                }
                catch (Exception e2)
                {
                    // De-Serialize using 
                    throw new RestClientException<object>(ErrorMessages.COMMUNICATION_RESPONSE_FAILURE, e2, e.Status, e.Response);
                }

                Exception exception = null;
                if (errorResult is TResult tr2)
                {
                    exception = new RestClientException<TResult>(tr2, e, e.Status, e.Response);
                }
                else if (errorResponse is TResult tr)
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
#endif

        /// <summary>
        /// Sets the request headers on the <paramref name="webRequest"/> using <paramref name="requestHeaders"/>. Special headers that require parsing are handled.
        /// </summary>
        /// <param name="webRequest">The request to set the headers on.</param>
        /// <param name="requestHeaders">The headers to set.</param>
        protected virtual void SetRequestHeaders(HttpWebRequest webRequest, WebHeaderCollection requestHeaders)
        {
            if (requestHeaders != null)
            {
                foreach (var hdr in requestHeaders.AllKeys)
                {
                    if (hdr == "If-Modified-Since")
                    {
                        webRequest.IfModifiedSince = DateTime.Parse(requestHeaders[hdr]);
                    }
                    else
                    {
                        webRequest.Headers.Add(hdr, requestHeaders[hdr]);
                    }
                }
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