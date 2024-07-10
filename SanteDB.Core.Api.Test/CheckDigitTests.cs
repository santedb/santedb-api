using NUnit.Framework;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.DataTypes.CheckDigitAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test
{
    [TestFixture]
    public class CheckDigitTests
    {

        [Test]
        public void TestMod97Classic()
        {
            var cda = new Mod97CheckDigitAlgorithm();
            var idv = new InlineMod97Validator();

            for (var iter = 0; iter < 100; iter++)
            {
                var id = Guid.NewGuid().ToByteArray().Where(o=>o > 0).Select(o=>(long)o).Aggregate((a, b) => ((a * b) + 1) % 9999999999).ToString().PadLeft(10, '0');
                var cd = cda.GenerateCheckDigit(id);
                Assert.IsTrue(cda.ValidateCheckDigit(id, cd));
                Assert.IsTrue(idv.IsValid(new EntityIdentifier(Guid.Empty, id + cd)));
                
            }
        }


        [Test]
        public void TestIso7064Mod97Classic()
        {
            var cda = new Iso7064Mod97CheckDigitAlgorithm();
            var idv = new InlineIso7064Mod97Validator();

            for (var iter = 0; iter < 100; iter++)
            {
                var id = Guid.NewGuid().ToByteArray().Where(o => o > 0).Select(o => (long)o).Aggregate((a, b) => ((a * b) + 1) % 9999999999).ToString().PadLeft(10, '0');
                var cd = cda.GenerateCheckDigit(id);
                Assert.IsTrue(cda.ValidateCheckDigit(id, cd));
                Assert.IsTrue(idv.IsValid(new EntityIdentifier(Guid.Empty, id + cd)));
                
            }
        }



    }
}
