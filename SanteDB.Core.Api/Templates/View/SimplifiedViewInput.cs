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
using System;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Simplified input type
    /// </summary>
    [XmlType(nameof(SimplifiedInputType), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedInputType
    {
        /// <summary>
        /// Simple text input
        /// </summary>
        [XmlEnum("text")]
        Text,
        /// <summary>
        /// Simple number input
        /// </summary>
        [XmlEnum("number")]
        Number,
        /// <summary>
        /// A date (year, month, day)
        /// </summary>
        [XmlEnum("date")]
        Date,
        /// <summary>
        /// A date and time (year, month, day, hour and minute)
        /// </summary>
        [XmlEnum("dateTime")]
        DateTime,
        /// <summary>
        /// A time input (hour and minute)
        /// </summary>
        [XmlEnum("time")]
        Time,
        /// <summary>
        /// A range input (slider)
        /// </summary>
        [XmlEnum("range")]
        Range
    }

    /// <summary>
    /// A simple input which allows the capture of simple data elements
    /// </summary>
    [XmlType(nameof(SimplifiedViewInput), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewInput : SimplifiedViewRowInputComponent
    {

        /// <summary>
        /// The type of information to be captured
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public SimplifiedInputType Type { get; set; }

        /// <summary>
        /// The minimum value of the input
        /// </summary>
        [XmlAttribute("min"), JsonProperty("min")]
        public string Min { get; set; }

        /// <summary>
        /// The maximum value of the input
        /// </summary>
        [XmlAttribute("max"), JsonProperty("max")]
        public string Max { get; set; }


        /// <summary>
        /// If the input must follow a particular pattern, this is the pattern
        /// </summary>
        [XmlAttribute("pattern"), JsonProperty("pattern")]
        public string Pattern { get; set; }

        /// <inheritdoc/>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            return "form-control";
        }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", base.GetCssClasses(renderContext));

            // Add an interpretation code 
            renderContext.HtmlWriter.WriteStartElement("input", NS_XHTML);

            switch (this.Type)
            {
                case SimplifiedInputType.Date:
                    renderContext.HtmlWriter.WriteAttributeString("type", "date");
                    break;
                case SimplifiedInputType.DateTime:
                    renderContext.HtmlWriter.WriteAttributeString("type", "datetime");
                    break;
                case SimplifiedInputType.Time:
                    renderContext.HtmlWriter.WriteAttributeString("type", "time");
                    break;
                case SimplifiedInputType.Number:
                    renderContext.HtmlWriter.WriteAttributeString("type", "number");
                    break;
                case SimplifiedInputType.Range:
                    renderContext.HtmlWriter.WriteAttributeString("type", "range");
                    break;
                case SimplifiedInputType.Text:
                default:
                    renderContext.HtmlWriter.WriteAttributeString("type", "text");
                    break;

            }

            if (!string.IsNullOrEmpty(this.Min))
            {
                renderContext.HtmlWriter.WriteAttributeString("min", this.Min);
            }
            if (!string.IsNullOrEmpty(this.Max))
            {
                renderContext.HtmlWriter.WriteAttributeString("max", this.Min);
            }
            if (this.Required)
            {
                renderContext.HtmlWriter.WriteAttributeString("required", "required");
            }
            if (this.CdssCallback)
            {
                renderContext.HtmlWriter.WriteAttributeString("cdss-interactive", "act");
            }
            if (!string.IsNullOrEmpty(this.Pattern))
            {
                renderContext.HtmlWriter.WriteAttributeString("pattern", this.Pattern);
            }

            renderContext.HtmlWriter.WriteAttributeString("ng-model", $"act.{this.Binding}");
            renderContext.HtmlWriter.WriteAttributeString("name", this.Name);
            renderContext.HtmlWriter.WriteAttributeString("class", "form-control");
            renderContext.HtmlWriter.WriteEndElement(); // input

            if(this.Type == SimplifiedInputType.Range)
            {
                renderContext.HtmlWriter.WriteElementString("span", NS_XHTML, $"{{ act.{this.Binding}}}");
            }
            // Validation
            if (!string.IsNullOrEmpty(this.Pattern))
            {
                this.RenderValidationError(renderContext.HtmlWriter, "pattern");
            }
            if (this.Required)
            {
                this.RenderValidationError(renderContext.HtmlWriter, "required");
            }
            if (!String.IsNullOrEmpty(this.Min))
            {
                this.RenderValidationError(renderContext.HtmlWriter, "min");
            }
            if (!String.IsNullOrEmpty(this.Max))
            {
                this.RenderValidationError(renderContext.HtmlWriter, "max");
            }
            if (this.CdssCallback)
            {
                this.RenderValidationError(renderContext.HtmlWriter, "cdss");
            }

            renderContext.HtmlWriter.WriteEndElement(); // div
        }

    }
}