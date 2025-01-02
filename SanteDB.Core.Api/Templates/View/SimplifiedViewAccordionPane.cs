using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A single accordion pane which is rendered in the accordion
    /// </summary>
    [XmlType(nameof(SimplifiedViewAccordionPane), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewAccordionPane : SimplifiedViewComponentCollection
    {
        /// <summary>
        /// The title of the accordion pane
        /// </summary>
        [XmlElement("title"), JsonProperty("title")]
        public string Title { get; set; }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            var accordion = renderContext.Component as SimplifiedViewAccordion;
            var idx = accordion.Content.IndexOf(this);

            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "card");
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "card-header");
            renderContext.HtmlWriter.WriteStartElement("button", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("data-target", $"#{accordion.Name}_content{idx}");
            renderContext.HtmlWriter.WriteAttributeString("data-toggle", "collapse");
            renderContext.HtmlWriter.WriteAttributeString("class", "btn btn-link card-title collapse-indicator");
            renderContext.HtmlWriter.WriteAttributeString("aria-expanded", $"{idx == 0}");
            renderContext.HtmlWriter.WriteString(this.Title ?? "Expand Option");
            renderContext.HtmlWriter.WriteStartElement("i", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "fa fa-fw fa-chevron-right");
            renderContext.HtmlWriter.WriteEndElement(); // i
            renderContext.HtmlWriter.WriteEndElement(); // button
            renderContext.HtmlWriter.WriteEndElement(); // div .card-header

            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", idx == 0 ? "collapse show" : "collapse");
            renderContext.HtmlWriter.WriteAttributeString("data-parent", $"#{accordion.Name}");
            renderContext.HtmlWriter.WriteAttributeString("id", $"{accordion.Name}_content{idx}");
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "card-body");
            base.Render(renderContext);
            renderContext.HtmlWriter.WriteEndElement(); // div.card-body
            renderContext.HtmlWriter.WriteEndElement(); // div.collapse
            renderContext.HtmlWriter.WriteEndElement(); // div.card
        }
    }
}