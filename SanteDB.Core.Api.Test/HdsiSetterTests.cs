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
