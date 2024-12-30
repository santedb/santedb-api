using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Grid Layout - Items are placed into a grid
    /// </summary>
    [XmlType(nameof(SimplifiedViewGridLayout), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewGridLayout : SimplifiedViewContentComponent, ISimplifiedViewLayout
    {

        /// <summary>
        /// Rows which are to be used in the layout
        /// </summary>
        [XmlElement("row"), JsonProperty("content")]
        public List<SimplifiedViewRow> Content { get; set; }

        /// <summary>
        /// Get CSS classes
        /// </summary>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            return $"container-fluid {base.GetCssClasses(renderContext)}".Trim();
        }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
            renderContext = renderContext.CreateChildContext(this);
            foreach (var itm in this.Content)
            {
                itm.Render(renderContext);
            }
            renderContext.HtmlWriter.WriteEndElement();
        }

    }
}