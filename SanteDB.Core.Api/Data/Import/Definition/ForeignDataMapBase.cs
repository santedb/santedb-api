using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// The base data map for external data
    /// </summary>
    [XmlType(nameof(ForeignDataMapBase), Namespace = "http://santedb.org/import")]
    public abstract class ForeignDataMapBase
    {

        /// <summary>
        /// Gets or sets the source from the foreign data source
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the documentation of the object
        /// </summary>
        [XmlElement("comment"), JsonProperty("comment")]
        public string Documentation { get; set; }

    }
}