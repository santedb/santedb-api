using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// The type of checkbox
    /// </summary>

    [XmlType(nameof(SimplifiedCheckboxType), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedCheckboxType
    {
        /// <summary>
        /// Checkbox with on/off options
        /// </summary>
        [XmlEnum("check")]
        Check,
        /// <summary>
        /// Radio button - only one can be selected
        /// </summary>
        [XmlEnum("radio")]
        Radio
    }

    /// <summary>
    /// A checkbox or radio input group
    /// </summary>
    [XmlType(nameof(SimplifiedCheckboxInput), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedCheckboxInput : SimplifiedViewRowInputComponent
    {

        /// <summary>
        /// The label to append to the checkbox
        /// </summary>
        [XmlElement("label"), JsonProperty("label")]
        public string LabelText { get; set; }

        /// <summary>
        /// When the value is checked or selected, the value to place on the model
        /// </summary>
        [XmlAttribute("true-value"), JsonProperty("trueValue")]
        public string TrueValue { get; set; }

        /// <summary>
        /// When the value is not checked the value to place on the model
        /// </summary>
        [XmlAttribute("false-value"), JsonProperty("falseValue")]
        public string FalseValue { get; set; }

        /// <summary>
        /// The type of checkbox control
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public SimplifiedCheckboxType Type { get; set; }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            // Are we in a row?
            var isInRow = renderContext.Component is SimplifiedViewRow;

            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", base.GetCssClasses(renderContext));

            renderContext.HtmlWriter.WriteStartElement("label", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "checkbox-container");
            renderContext.HtmlWriter.WriteStartElement("input", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "checkbox-control");
            base.RenderInputCoreAttributes(renderContext.HtmlWriter);
            renderContext.HtmlWriter.WriteAttributeString("type", this.Type == SimplifiedCheckboxType.Check ? "checkbox" : "radio");

            if (!string.IsNullOrEmpty(this.TrueValue))
            {
                renderContext.HtmlWriter.WriteAttributeString("ng-true-value", $"'{this.TrueValue}'");
            }
            if (!string.IsNullOrEmpty(this.FalseValue))
            {
                renderContext.HtmlWriter.WriteAttributeString("ng-false-value", $"'{this.TrueValue}'");
            }

            renderContext.HtmlWriter.WriteEndElement(); // input
            renderContext.HtmlWriter.WriteStartElement("span", NS_XHTML);
            renderContext.HtmlWriter.WriteRaw(" ");
            renderContext.HtmlWriter.WriteEndElement(); // span

            if (!string.IsNullOrEmpty(this.LabelText))
            {
                renderContext.HtmlWriter.WriteString(this.LabelText);
            }

            renderContext.HtmlWriter.WriteEndElement(); // label

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