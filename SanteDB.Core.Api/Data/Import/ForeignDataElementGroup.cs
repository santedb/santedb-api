using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A class which describes a foreign data format
    /// </summary>
    [XmlType(nameof(ForeignDataElementGroup), Namespace = "http://santedb.org/import")]
    [XmlRoot(nameof(ForeignDataElementGroup), Namespace = "http://santedb.org/import")]
    [JsonObject]
    public class ForeignDataElementGroup : ForeignDataObject
    {

        /// <summary>
        /// Gets the data elements present in the foreign data
        /// </summary>
        [XmlArray("children"),  
            XmlArrayItem("element", Type = typeof(ForeignDataElement)),
            XmlArrayItem("group", Type = typeof(ForeignDataElementGroup)), 
            JsonProperty("children")]
        public List<ForeignDataObject> DataElements { get; set; }

    }
}