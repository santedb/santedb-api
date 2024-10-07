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
 */
using NUnit.Framework;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.DataTypes.CheckDigitAlgorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Debug.WriteLine(id + cd);
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
