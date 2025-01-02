using Newtonsoft.Json;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A single selectio option
    /// </summary>
    [XmlType(nameof(SimplifiedViewSelectOption), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewSelectOption
    {

        /// <summary>
        /// The value that should be placed into the model binding when this value is selected
        /// </summary>
        [XmlAttribute("value"), JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// The display of this option (as it appears to the user)
        /// </summary>
        [XmlText(), JsonProperty("text")]
        public string Display { get; set; }

        /// <summary>
        /// Render out
        /// </summary>
        internal void Render(XmlWriter writer)
        {
            writer.WriteStartElement("option", "http://www.w3.org/1999/xhtml");
            writer.WriteAttributeString("value", this.Value);
            writer.WriteString(this.Display);
            writer.WriteEndElement();// option
        }
    }
}