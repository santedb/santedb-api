using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A simple static text block
    /// </summary>
    [XmlType(nameof(SimplifiedViewTextBlock), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewTextBlock : SimplifiedViewContentComponent
    {

        /// <summary>
        /// Gets or sets the text 
        /// </summary>
        [XmlText, JsonProperty("text")]
        public string Text { get; set; }

        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            var isInRow = renderContext.Component is SimplifiedViewRow;

            if (isInRow)
            {
                renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            }
            else
            {
                renderContext.HtmlWriter.WriteStartElement("span", NS_XHTML);
            }

            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
            renderContext.HtmlWriter.WriteString(this.Text);
            renderContext.HtmlWriter.WriteEndElement(); // span or div
        }
    }
}