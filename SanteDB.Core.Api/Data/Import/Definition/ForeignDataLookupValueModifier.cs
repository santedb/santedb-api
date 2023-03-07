using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Modifier for a fixed value
    /// </summary>
    [XmlType(nameof(ForeignDataLookupValueModifier), Namespace = "http://santedb.org/import")]
    public class ForeignDataLookupValueModifier : ForeignDataValueModifier
    {

        /// <summary>
        /// Fixed value
        /// </summary>
        [XmlText(), JsonProperty("value")]
        public string SourceColumn { get; set; }
    }
}