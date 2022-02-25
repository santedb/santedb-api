using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A generic object map
    /// </summary>
    [XmlType(nameof(ForeignDataObjectMap), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public abstract class ForeignDataObjectMap
    {

        /// <summary>
        /// Gets the source path of the object
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public string SourcePath { get; set; }

        /// <summary>
        /// If the mapping fails should the import be aborted?
        /// </summary>
        [XmlElement("abortOnError"), JsonProperty("abortOnError")]
        public bool AbortOnError { get; set; }

    }
}