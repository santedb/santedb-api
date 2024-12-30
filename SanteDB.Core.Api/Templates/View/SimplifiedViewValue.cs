using Newtonsoft.Json;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Instructs the EMR how to render the content
    /// </summary>
    [XmlType(nameof(SimplifiedRenderType), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedRenderType
    {
        /// <summary>
        /// Render the bound property as a concept display name
        /// </summary>
        [XmlEnum("concept")]
        Concept,
        /// <summary>
        /// Render the bound property as an entity name
        /// </summary>
        [XmlEnum("name")]
        Name,
        /// <summary>
        /// Render the bound property as an address
        /// </summary>
        [XmlEnum("address")]
        Address,
        /// <summary>
        /// Render the bound property as a date to the month
        /// </summary>
        [XmlEnum("date-month")]
        DateMonth,
        /// <summary>
        /// Render the bound property as a date to the day
        /// </summary>
        [XmlEnum("date")]
        DateDay,
        /// <summary>
        /// Render the bound property as a date and time
        /// </summary>
        [XmlEnum("date-time")]
        DateTime,
        /// <summary>
        /// Render the bound property as a telecom (e-mail address or telephone number)
        /// </summary>
        [XmlEnum("telecom")]
        Telecom,
        /// <summary>
        /// Render the bound property as an identifier
        /// </summary>
        [XmlEnum("identifier")]
        Identifier

    }

    /// <summary>
    /// Simplified view binding value
    /// </summary>
    [XmlType(nameof(SimplifiedViewValue), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewValue : SimplifiedViewContentComponent
    {

        /// <summary>
        /// The HDSI binding path for the data to be displayed
        /// </summary>
        [XmlAttribute("binding"), JsonProperty("binding")]
        public string BindingPath { get; set; }

        /// <summary>
        /// How the bound property should be rendered
        /// </summary>
        [XmlAttribute("render"), JsonProperty("render")]
        public SimplifiedRenderType Filter { get; set; }

        /// <summary>
        /// True if the rendering is specified 
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool FilterSpecified { get; set; }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {

            var rowHost = renderContext.Component is SimplifiedViewRow;

            if (!rowHost)
            {
                renderContext.HtmlWriter.WriteStartElement("span", NS_XHTML);
            }
            else
            {
                renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            }
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));

            if (this.FilterSpecified)
            {
                renderContext.HtmlWriter.WriteString($"{{ act.{this.BindingPath} }} | { this.MapFilter()}");

            }
            else
            {
                renderContext.HtmlWriter.WriteString($"{{ act.{this.BindingPath} }}");
            }
            renderContext.HtmlWriter.WriteEndElement();
        }


        /// <summary>
        /// Map the rendering filter
        /// </summary>
        private string MapFilter()
        {
            switch(this.Filter)
            {
                case SimplifiedRenderType.Address:
                    return "address";
                case SimplifiedRenderType.Concept:
                    return "concept";
                case SimplifiedRenderType.DateMonth:
                    return "humanDate: 'm'";
                case SimplifiedRenderType.DateDay:
                    return "humanDate: 'D'";
                case SimplifiedRenderType.DateTime:
                    return "humanDate: 'M'";
                case SimplifiedRenderType.Identifier:
                    return "identifier";
                case SimplifiedRenderType.Name:
                    return "name";
                case SimplifiedRenderType.Telecom:
                    return "telecom";
                default:
                    return "";
            }
        }
    }
}