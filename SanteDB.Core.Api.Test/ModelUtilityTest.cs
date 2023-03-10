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
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Api.Test
{
    [TestFixture]
    public class ModelUtilityTest
    {

        /// <summary>
        /// Test the deep copy method
        /// </summary>
        [Test]
        public void TestDeepCopy()
        {
            var a = new Entity()
            {
                Names = new List<EntityName>()
                {
                    new EntityName(NameUseKeys.Legal, "TEST")
                }
            };
            var b = a.Clone();
            Assert.IsInstanceOf<Entity>(b);
            Assert.AreEqual(a.Names.Count, (b as Entity).Names.Count);
            a.Names.Add(new EntityName(NameUseKeys.Alphabetic, "ANOTHER"));
            Assert.AreNotEqual(a.Names.Count, (b as Entity).Names.Count);
        }

        /// <summary>
        /// Test the semantic equality 
        /// </summary>
        [Test]
        public void TestSemanticEquals()
        {
            var a = new Entity()
            {
                Key = Guid.NewGuid(),
                Names = new List<EntityName>()
                {
                    new EntityName(NameUseKeys.Legal, "TEST")
                }
            };
            var b = new Entity()
            {
                Key = Guid.NewGuid(),
                Names = new List<EntityName>()
                {
                    new EntityName(NameUseKeys.Legal, "TEST")
                }
            };

            Assert.IsTrue(a.SemanticEquals(b));

            // Change A in a logical  way
            b.Names[0].Component[0].Value = "TESTS";
            Assert.IsFalse(a.SemanticEquals(b));


        }
    }
}
