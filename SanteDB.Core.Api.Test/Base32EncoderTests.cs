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
 * Date: 2023-6-21
 */
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Api.Test
{
    [TestFixture(Category = "Core API")]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Base32EncoderTests
    {
        [Test]
        public void TestBase32Encode_SimpleString()
        {
            var input = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog.");

            var actual = input.Base32Encode();

            Assert.AreEqual("KRUGKIDROVUWG2ZAMJZG653OEBTG66BANJ2W24DTEBXXMZLSEB2GQZJANRQXU6JAMRXWOLQ=", actual);
        }

        [Test]
        public void TestBase32Encode_LessThan1Block()
        {
            var inputs = new Dictionary<string, string>()
            {
                { "A", "IE======" },
                { "AB", "IFBA====" },
                {"ABC", "IFBEG===" },
                {"ABCD", "IFBEGRA=" }
            };

            foreach (var input in inputs)
            {
                var actual = Encoding.UTF8.GetBytes(input.Key).Base32Encode();

                Assert.AreEqual(input.Value, actual);
            }
        }

        [Test]
        public void TestBase32Encode_EmptyString()
        {
            var input = new byte[0];

            var actual = input.Base32Encode();

            Assert.AreEqual("", actual);
        }
    }
}
