﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test.RestClient
{
    internal class RestClientTestServer : IDisposable
    {
        readonly HttpListener _Listener;
        readonly CancellationTokenSource _Cts;
        private bool disposedValue;
        private Task _ListenLoop;


        public RestClientTestServer()
        {
            _Cts = new CancellationTokenSource();
            _Listener = new HttpListener();
            PortNumber = GetRandomOpenPort();
            Console.WriteLine($"{nameof(RestClientTestServer)}::PortNumber={PortNumber}");
            _Listener.Prefixes.Add($"http://localhost:{PortNumber}/");
            _Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _Listener.Start();
            Console.WriteLine($"{nameof(RestClientTestServer)}::_Listener.Start() - Success");
            _ListenLoop = Task.Run(ListenerMainLoop, _Cts.Token);
        }

        private async Task ListenerMainLoop()
        {
            var ct = _Cts.Token;

            while (!ct.IsCancellationRequested)
            {
                HttpListenerContext ctx = null;

                try
                {
                    ctx = await _Listener.GetContextAsync();
                }
                catch (ObjectDisposedException)
                {
                }
                catch (System.Net.HttpListenerException)
                {
                }

                if (null != ctx)
                {
                    try
                    {
                        await HandleRequestAsync(ctx, ct);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{nameof(RestClientTestServer)}::ListenerMainLoop - Unhandled Exception (Returning 500)\n{ex.ToString()}");
                        ctx.Response.StatusCode = 500;
                        ctx.Response.StatusDescription = "Internal Server Error";
                        ctx.Response.Close();
                    }

                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken token = default)
        {
            var routepath = context.Request.Url.AbsolutePath;

            if (routepath.StartsWith("//")) //Remove a double slash from the test server?
            {
                routepath = routepath.Substring(1);
            }

            if (!routepath.StartsWith("/"))
            {
                Console.WriteLine($"{nameof(RestClientTestServer)}::{nameof(HandleRequestAsync)} - received bad request, executing handler.");
                await HandleBadRequestAsync(context.Request, context.Response, token);
            }
            else if (routepath.IndexOf('/', 1) > 0)
            {
                routepath = routepath.Substring(0, routepath.IndexOf('/', 1));
            }

            Console.WriteLine($"{nameof(RestClientTestServer)}::{nameof(HandleRequestAsync)}:routepath=\"{routepath}\".");

            switch (routepath)
            {
                case "/streamdelay":
                    await HandleStreamDelayRequestAsync(context.Request, context.Response, token);
                    break;
                case "/delay":
                    await HandleResponseDelayRequestAsync(context.Request, context.Response, token);
                    break;
                case "/badrequest":
                    await HandleBadRequestAsync(context.Request, context.Response, token);
                    break;
                case "/echo":
                    await HandleEchoRequestAsync(context.Request, context.Response, token);
                    break;
                default:
                    await HandleOtherRequestAsync(context.Request, context.Response, token);
                    break;
            }
        }

        private async Task HandleStreamDelayRequestAsync(HttpListenerRequest request, HttpListenerResponse response, CancellationToken token = default)
        {
            var delay = TimeSpan.FromMilliseconds(10);
            try
            {
                var delayamountstring = request.Url.AbsolutePath.Substring(request.Url.AbsolutePath.IndexOf('/', 1) + 1);

                if (int.TryParse(delayamountstring, out var s))
                {
                    delay = TimeSpan.FromMilliseconds(s);
                }

                else if (TimeSpan.TryParse(delayamountstring, out var d))
                {
                    delay = d;
                }
            }
            catch { }

            Console.WriteLine($"{nameof(RestClientTestServer)}::{nameof(HandleStreamDelayRequestAsync)}:delay={delay}");

            response.StatusCode = 200;
            response.StatusDescription = "OK";
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.AddHeader("Server", $"{nameof(RestClientTestServer)} 1.0");
            response.AddHeader("Date", DateTimeOffset.UtcNow.ToString("u"));
            var responsebody = new RequestResponse(request);

            if (null != request.InputStream)
            {
                using (var sr = new StreamReader(request.InputStream, Encoding.UTF8, false, 1024, true))
                {
                    responsebody.RequestContent = await sr.ReadToEndAsync();
                }
            }

            var responsebodystr = JsonConvert.SerializeObject(responsebody);

            var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, Encoding.UTF8, 1024, true))
            {
                await sw.WriteLineAsync(responsebodystr);
            }

            ms.Seek(0, SeekOrigin.Begin);

            response.ContentLength64 = ms.Length;

            int b = -1;
            while (!token.IsCancellationRequested && (b = ms.ReadByte()) >= 0)
            {
                await Task.Delay(delay, token);
                token.ThrowIfCancellationRequested();
                response.OutputStream.WriteByte((byte)b);
                await response.OutputStream.FlushAsync();
            }

            response.Close();
        }

        private async Task HandleResponseDelayRequestAsync(HttpListenerRequest request, HttpListenerResponse response, CancellationToken token = default)
        {
            var delay = TimeSpan.FromSeconds(5);
            try
            {
                var delayamountstring = request.Url.AbsolutePath.Substring(request.Url.AbsolutePath.IndexOf('/', 1) + 1);

                if (int.TryParse(delayamountstring, out var s))
                {
                    delay = TimeSpan.FromSeconds(s);
                }
                else if (TimeSpan.TryParse(delayamountstring, out var d))
                {
                    delay = d;
                }
            }
            catch { }

            Console.WriteLine($"{nameof(RestClientTestServer)}::{nameof(HandleResponseDelayRequestAsync)}:delay={delay}");

            await Task.Delay(delay, token);
            await HandleOtherRequestAsync(request, response, token);
        }

        private async Task HandleOtherRequestAsync(HttpListenerRequest request, HttpListenerResponse response, CancellationToken token = default)
        {
            Console.WriteLine($"{nameof(RestClientTestServer)}::{nameof(HandleOtherRequestAsync)}");

            response.StatusCode = 200;
            response.StatusDescription = "OK";
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.AddHeader("Server", $"{nameof(RestClientTestServer)} 1.0");
            response.AddHeader("Date", DateTimeOffset.UtcNow.ToString("u"));
            var responsebody = new RequestResponse(request);

            if (null != request.InputStream)
            {
                using (var sr = new StreamReader(request.InputStream, Encoding.UTF8, false, 1024, true))
                {
                    responsebody.RequestContent = await sr.ReadToEndAsync();
                }
            }

            var responsebodystr = JsonConvert.SerializeObject(responsebody);

            using (var sw = new StreamWriter(response.OutputStream, Encoding.UTF8, 1024, true))
            {
                await sw.WriteLineAsync(responsebodystr);
            }

            response.Close();
        }

        private async Task HandleEchoRequestAsync(HttpListenerRequest request, HttpListenerResponse response, CancellationToken token = default)
        {
            Console.WriteLine($"{nameof(RestClientTestServer)}::{nameof(HandleEchoRequestAsync)}");

            response.StatusCode = 200;
            response.StatusDescription = "OK";
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.AddHeader("Server", $"{nameof(RestClientTestServer)} 1.0");
            response.AddHeader("Date", DateTimeOffset.UtcNow.ToString("u"));
            var responsebody = new RequestResponse(request);

            if (null != request.InputStream)
            {
                await request.InputStream.CopyToAsync(response.OutputStream);
            }

            response.Close();
        }

        private async Task HandleBadRequestAsync(HttpListenerRequest request, HttpListenerResponse response, CancellationToken token = default)
        {
            response.StatusCode = 400;
            response.StatusDescription = "Bad Request";
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.AddHeader("Server", $"{nameof(RestClientTestServer)} 1.0");
            response.AddHeader("Date", DateTimeOffset.UtcNow.ToString("u"));
            var responsebody = new RequestResponse(request);

            if (null != request.InputStream)
            {
                using (var sr = new StreamReader(request.InputStream, Encoding.UTF8, false, 1024, true))
                {
                    responsebody.RequestContent = await sr.ReadToEndAsync();
                }
            }

            var responsebodystr = JsonConvert.SerializeObject(responsebody);

            using (var sw = new StreamWriter(response.OutputStream, Encoding.UTF8, 1024, true))
            {
                await sw.WriteLineAsync(responsebodystr);
            }

            response.Close();
        }

        public int PortNumber { get; }

        private static int GetRandomOpenPort()
        {
            var tcplistener = new TcpListener(IPAddress.Loopback, 0);
            tcplistener.Start();
            var port = (tcplistener.LocalEndpoint as IPEndPoint).Port;
            tcplistener.Stop();
            return port;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (null != _Listener)
                    {
                        _Cts.Cancel();

                        try
                        {
                            _Listener.Abort();
                            _Listener.Stop();
                            _Listener.Close();
                        }
                        catch (ObjectDisposedException)
                        {

                        }

                        if (!_ListenLoop.IsCompleted)
                        {
                            _ListenLoop.Wait();
                        }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RestClientTestServer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
