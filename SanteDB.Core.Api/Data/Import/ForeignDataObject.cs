using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a basic foreign data object 
    /// </summary>
    /// 
    [XmlType(nameof(ForeignDataObject), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public abstract class ForeignDataObject
    {

        /// <summary>
        /// Gets the name of the for
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Identifies the order of the object in the container
        /// </summary>
        [XmlElement("order"), JsonProperty("order")]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the path to this element in the source data
        /// </summary>
        [XmlElement("path"), JsonProperty("path")]
        public string Path { get; set; }
    }
}