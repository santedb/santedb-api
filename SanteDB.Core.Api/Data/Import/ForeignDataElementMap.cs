using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a single transform from a foreign data element to a CDR model element
    /// </summary>
    [XmlType(nameof(ForeignDataElementMap), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataElementMap : ForeignDataObjectMap
    {

        /// <summary>
        /// Gets or sets the target property where the source data should be stored
        /// </summary>
        [XmlElement("property"), JsonProperty("property")]
        public string TargetProperty { get; set; }

        /// <summary>
        /// Gets or sets the transforms for the data
        /// </summary>
        [XmlArray("transforms"), XmlArrayItem("add"), JsonProperty("transforms")]
        public List<ForeignDataElementMapTransform> Transforms { get; set; }

    }
}