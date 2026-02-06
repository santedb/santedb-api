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
 * Date: 2024-12-30
 */
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