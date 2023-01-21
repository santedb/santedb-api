using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// When condition
    /// </summary>
    [XmlType(nameof(ForeignDataMapOnlyWhenCondition), Namespace = "http://santedb.org/import")]
    public class ForeignDataMapOnlyWhenCondition : ForeignDataMapBase
    {
        /// <summary>
        /// Gets or sets the value that the <see cref="ForeignDataMapBase.Source"/>
        /// must equal 
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public List<string> Value { get; set; }

    }
}