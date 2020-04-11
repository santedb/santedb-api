using Microsoft.VisualStudio.TestTools.UnitTesting;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteDB.Core.PCL.Test
{

    /// <summary>
    /// Simple shim for entity master
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [XmlType(Namespace = "http://santedb.org/model")]
    public class EntityMaster<T> : Entity
       where T : IdentifiedData, new()
    {
    }

    /// <summary>
    /// Tests the serialization of a bundle when custom elements are in the bundle
    /// </summary>
    [TestClass]
    public class NonStandardEntityInBundleTest
    {

        /// <summary>
        /// Tests that a non standard bundle can be parsed
        /// </summary>
        [TestMethod]
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
