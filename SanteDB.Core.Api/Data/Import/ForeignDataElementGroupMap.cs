using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A class which describes how the components of a <see cref="ForeignDataElementGroup"/> can be imported
    /// </summary>
    [XmlType(nameof(ForeignDataElementGroupMap), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataElementGroupMap : ForeignDataObjectMap
    {

        /// <summary>
        /// Gets the target type 
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public TypeReferenceConfiguration TargetType { get; set; }

        /// <summary>
        /// Gets the child or element maps
        /// </summary>
        [XmlArray("map"), XmlArrayItem("element", typeof(ForeignDataElementMap)), XmlArrayItem("group", typeof(ForeignDataElementGroupMap)),
            JsonProperty("map")]
        public List<ForeignDataObjectMap> ObjectMap { get; set; }

    }
}