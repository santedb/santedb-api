using NUnit.Framework;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Security.Signing;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test
{
    /// <summary>
    /// Tests for the JWS 
    /// </summary>
    [TestFixture]
    public class JwsTest
    {

        /// <summary>
        /// Initializes the JWS test
        /// </summary>
        [OneTimeSetUp]
        public void Initialize()
        {
            TestApplicationContext.TestAssembly = typeof(JwsTest).Assembly;
            TestApplicationContext.Initialize(TestContext.CurrentContext.TestDirectory);
        }


        /// <summary>
        /// Test the creation of JWS content
        /// </summary>
        [Test]
        public void TestJwsCreateParse() 
        {
            object payload = new
            {
                foo = "bar",
                bar = "baz",
                buzz = "bizz"
            };

            var dataSigningService = ApplicationServiceContext.Current.GetService<IDataSigningService>();
            var jsonPayload = JsonWebSignature.Create(payload, dataSigningService)
                .WithCompression(Http.Description.HttpCompressionAlgorithm.Deflate)
                .WithKey("default")
                .WithType("foobar")
                .AsSigned();

            Assert.IsNotNull(jsonPayload.Signature);
            Assert.AreEqual("default", jsonPayload.Header.KeyId);
            Assert.AreEqual("DEF", jsonPayload.Header.Zip);
            Assert.AreEqual(3, jsonPayload.Token.Split('.').Length);

            Assert.AreEqual(JsonWebSignatureParseResult.Success, JsonWebSignature.TryParse(jsonPayload.Token, dataSigningService, out var signature));
        }
    }
}
