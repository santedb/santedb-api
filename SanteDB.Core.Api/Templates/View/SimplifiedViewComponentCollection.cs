using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Base class for a component collection
    /// </summary>
    [XmlType(nameof(SimplifiedViewComponentCollection), Namespace = "http://santedb.org/model/template/view")]
    public abstract class SimplifiedViewComponentCollection : SimplifiedViewComponent
    {

        /// <summary>
        /// Gets or sets the content
        /// </summary>
        [JsonProperty("content"),
            XmlElement("accordion", typeof(SimplifiedViewAccordion)),
            XmlElement("label", typeof(SimplifiedViewLabel)),
            XmlElement("input", typeof(SimplifiedViewInput)),
            XmlElement("select", typeof(SimplifiedViewSelect)),
            XmlElement("concept", typeof(SimplifiedViewConceptSelect)),
            XmlElement("flex", typeof(SimplifiedViewFlexLayout)),
            XmlElement("grid", typeof(SimplifiedViewGridLayout)),
            XmlElement("checkbox", typeof(SimplifiedCheckboxInput)),
            XmlElement("value", typeof(SimplifiedViewValue)),
            XmlElement("html", typeof(XElement), Namespace = NS_XHTML),
            XmlElement("span", typeof(SimplifiedViewTextBlock)),
            XmlElement("list", typeof(SimplifiedViewListBlock)),
            XmlElement("hint", typeof(SimplifiedViewHintComponent)),
            XmlElement("component", typeof(SimplifiedViewComponent))]
        public List<object> Content { get; set; }

        /// <summary>
        /// Render the content
        /// </summary>
        /// <param name="renderContext"></param>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext = renderContext.CreateChildContext(this);
            foreach(var itm in this.Content)
            {
                if (itm is SimplifiedViewComponent svc)
                {
                    svc.Render(renderContext);
                }
                else if(itm is XElement xe)
                {
                    xe.WriteTo(renderContext.HtmlWriter);
                }
            }
        }
    }
}