using NUnit.Framework;
using SanteDB.Core.Configuration.Http;
using SanteDB.Core.Http;
using SanteDB.Core.Http.Description;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test.RestClient
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    internal class RestClientBaseTest
    {
        /// <summary>
        /// Initializes the JWS test
        /// </summary>
        [OneTimeSetUp]
        public void Initialize()
        {
            TestApplicationContext.TestAssembly = typeof(RestClientBaseTest).Assembly;
            TestApplicationContext.Initialize(TestContext.CurrentContext.TestDirectory);
        }

        private RestClientDescriptionConfiguration GetBasicEndpointDescription(string address = "http://test.fixture/endpoint", TimeSpan timeout = default, RestRequestCredentials credentials = null)
        {
            var config = new RestClientDescriptionConfiguration();
            config.Endpoint.Add(new RestClientEndpointConfiguration
            {
                Address = address,
                ConnectTimeout = timeout
            });

            config.Binding = new RestClientBindingConfiguration
            {
                Security = new RestClientSecurityConfiguration
                {
                    CredentialProvider = new TestCredentialProvider(credentials ?? new HttpBasicCredentials("bob", "password"))
                }
            };

            return config;
        }

        [Test]
        public void TestCreateCorrectRequestUri_SimplePath()
        {
            var config = GetBasicEndpointDescription();

            var fixture = new RestClientBaseFixture(config);

            var expected = new Uri("http://test.fixture/endpoint/test");
            var actual = fixture.CreateCorrectRequestUri("test", null);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestCreateCorrectRequestUri_LeadingSlash()
        {
            var config = GetBasicEndpointDescription();

            var fixture = new RestClientBaseFixture(config);

            var expected = new Uri("http://test.fixture/endpoint/test");
            var actual = fixture.CreateCorrectRequestUri("/test", null);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestCreateCorrectRequestUri_ComplexPath()
        {
            var config = GetBasicEndpointDescription(address: "http://test.fixture/endpoint?version=1.0");

            var fixture = new RestClientBaseFixture(config);

            var expected = new Uri("http://test.fixture/endpoint/test?version=1.0");
            var actual = fixture.CreateCorrectRequestUri("test", null);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestCreateCorrectRequestUri_QueryString()
        {
            var config = GetBasicEndpointDescription();

            var fixture = new RestClientBaseFixture(config);

            var query = new NameValueCollection
            {
                { "q1", "foo" },
                { "q2", "bar" }
            };

            var expected = new Uri("http://test.fixture/endpoint/test?q1=foo&q2=bar");
            var actual = fixture.CreateCorrectRequestUri("test", query);

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void TestGetRequestCredentials_InClient()
        {
            var config = GetBasicEndpointDescription();

            var fixture = new RestClientBaseFixture(config);

            var expected = new HttpBasicCredentials("test", "testpass");
            fixture.Credentials = expected;

            config.Binding.Security.PreemptiveAuthentication = true;
            var actual = fixture.GetRequestCredentials();
            Assert.AreEqual(expected, actual);

            config.Binding.Security.PreemptiveAuthentication = false;
            actual = fixture.GetRequestCredentials();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGetResponseCredentials_WithPreemptive()
        {
            var config = GetBasicEndpointDescription();

            var fixture = new RestClientBaseFixture(config);

            config.Binding.Security.PreemptiveAuthentication = true;
            var expected = config.Binding.Security.CredentialProvider.GetCredentials(fixture);
            var actual = fixture.GetRequestCredentials();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestGetResponseCredentials_WithoutPreemptive()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            config.Binding.Security.PreemptiveAuthentication = false;

            var actual = fixture.GetRequestCredentials();

            Assert.IsNull(actual);
        }

        [Test]
        public void TestInvoke_CancelRequest()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);
            var called = false;

            fixture.Requesting += (sender, args) =>
            {
                called = true;
                args.Cancel = true;
            };

            var actual = fixture.Invoke<string, RestClientTestResponse<string>>("TEST", "test", "body");

            Assert.IsTrue(called);
            Assert.IsNull(actual);
        }

        [Test]
        public void TestCreateHttpRequest_MissingConfiguration()
        {
            var fixture = new RestClientBaseFixture();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var request = fixture.CreateHttpRequest("test", null);
                Assert.Fail();
            });

            fixture = new RestClientBaseFixture(new RestClientDescriptionConfiguration());

            Assert.Throws<InvalidOperationException>(() =>
            {
                var request = fixture.CreateHttpRequest("test", null);
                Assert.Fail();
            });

            fixture = new RestClientBaseFixture(new RestClientDescriptionConfiguration() { Endpoint = new List<RestClientEndpointConfiguration>() });

            Assert.Throws<InvalidOperationException>(() =>
            {
                var request = fixture.CreateHttpRequest("test", null);
                Assert.Fail();
            });
        }

        [Test]
        public void TestCreateHttpRequest_UriHandling()
        {
            var config = GetBasicEndpointDescription();

            var fixture = new RestClientBaseFixture(config);

            var expected = "http://test.fixture/endpoint/test";
            var actual = fixture.CreateHttpRequest("test", null).RequestUri.ToString();
            Assert.AreEqual(expected, actual);

            actual = fixture.CreateHttpRequest("./test", null).RequestUri.ToString();
            Assert.AreEqual(expected, actual);

            actual = fixture.CreateHttpRequest("../endpoint/test", null).RequestUri.ToString();
            Assert.AreEqual(expected, actual);

            actual = fixture.CreateHttpRequest("./test/../test", null).RequestUri.ToString();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestCreateHttpRequest_Credentials()
        {
            var config = GetBasicEndpointDescription();

            var fixture = new RestClientBaseFixture(config);

            fixture.Credentials = new HttpBasicCredentials("mycred", "password"); ;

            var expected = $"BASIC {Convert.ToBase64String(Encoding.UTF8.GetBytes("mycred:password"))}";
            var actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.Authorization];

            Assert.AreEqual (expected, actual);

        }

        [Test]
        public void TestCreateHttpRequest_AcceptHeader()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            fixture.Accept = "x-test/plain";

            var expected = "x-test/plain";
            var actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.Accept];

            Assert.AreEqual (expected, actual); 
        }

        [Test]
        public void TestCreateHttpRequest_AcceptEncoding()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            config.Binding.OptimizationMethod = HttpCompressionAlgorithm.None;
            var actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.AcceptEncoding];

            Assert.IsNull(actual);

            config.Binding.CompressRequests = true;
            actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.AcceptEncoding];

            Assert.IsNull(actual);

            var expected = "deflate";
            config.Binding.OptimizationMethod = HttpCompressionAlgorithm.Deflate;
            actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.AcceptEncoding];

            Assert.AreEqual(expected, actual);

            expected = "gzip";
            config.Binding.OptimizationMethod = HttpCompressionAlgorithm.Gzip;
            actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.AcceptEncoding];

            Assert.AreEqual(expected, actual);

            expected = "bzip2";
            config.Binding.OptimizationMethod = HttpCompressionAlgorithm.Bzip2;
            actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.AcceptEncoding];

            Assert.AreEqual(expected, actual);

            expected = "lzma";
            config.Binding.OptimizationMethod = HttpCompressionAlgorithm.Lzma;
            actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.AcceptEncoding];

            Assert.AreEqual(expected, actual);

            expected = "lzma,bzip2,gzip,deflate";
            config.Binding.OptimizationMethod = HttpCompressionAlgorithm.Gzip | HttpCompressionAlgorithm.Bzip2 | HttpCompressionAlgorithm.Lzma | HttpCompressionAlgorithm.Deflate;
            actual = fixture.CreateHttpRequest("test", null).Headers[System.Net.HttpRequestHeader.AcceptEncoding];

            Assert.AreEqual(expected, actual);

        }

        [Test]
        public void TestRequest_HttpGet()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            var response = fixture.Get<RestClientTestResponse>("test");

            Assert.NotNull(response);
            Assert.AreEqual("GET", response.Method);
            Assert.IsNull(response.Query);
            Assert.AreEqual("test", response.Url);
        }

        [Test]
        public void TestRequest_HttpOptions()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            var response = fixture.Options<RestClientTestResponse>("test");

            Assert.NotNull(response);
            Assert.AreEqual("OPTIONS", response.Method);
            Assert.IsNull(response.Query);
            Assert.AreEqual("test", response.Url);
        }

        [Test]
        public void TestRequest_HttpHead()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            fixture.Requesting += (sender, args) =>
            {
                Assert.AreEqual("HEAD", args.Method);
                Assert.AreEqual("test", args.Url);
            };

            var response = fixture.Head("test");

            Assert.NotNull(response);
            Assert.AreEqual(nameof(RestClientBaseFixture), response["Server"]);
        }

        [Test]
        public void TestRequest_HttpPost()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            var response = fixture.Post<string, RestClientTestResponse<string>>("test", "test body");

            Assert.NotNull(response);
            Assert.AreEqual("POST", response.Method);
            Assert.IsNull(response.Query);
            Assert.AreEqual("test", response.Url);
            Assert.AreEqual("test body", response.Body);
        }

        [Test]
        public void TestRequest_HttpPut()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            var response = fixture.Put<string, RestClientTestResponse<string>>("test", "test body");

            Assert.NotNull(response);
            Assert.AreEqual("PUT", response.Method);
            Assert.IsNull(response.Query);
            Assert.AreEqual("test", response.Url);
            Assert.AreEqual("test body", response.Body);
        }

        [Test]
        public void TestRequest_HttpLock()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            var response = fixture.Lock<RestClientTestResponse>("test");

            Assert.NotNull(response);
            Assert.AreEqual("LOCK", response.Method);
            Assert.IsNull(response.Query);
            Assert.AreEqual("test", response.Url);
        }

        [Test]
        public void TestRequest_HttpUnlock()
        {
            var config = GetBasicEndpointDescription();
            var fixture = new RestClientBaseFixture(config);

            var response = fixture.Unlock<RestClientTestResponse>("test");

            Assert.NotNull(response);
            Assert.AreEqual("UNLOCK", response.Method);
            Assert.IsNull(response.Query);
            Assert.AreEqual("test", response.Url);
        }

    }
}
