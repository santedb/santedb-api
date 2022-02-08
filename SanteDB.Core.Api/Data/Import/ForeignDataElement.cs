using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a single data element from a foreign data element
    /// </summary>
    [XmlType(nameof(ForeignDataElement), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataElement : ForeignDataObject
    {

        /// <summary>
        /// Gets the type of the data element
        /// </summary>
        [XmlElement("type"), JsonProperty("type")]
        public TypeReferenceConfiguration Type { get; set; }

        /// <summary>
        /// Gets or sets an example of the data in this column
        /// </summary>
        [XmlElement("sample"),
            JsonProperty("sample")]
        public string SampleValue { get; set; }
    }
}