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
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test
{
    [TestFixture]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class HdsiSetterTests
    {

        /// <summary>
        /// Test that the model utilities can get values and create trees based on HDSI expressions
        /// </summary>
        [Test]
        public void TestCanGetAndCreate()
        {

            var patientUnderTest = new Patient()
            {
                DateOfBirth = DateTime.MaxValue,
                Names = new List<Model.Entities.EntityName>()
                {
                    new Model.Entities.EntityName(NameUseKeys.Anonymous, "BOB")
                }
            };

            var hdsiPath = "dateOfBirth";
            var getValue = patientUnderTest.GetOrSetValueAtPath(hdsiPath);
            Assert.AreEqual(DateTime.MaxValue, getValue);

            hdsiPath = $"name[{NameUseKeys.Anonymous}].component.value";
            getValue = patientUnderTest.GetOrSetValueAtPath(hdsiPath);
            Assert.AreEqual("BOB", getValue);

            // Now - the class should set the sub-paths
            hdsiPath = $"name[OfficialRecord].component.value";
            getValue = patientUnderTest.GetOrSetValueAtPath(hdsiPath);
            Assert.IsNull(getValue); // The value is null
            Assert.AreEqual(2, patientUnderTest.Names.Count);  // The new name type was added
            Assert.AreEqual(1, patientUnderTest.Names.Last().Component.Count);
            getValue = patientUnderTest.GetOrSetValueAtPath(hdsiPath, "RICHIE");
            Assert.AreEqual(2, patientUnderTest.Names.Count);  // The new name type was added
            Assert.AreEqual(1, patientUnderTest.Names.Last().Component.Count);
            Assert.AreEqual("RICHIE", patientUnderTest.Names.Last().Component.Last().Value);

            hdsiPath = "relationship[Mother].target@Person.name[MaidenName].component[Family].value";
            getValue = patientUnderTest.GetOrSetValueAtPath(hdsiPath, "EVANS");
            Assert.AreEqual(1, patientUnderTest.Relationships.Count);  // The new name type was added
            Assert.IsInstanceOf<Person>(patientUnderTest.Relationships.Last().TargetEntity);
            Assert.AreEqual("EVANS", patientUnderTest.Relationships.First().TargetEntity.Names.First().Component.First().Value);  // The new name type was added

            hdsiPath = "relationship[Mother].target@Person.dateOfBirth";
            getValue = patientUnderTest.GetOrSetValueAtPath(hdsiPath, DateTime.Now.Date);
            Assert.AreEqual(DateTime.Now.Date, (patientUnderTest.Relationships.First().TargetEntity as Person).DateOfBirth.Value.Date);
        }
    }
}
