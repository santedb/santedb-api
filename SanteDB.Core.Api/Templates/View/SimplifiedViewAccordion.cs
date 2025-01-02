using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Represents a simplified accordion which is a collection of accordion panels where only one panel can be expanded at a time
    /// </summary>
    [XmlType(nameof(SimplifiedViewAccordion), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewAccordion : SimplifiedViewContentComponent
    {

        /// <summary>
        /// Represents a single accordion pane
        /// </summary>
        [XmlElement("panel"), JsonProperty("content")]
        public List<SimplifiedViewAccordionPane> Content { get; set; }


        /// <inheritdoc/>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            return $"accordion {base.GetCssClasses(renderContext)}".Trim();
        }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));

            this.Name = this.Name ?? $"acc{Guid.NewGuid().ToString().Substring(0, 6)}";
            renderContext.HtmlWriter.WriteAttributeString("id", this.Name);

            var childContext = renderContext.CreateChildContext(this);
            foreach(var itm in this.Content)
            {
                itm.Render(childContext);
            }

            renderContext.HtmlWriter.WriteEndElement();
        }
    }
}