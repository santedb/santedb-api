using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Modifier for a fixed value
    /// </summary>
    [XmlType(nameof(ForeignDataFixedValueModifier), Namespace = "http://santedb.org/import")]
    public class ForeignDataFixedValueModifier : ForeignDataValueModifier
    {

        /// <summary>
        /// Fixed value
        /// </summary>
        [XmlText(), JsonProperty("value")]
        public string FixedValue { get; set; }
    }
}