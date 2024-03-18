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
 * User: fyfej
 * Date: 2023-6-21
 */
using NUnit.Framework;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace SanteDB.Core.Api.Test
{

    /// <summary>
    /// Simple shim for entity master
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ExcludeFromCodeCoverage]
    [XmlType(Namespace = "http://santedb.org/model")]
    public class EntityMaster<T> : Entity
       where T : IdentifiedData, new()
    {
    }

    /// <summary>
    /// Tests the serialization of a bundle when custom elements are in the bundle
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestFixture(Category = "Core API")]
    public class NonStandardEntityInBundleTest
    {

        /// <summary>
        /// Tests that a non standard bundle can be parsed
        /// </summary>
        [Test]
        public void TestCanParseNonStandardBundle()
        {
            ModelSerializationBinder.RegisterModelType(typeof(EntityMaster<Patient>));
            using (var stream = typeof(NonStandardEntityInBundleTest).Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.Resources.NonStandardBundle.xml"))
            {
                var xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(Bundle));
                var bundle = xsz.Deserialize(stream) as Bundle;
                Assert.AreEqual(4, bundle.Item.Count);
            }

        }
    }
}
