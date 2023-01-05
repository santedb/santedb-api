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
