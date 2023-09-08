using Newtonsoft.Json;
using NUnit.Framework;
using SanteDB.Core.Configuration.Http;
using SanteDB.Core.Http;
using SanteDB.Core.Http.Description;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public void RestClient_HttpTimeout()
        {
            var config = GetBasicConfig();
            config.Endpoint[0].Timeout = TimeSpan.FromSeconds(2);

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
    }
}
