using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test
{
    [TestFixture(Category ="Core API")]
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

            foreach(var input in inputs)
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
