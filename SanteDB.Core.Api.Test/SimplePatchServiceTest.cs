/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using System;
using System.Diagnostics.CodeAnalysis;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Services.Impl;
using SanteDB.Core.Model.Patch;
using SanteDB.Core.Model.Roles;
using System.IO;
using System.Xml.Serialization;
using SanteDB.Core.Model.Constants;
using Newtonsoft.Json;
using SanteDB.Core.Model.Serialization;
using System.Linq;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Exceptions;
using NUnit.Framework;

namespace SanteDB.Core.Api.Test
{
    /// <summary>
    /// Represents a unit test which tests the patching ability 
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestFixture(Category = "Core API")]
    public class SimplePatchServiceTest
    {

        /// <summary>
        /// Guid sanity methods
        /// </summary>
        [Test]
        public void TestGuidSanity()
        {
            Guid g = Guid.Parse("880D2A08-8E94-402B-84B6-CB3BC0A576A9");
            byte[] b = g.ToByteArray();

        }

        /// <summary>
        /// Serialize patch
        /// </summary>
        private Patch SerializePatch(Patch patch)
        {
            String patchXml = String.Empty;
            Patch retVal = null;
            using (StringWriter sw = new StringWriter())
            {
                var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(Patch));
                xsz.Serialize(sw, patch);
                patchXml = sw.ToString();
            }
            using(StringReader sr = new StringReader(patchXml))
            {
                var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(Patch));
                retVal = xsz.Deserialize(sr) as Patch;
            }
            
            return retVal;
        }

        /// <summary>
        /// Tests that the diff method does not generate a patch for the same object
        /// </summary>
        [Test]
        public void DiffShouldNotGeneratePatchForIdentical()
        {
            SecurityUser a = new SecurityUser()
            {
                Key = Guid.Empty,
                UserName = "pepe",
                Password = "pepelepew",
                SecurityHash = Guid.NewGuid().ToString(),
                Email = "pepe@acme.com"
            };

            var patchService = new SimplePatchService();
            var patch = patchService.Diff(a, a);
            Assert.IsNotNull(patch);
            Assert.AreEqual(0, patch.Operation.Count);
        }

        /// <summary>
        /// Tests whether the diff engine detects a simple assignment of a property
        /// </summary>
        [Test]
        public void DiffShouldDetectSimplePropertyAssignment()
        {
            SecurityUser a = new SecurityUser()
            {
                Key = Guid.Empty,
                UserName = "pepe",
                Password = "pepelepew",
                SecurityHash = Guid.NewGuid().ToString(),
                Email = "pepe@acme.com"
            },
            b = new SecurityUser()
            {
                Key = Guid.Empty,
                UserName = "pepe",
                Password = "pepelepew",
                SecurityHash = Guid.NewGuid().ToString(),
                Email = "lepew@acme.com"
            };

            // Patch service 
            SimplePatchService patchService = new SimplePatchService();
            var patch = patchService.Diff(a, b);
            var patchString = patch.ToString();
            Assert.IsNotNull(patch);
            Assert.AreEqual(5, patch.Operation.Count);

            // Assert test
            Assert.AreEqual(PatchOperationType.Test, patch.Operation[0].OperationType);
            Assert.AreEqual(PatchOperationType.Test, patch.Operation[1].OperationType);
            Assert.AreEqual("email", patch.Operation[1].Path);
            Assert.AreEqual("pepe@acme.com", patch.Operation[1].Value);

            // Assert replace
            Assert.AreEqual(PatchOperationType.Replace, patch.Operation[2].OperationType);
            Assert.AreEqual("email", patch.Operation[2].Path);
            Assert.AreEqual("lepew@acme.com", patch.Operation[2].Value);
        }

        /// <summary>
        /// Tests that patch cascades to sub object
        /// </summary>
        [Test]
        public void DiffShouldGenerateForSubMembers()
        {

            Patient a = new Patient()
            {
                Key = Guid.Empty,
                VersionKey = Guid.NewGuid(),
                DateOfBirth = DateTime.MinValue,
                DateOfBirthPrecision = Model.DataTypes.DatePrecision.Full,
                Identifiers = new System.Collections.Generic.List<Model.DataTypes.EntityIdentifier>()
                {
                    new Model.DataTypes.EntityIdentifier(Guid.Empty, "1234") { Key = Guid.NewGuid() }
                }
            },
            b = new Patient()
            {
                Key = Guid.Empty,
                VersionKey = Guid.NewGuid(),
                DateOfBirth = DateTime.MaxValue,
                DateOfBirthPrecision = Model.DataTypes.DatePrecision.Day,
                Identifiers = new System.Collections.Generic.List<Model.DataTypes.EntityIdentifier>()
                {
                    new Model.DataTypes.EntityIdentifier(Guid.Empty, "1234") { Key = a.Identifiers[0].Key },
                    new Model.DataTypes.EntityIdentifier(Guid.NewGuid(), "3245") { Key = Guid.NewGuid() }
                },
                Names = new System.Collections.Generic.List<Model.Entities.EntityName>()
                {
                    new Model.Entities.EntityName(NameUseKeys.Legal, "Smith", "Joe") { NameUse = new Model.DataTypes.Concept() { Key = NameUseKeys.Legal, Mnemonic = "Legal" } }
                }
            };

            var patchService = new SimplePatchService();
            var patch = patchService.Diff(a, b);
            var patchString = patch.ToString();

            // Assert that there is a patch
            Assert.IsNotNull(patch);
            Assert.AreEqual(10, patch.Operation.Count);
            Assert.AreEqual(PatchOperationType.Add, patch.Operation[6].OperationType);
            Assert.AreEqual(b.Identifiers[1], patch.Operation[6].Value);
            Assert.AreEqual("identifier", patch.Operation[6].Path);

            this.SerializePatch(patch);            
        }

        /// <summary>
        /// Tests that the Diff method removes items from a collection where the key is the same but the value is different
        /// </summary>
        [Test]
        public void DiffShouldRemoveNameWithSameValues()
        {
            Patient a = new Patient()
            {
                Key = Guid.Empty,
                VersionKey = Guid.NewGuid(),
                DateOfBirth = DateTime.MaxValue,
                DateOfBirthPrecision = Model.DataTypes.DatePrecision.Full,
                Identifiers = new System.Collections.Generic.List<Model.DataTypes.EntityIdentifier>()
                {
                    new Model.DataTypes.EntityIdentifier(Guid.Empty, "1234") { Key = Guid.NewGuid() },
                    new Model.DataTypes.EntityIdentifier(Guid.NewGuid(), "3245") { Key = Guid.NewGuid() }
                },
                Names = new System.Collections.Generic.List<Model.Entities.EntityName>()
                {
                    new Model.Entities.EntityName(NameUseKeys.Legal, "Smith", "Joe") { Key = Guid.NewGuid() }
                },
                Tags = new System.Collections.Generic.List<Model.DataTypes.EntityTag>()
                {
                    new Model.DataTypes.EntityTag("KEY", "VALUE")
                }
            },
            b = new Patient()
            {
                Key = Guid.Empty,
                VersionKey = Guid.NewGuid(),
                DateOfBirth = DateTime.MaxValue,
                DateOfBirthPrecision = Model.DataTypes.DatePrecision.Full,
                Identifiers = new System.Collections.Generic.List<Model.DataTypes.EntityIdentifier>()
                {
                    new Model.DataTypes.EntityIdentifier(Guid.Empty, "1234") { Key = a.Identifiers[0].Key },
                    new Model.DataTypes.EntityIdentifier(Guid.NewGuid(), "3245") { Key = a.Identifiers[1].Key }
                },
                Names = new System.Collections.Generic.List<Model.Entities.EntityName>()
                {
                    new Model.Entities.EntityName(NameUseKeys.Legal, "Smith", "Joseph") { Key = a.Names[0].Key }
                },
                Addresses = new System.Collections.Generic.List<Model.Entities.EntityAddress>()
                {
                    new Model.Entities.EntityAddress(AddressUseKeys.HomeAddress, "123 Main Street West", "Hamilton", "ON", "CA", "L8K5N2")
                },
                Tags = new System.Collections.Generic.List<Model.DataTypes.EntityTag>()
                {
                    new Model.DataTypes.EntityTag("KEY", "VALUE2")
                }
            };

            var patchService = new SimplePatchService();
            var patch = patchService.Diff(a, b);
            var patchString = patch.ToString();
            Assert.IsNotNull(patch);
            Assert.AreEqual(11, patch.Operation.Count);

            // Assert there is a remove operation for a name
            Assert.IsTrue(patch.Operation.Any(o => o.OperationType == PatchOperationType.Remove && o.Value.ToString() == a.Names[0].Key.ToString()));
            Assert.IsTrue(patch.Operation.Any(o => o.OperationType == PatchOperationType.Remove && o.Path.Contains("tag.key")));
            this.SerializePatch(patch);

            var result = patchService.Patch(patch, a, true) as Patient;
            Assert.AreEqual(b.Addresses.Count, result.Addresses.Count);
            Assert.AreEqual(b.Names .Count, result.Names.Count);
            Assert.AreEqual(b.Identifiers.Count, result.Identifiers.Count);
            Assert.AreEqual(b.Tags.Count, result.Tags.Count);
            Assert.AreEqual(b.Tags.First().Value, result.Tags.First().Value);
            Assert.AreEqual("Joseph", result.Names.First().Component.Last().Value);

        }

        /// <summary>
        /// Tests that the diff function cascades to a nested single object
        /// </summary>
        [Test]
        public void DiffShouldCascadeToNestedSingleObjectRef()
        {
            Act a = new QuantityObservation()
            {
                Template = new Model.DataTypes.TemplateDefinition()
                {
                    Mnemonic = "TESTTEMPLATE",
                    Key = Guid.NewGuid(),
                    Description = "This is a test"
                }
            },
            b = new QuantityObservation()
            {
                Template = new Model.DataTypes.TemplateDefinition()
                {
                    Mnemonic = "OBSERVATION",
                    Key = Guid.NewGuid(),
                    Description = "This is a different template"
                }
            };

            var patchService = new SimplePatchService();
            var patch = patchService.Diff(a, b);
            var patchString = patch.ToString();

        }

        /// <summary>
        /// Detects that a patch fails an assertion
        /// </summary>
        [Test]
        public void PatchShouldFailAssertion()
        {
            Patient a = new Patient()
            {
                Key = Guid.Empty,
                VersionKey = Guid.NewGuid(),
                DateOfBirth = DateTime.MaxValue,
                DateOfBirthPrecision = Model.DataTypes.DatePrecision.Full,
                Identifiers = new System.Collections.Generic.List<Model.DataTypes.EntityIdentifier>()
                {
                    new Model.DataTypes.EntityIdentifier(Guid.Empty, "1234") { Key = Guid.NewGuid() },
                    new Model.DataTypes.EntityIdentifier(Guid.NewGuid(), "3245") { Key = Guid.NewGuid() }
                },
                Names = new System.Collections.Generic.List<Model.Entities.EntityName>()
                {
                    new Model.Entities.EntityName(NameUseKeys.Legal, "Smith", "Joe") { Key = Guid.NewGuid() }
                },
                Tags = new System.Collections.Generic.List<Model.DataTypes.EntityTag>()
                {
                    new Model.DataTypes.EntityTag("KEY", "VALUE")
                }
            },
            b = new Patient()
            {
                Key = Guid.Empty,
                VersionKey = Guid.NewGuid(),
                DateOfBirth = DateTime.MaxValue,
                DateOfBirthPrecision = Model.DataTypes.DatePrecision.Full,
                Identifiers = new System.Collections.Generic.List<Model.DataTypes.EntityIdentifier>()
                {
                    new Model.DataTypes.EntityIdentifier(Guid.Empty, "1234") { Key = a.Identifiers[0].Key },
                    new Model.DataTypes.EntityIdentifier(Guid.NewGuid(), "3245") { Key = a.Identifiers[1].Key }
                },
                Names = new System.Collections.Generic.List<Model.Entities.EntityName>()
                {
                    new Model.Entities.EntityName(NameUseKeys.Legal, "Smith", "Joseph") { Key = a.Names[0].Key }
                },
                Addresses = new System.Collections.Generic.List<Model.Entities.EntityAddress>()
                {
                    new Model.Entities.EntityAddress(AddressUseKeys.HomeAddress, "123 Main Street West", "Hamilton", "ON", "CA", "L8K5N2")
                },
                Tags = new System.Collections.Generic.List<Model.DataTypes.EntityTag>()
                {
                    new Model.DataTypes.EntityTag("KEY", "VALUE2")
                }
            };

            var patchService = new SimplePatchService();
            var patch = patchService.Diff(a, b);
            var patchString = patch.ToString();
            Assert.IsNotNull(patch);
            Assert.AreEqual(11, patch.Operation.Count);
            
            // Debug info
            this.SerializePatch(patch);

            // 1. Patch should be fine. data now = b
            var data = patchService.Patch(patch, a);

            // 2. Patch should fail, a is different
            a = a.Clone() as Patient;
            a.VersionKey = Guid.NewGuid();
            try
            {
                data = patchService.Patch(patch, a);
                Assert.Fail();
            }
            catch(PatchAssertionException e)
            {
                
            }
        }


        /// <summary>
        /// Test that the patch updates the target object
        /// </summary>
        [Test]
        public void PatchShouldUpdateTargetObject()
        {
            Guid oguid = Guid.NewGuid(),
                nguid = Guid.NewGuid();

            Patient a = new Patient()
            {
                Key = Guid.Empty,
                VersionKey = Guid.NewGuid(),
                DateOfBirth = DateTime.MaxValue,
                DateOfBirthPrecision = Model.DataTypes.DatePrecision.Full,
                Identifiers = new System.Collections.Generic.List<Model.DataTypes.EntityIdentifier>()
                {
                    new Model.DataTypes.EntityIdentifier(Guid.Empty, "1234") { Key = Guid.NewGuid() },
                    new Model.DataTypes.EntityIdentifier(Guid.NewGuid(), "3245") { Key = Guid.NewGuid() }
                },
                Names = new System.Collections.Generic.List<Model.Entities.EntityName>()
                {
                    new Model.Entities.EntityName(NameUseKeys.Legal, "Smith", "Joe") { Key = Guid.NewGuid() },
                    new Model.Entities.EntityName(NameUseKeys.OfficialRecord, "Smith", "Joseph") { Key = Guid.NewGuid() }
                },
                Tags = new System.Collections.Generic.List<Model.DataTypes.EntityTag>()
                {
                    new Model.DataTypes.EntityTag("KEY", "VALUE")
                },
                Relationships = new System.Collections.Generic.List<Model.Entities.EntityRelationship>() {
                    new Model.Entities.EntityRelationship()
                    {
                        Key = Guid.NewGuid(),
                        RelationshipTypeKey = EntityRelationshipTypeKeys.DedicatedServiceDeliveryLocation,
                        TargetEntityKey = oguid
                    }
                }
            },
            b = new Patient()
            {
                Key = Guid.Empty,
                VersionKey = Guid.NewGuid(),
                DateOfBirth = DateTime.MaxValue,
                DateOfBirthPrecision = Model.DataTypes.DatePrecision.Day,
                Identifiers = new System.Collections.Generic.List<Model.DataTypes.EntityIdentifier>()
                {
                    new Model.DataTypes.EntityIdentifier(Guid.Empty, "1234") { Key = a.Identifiers[0].Key }
                },
                Names = new System.Collections.Generic.List<Model.Entities.EntityName>()
                {
                    new Model.Entities.EntityName(NameUseKeys.Legal, "Smith", "Joseph") { Key = a.Names[0].Key }
                },
                Addresses = new System.Collections.Generic.List<Model.Entities.EntityAddress>()
                {
                    new Model.Entities.EntityAddress(AddressUseKeys.HomeAddress, "123 Main Street West", "Hamilton", "ON", "CA", "L8K5N2")
                },
                Tags = new System.Collections.Generic.List<Model.DataTypes.EntityTag>()
                {
                    new Model.DataTypes.EntityTag("KEY", "VALUE2")
                },
                Relationships = new System.Collections.Generic.List<Model.Entities.EntityRelationship>() {
                    new Model.Entities.EntityRelationship()
                    {
                        Key = Guid.NewGuid(),
                        RelationshipTypeKey = EntityRelationshipTypeKeys.DedicatedServiceDeliveryLocation,
                        TargetEntityKey = nguid
                    }
                }
            };

            var patchService = new SimplePatchService();
            var patch = patchService.Diff(a, b);
            var patchString = patch.ToString();
            Assert.IsNotNull(patch);
            Assert.AreEqual(15, patch.Operation.Count);

            // Debug info
            patch = this.SerializePatch(patch);

            // 1. Patch should be fine. data now = b
            var data = patchService.Patch(patch, a) as Patient;

            // Should update result
            Assert.AreEqual(1, data.Names.Count);
            Assert.AreEqual(1, data.Identifiers.Count);
            Assert.AreEqual(1, data.Addresses.Count);
            Assert.AreEqual(1, data.Tags.Count);
            Assert.AreEqual("VALUE2", data.Tags[0].Value);
            Assert.AreEqual(b.VersionKey, data.VersionKey);

            // Should not update source
            Assert.AreEqual(2, a.Names.Count);
            Assert.AreEqual(2, a.Identifiers.Count);
            Assert.IsEmpty(a.Addresses);
            Assert.AreEqual(1, a.Tags.Count);
            Assert.AreEqual("VALUE", a.Tags[0].Value);
            Assert.AreEqual(a.VersionKey, patch.Operation[1].Value);
            Assert.AreEqual(nguid, data.Relationships[0].TargetEntityKey);
        }
    }
}
