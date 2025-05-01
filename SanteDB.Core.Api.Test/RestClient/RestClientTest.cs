/*
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
using NUnit.Framework;
using SanteDB.Core.Configuration.Http;
using SanteDB.Core.Http;
using SanteDB.Core.Http.Description;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SanteDB.Core.Api.Test.RestClient
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    internal class RestClientTest
    {
        private RestClientTestServer _TestServer;

        private string _Host;

        /// <summary>
        /// Initializes the JWS test
        /// </summary>
        [OneTimeSetUp]
        public void SetUp()
        {
            TestApplicationContext.TestAssembly = typeof(RestClientTest).Assembly;
            TestApplicationContext.Initialize(TestContext.CurrentContext.TestDirectory);

            if (null != _TestServer)
            {
                _TestServer.Dispose();
            }
            _TestServer = new RestClientTestServer();
            _Host = $"http://localhost:{_TestServer.PortNumber}/";
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _TestServer.Dispose();
            _TestServer = null;
        }

        private RestClientDescriptionConfiguration GetBasicConfig(HttpCompressionAlgorithm compressionAlgorithm = HttpCompressionAlgorithm.None, TimeSpan? timeout = null)
        {
            var config = new RestClientDescriptionConfiguration();

            config.Accept = "*/*";
            config.Binding = new RestClientBindingConfiguration();
            config.Binding.ContentTypeMapper = new DefaultContentTypeMapper();
            config.Binding.OptimizationMethod = compressionAlgorithm;
            config.Binding.CompressRequests = true;
            config.Binding.Security = new RestClientSecurityConfiguration();
            config.Binding.Security.Mode = SecurityScheme.None;
            config.Binding.Security.PreemptiveAuthentication = false;
            config.Endpoint = new List<RestClientEndpointConfiguration>()
            {
                new RestClientEndpointConfiguration(_Host, timeout)
            };

            return config;
        }

        /* Removed until I can isolate this issue on Linux
        [Test]
        public void RestClient_HttpGet()
        {
            var fixture = new Core.Http.RestClient(GetBasicConfig());

            var response = fixture.Get<RequestResponse>("/");

            Assert.NotNull(response);
            Assert.AreEqual("GET", response.Method);
            Assert.AreEqual("/", response.Path);
        }

        [Test]
        public void RestClient_HttpPostBytes()
        {
            var fixture = new Core.Http.RestClient(GetBasicConfig());

            var response = fixture.Post<byte[], byte[]>("/echo", Encoding.UTF8.GetBytes("Hello World"));

            Assert.NotNull(response);

            var actual = Encoding.UTF8.GetString(response);

            Assert.AreEqual("Hello World", actual);
        }

        [Test]
        public void RestClient_HttpGetBytes()
        {
            var fixture = new Core.Http.RestClient(GetBasicConfig());

            var response = fixture.Get("/");

            Assert.NotNull(response);

            string responsetext = null;

            using (var sr = new StreamReader(new MemoryStream(response)))
            {
                responsetext = sr.ReadToEnd();
            }

            var actual = JsonConvert.DeserializeObject<RequestResponse>(responsetext);

            Assert.NotNull(actual);
            Assert.AreEqual("/", actual.Path);
            Assert.AreEqual("GET", actual.Method);
        }

        [Test]
        public void RestClient_HttpConnectTimeout()
        {
            var config = GetBasicConfig();
            config.Endpoint[0].ConnectTimeout = TimeSpan.FromSeconds(2);

            var fixture = new Core.Http.RestClient(config);

            var sw = Stopwatch.StartNew();
            try
            {
                var response = fixture.Get<RequestResponse>("/delay/10");
            }
            catch (TimeoutException)
            {
                sw.Stop();
                var elapsed = sw.Elapsed.TotalSeconds;
                Assert.GreaterOrEqual(elapsed, 2d);
                Assert.Less(elapsed, 2.5d);
                return;
            }

            Assert.Fail();

        }

        [Test]
        public void RestClient_HttpReceiveTimeout()
        {
            var config = GetBasicConfig();
            config.Endpoint[0].ConnectTimeout = TimeSpan.FromSeconds(2);
            config.Endpoint[0].ReceiveTimeout = TimeSpan.FromSeconds(5);

            var fixture = new Core.Http.RestClient(config);

            var sw = Stopwatch.StartNew();
            try
            {
                var response = fixture.Get<RequestResponse>("/streamdelay/50");
            }
            catch (TimeoutException)
            {
                sw.Stop();
                var elapsed = sw.Elapsed.TotalSeconds;
                Assert.GreaterOrEqual(elapsed, 5d);
                Assert.Less(elapsed, 5.5d);
                return;
            }

            Assert.Fail();

        }

        [Test]
        public void RestClient_HttpReceiveInfiniteTimeout()
        {
            var config = GetBasicConfig();
            config.Endpoint[0].ConnectTimeout = TimeSpan.FromSeconds(2);
            config.Endpoint[0].ReceiveTimeout = null;

            var fixture = new Core.Http.RestClient(config);

            try
            {
                var response = fixture.Get<RequestResponse>("/streamdelay/7");
            }
            catch (TimeoutException)
            {
                Assert.Fail();
                return;
            }

        }

        */
    }
}
