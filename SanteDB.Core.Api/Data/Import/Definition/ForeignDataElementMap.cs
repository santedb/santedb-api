using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Represents a single foreign data element to be mapped
    /// </summary>
    [XmlType(nameof(ForeignDataElementMap), Namespace = "http://santedb.org/import")]
    public class ForeignDataElementMap : ForeignDataMapBase
    {

        /// <summary>
        /// Gets or sets the target HDSI path
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public string TargetHdsiPath { get; set; }

        /// <summary>
        /// Gets or sets the transformations on the source column
        /// </summary>
        [XmlArray("transforms"), XmlArrayItem("add"), JsonProperty("transforms")]
        public List<ForeignDataElementTransform> Transforms { get; set; }
    }
}