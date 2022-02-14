using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A transform 
    /// </summary>
    [XmlType(nameof(ForeignDataElementMapTransform), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataElementMapTransform
    {

        /// <summary>
        /// Gets the type of transform to use
        /// </summary>
        [XmlElement("transformer"), JsonProperty("transformer")]
        public string Transformer { get; set; }

        /// <summary>
        /// Gets or sets the list of arguments
        /// </summary>
        [XmlArray("args"),
            XmlArrayItem("int", typeof(Int32)),
            XmlArrayItem("string", typeof(String)),
            XmlArrayItem("bool", typeof(Boolean)),
            XmlArrayItem("dateTime", typeof(DateTime)),
            JsonProperty("args")]
        public List<Object> Arguments { get; set; }

    }
}