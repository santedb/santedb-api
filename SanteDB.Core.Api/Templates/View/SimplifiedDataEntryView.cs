using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Gets or sets the simplified data entry view
    /// </summary>
    [XmlType(nameof(SimplifiedDataEntryView), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedDataEntryView
    {
        private const string NS_XHTML = "http://www.w3.org/1999/xhtml";

        // TODO: Generate 

        /// <summary>
        /// Render the simplified view to <paramref name="xw"/>
        /// </summary>
        public void Render(XmlWriter xw)
        {
            xw.WriteStartElement("div", NS_XHTML);
            xw.WriteString("Simplified Views Are Not Yet Supported");
            xw.WriteEndElement(); // div
        }

        /// <summary>
        /// Parse the view content definition
        /// </summary>
        public static SimplifiedDataEntryView Parse(string str)
        {
            return new SimplifiedDataEntryView();
        }

    }
}