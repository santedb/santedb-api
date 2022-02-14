using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Defines how data can be imported from a data import source 
    /// </summary>
    [XmlType(nameof(ForeignDataMap), Namespace = "http://santedb.org/import")]
    [XmlRoot(nameof(ForeignDataMap), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataMap 
    {

        /// <summary>
        /// Gets or sets the UUID for the map
        /// </summary>
        [XmlElement("uuid"), JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the map
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The source object from the foreign data description this map applies to
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public ForeignDataElementGroup Source { get; set; }

        /// <summary>
        /// Gets or sets the mapping definition for the object
        /// </summary>
        [XmlArray("map"),
            XmlArrayItem("element", typeof(ForeignDataElementMap)), 
            XmlArrayItem("group", typeof(ForeignDataElementGroupMap)),
            JsonProperty("map")]
        public List<ForeignDataObjectMap> ObjectMap { get; set; }
    }
}
