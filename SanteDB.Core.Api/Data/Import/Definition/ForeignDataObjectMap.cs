using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{

    /// <summary>
    /// Foreign data map definition that maps a complex object into a SanteDB
    /// </summary>
    [XmlType(nameof(ForeignDataObjectMap), Namespace = "http://santedb.org/import")]
    public class ForeignDataObjectMap : ForeignDataMapBase
    {

        /// <summary>
        /// The SanteDB resource which should be mapped
        /// </summary>
        [XmlElement("resource"), JsonProperty("resource")]
        public ResourceTypeReferenceConfiguration Resource { get; set; }

        /// <summary>
        /// Mappings for this object
        /// </summary>
        [XmlArray("maps"), XmlArrayItem("add"), JsonProperty("maps")]
        public List<ForeignDataElementMap> Maps { get; set; }

        /// <summary>
        /// An object transformer which is applied to the source data 
        /// </summary>
        [XmlElement("transform"), JsonProperty("transform")]
        public ForeignDataElementTransform Transform { get; set; }
    }
}