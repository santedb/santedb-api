/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2024-12-22
 */
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