using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A selection of pre-determined options which allows the user to select one
    /// </summary>
    [XmlType(nameof(SimplifiedViewSelect), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewSelect : SimplifiedViewRowInputComponent
    {

        /// <summary>
        /// The options in the selection list
        /// </summary>
        [XmlElement("option"), JsonProperty("option")]
        public List<SimplifiedViewSelectOption> Options { get; set; }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", base.GetCssClasses(renderContext));

            // Add an interpretation code 
            renderContext.HtmlWriter.WriteStartElement("select", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "form-control");
            base.RenderInputCoreAttributes(renderContext.HtmlWriter);

            // Options
            foreach (var opt in this.Options)
            {
                opt.Render(renderContext.HtmlWriter);
            }
            renderContext.HtmlWriter.WriteEndElement();

            if (this.Required)
            {
                base.RenderValidationError(renderContext.HtmlWriter, "required");
            }
            if (this.CdssCallback)
            {
                this.RenderValidationError(renderContext.HtmlWriter, "cdss");
            }

            renderContext.HtmlWriter.WriteEndElement(); // div
        }
    }
}