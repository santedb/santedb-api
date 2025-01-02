using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Simplified row render
    /// </summary>
    [XmlType(nameof(SimplifiedViewRow), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewRow : SimplifiedViewComponentCollection
    {



        /// <inheritdoc/>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            return $"form-group row {base.GetCssClasses(renderContext)}".Trim();
        }

        /// <inheritdoc />
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
            base.Render(renderContext);
            renderContext.HtmlWriter.WriteEndElement(); // DIV
        }
    }
}