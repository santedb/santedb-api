/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-12-23
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Info block type
    /// </summary>
    [XmlType(nameof(SimplifiedInfoBlockType), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedInfoBlockType
    {
        /// <summary>
        /// The block is informational
        /// </summary>
        [XmlEnum("info")]
        Info,
        /// <summary>
        /// The block is a warning
        /// </summary>
        [XmlEnum("warn")]
        Warning,
        /// <summary>
        /// The block is a danger (error, risk, etc.)
        /// </summary>
        [XmlEnum("danger")]
        Danger
    }

    /// <summary>
    /// An information block which can be used to show instructions, alerts, notes, etc.
    /// </summary>
    [XmlType(nameof(SimplifiedViewHintComponent), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewHintComponent : SimplifiedViewComponent
    {

        /// <summary>
        /// The type of informational block this object represents
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public SimplifiedInfoBlockType Type { get; set; }

        /// <summary>
        /// The title of the informational block
        /// </summary>
        [XmlElement("title"), JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the content
        /// </summary>
        [XmlElement("html", typeof(XElement), Namespace = NS_XHTML), 
            XmlElement("span", typeof(SimplifiedViewTextBlock)),
            XmlElement("flex", typeof(SimplifiedViewFlexLayout)),
            XmlElement("list", typeof(SimplifiedViewListBlock)), JsonProperty("content")]
        public List<object> Content { get; set; }

        /// <summary>
        /// Get css classes
        /// </summary>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            var cssClasses = "d-flex justify-content-left m-auto";
            switch(this.Type) {
                case SimplifiedInfoBlockType.Danger:
                    cssClasses += " alert alert-danger";
                    break;
                case SimplifiedInfoBlockType.Warning:
                    cssClasses += " alert alert-warning";
                    break;
                case SimplifiedInfoBlockType.Info:
                    cssClasses += " alert alert-info";
                    break;
            }
            return $"{cssClasses} {base.GetCssClasses(renderContext)}".Trim();
        }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {

            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));

            // Info set 
            renderContext.HtmlWriter.WriteStartElement("h5", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "m-0 p-2");
            renderContext.HtmlWriter.WriteStartElement("i", NS_XHTML);
            switch(this.Type)
            {
                case SimplifiedInfoBlockType.Danger:
                    renderContext.HtmlWriter.WriteAttributeString("class", "fas fa-fw fa-exclamation-triangle m-auto d-block");
                    break;
                case SimplifiedInfoBlockType.Warning:
                    renderContext.HtmlWriter.WriteAttributeString("class", "fas fa-fw fa-exclamation-circle m-auto d-block");
                    break;
                case SimplifiedInfoBlockType.Info:
                    renderContext.HtmlWriter.WriteAttributeString("class", "fas fa-fw fa-info-circle m-auto d-block");
                    break;
            }

            renderContext.HtmlWriter.WriteEndElement();// i
            renderContext.HtmlWriter.WriteEndElement(); // div

            // Title
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", "d-flex flex-column mx-1");
            if(!string.IsNullOrEmpty(this.Title))
            {
                renderContext.HtmlWriter.WriteStartElement("h5", NS_XHTML);
                renderContext.HtmlWriter.WriteAttributeString("class", "mb-1 m-0 p-0");
                renderContext.HtmlWriter.WriteString(this.Title);
                renderContext.HtmlWriter.WriteEndElement(); // h5
            }

            var childContext = renderContext.CreateChildContext(this);
            foreach(var itm in this.Content)
            {
                if (itm is SimplifiedViewComponent scv)
                {
                    scv.Render(childContext);
                }
                else if(itm is XElement xe)
                {
                    xe.WriteTo(renderContext.HtmlWriter);
                }
            }
            renderContext.HtmlWriter.WriteEndElement(); // div flex-column
            renderContext.HtmlWriter.WriteEndElement(); // div alert
        }
    }
}