using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A single item in the list
    /// </summary>
    [XmlType(nameof(SimplifiedViewListItem), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewListItem : SimplifiedViewComponent
    {

        /// <summary>
        /// The term which is being defined in the list
        /// </summary>
        [XmlElement("term"), JsonProperty("term")]
        public String Term { get; set; }

        /// <summary>
        /// The text of the list item
        /// </summary>
        [XmlText, JsonProperty("text")]
        public String Text { get; set; }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            var isinDl = (renderContext.Component as SimplifiedViewListBlock).Type == SimplifiedListType.Definition;

            if(isinDl)
            {
                renderContext.HtmlWriter.WriteStartElement("dt", NS_XHTML);
                renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
                renderContext.HtmlWriter.WriteString(this.Term);
                renderContext.HtmlWriter.WriteEndElement();
                renderContext.HtmlWriter.WriteStartElement("dd", NS_XHTML);
                renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
                renderContext.HtmlWriter.WriteString(this.Text);
                renderContext.HtmlWriter.WriteEndElement();
            }
            else
            {
                renderContext.HtmlWriter.WriteStartElement("li", NS_XHTML);
                if(!String.IsNullOrEmpty(this.Term))
                {
                    renderContext.HtmlWriter.WriteElementString("strong", NS_XHTML, this.Term);
                }
                renderContext.HtmlWriter.WriteString(this.Text);
                renderContext.HtmlWriter.WriteEndElement();
            }
        }
    }
}