/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using NUnit.Framework;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Security.Signing;
using SanteDB.Core.TestFramework;

namespace SanteDB.Core.Api.Test
{
    /// <summary>
    /// Tests for the JWS 
    /// </summary>
    [TestFixture]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
