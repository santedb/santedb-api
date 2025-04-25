﻿/*
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
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Simplified list type
    /// </summary>
    [XmlType(nameof(SimplifiedListType), Namespace = "http://santedb.org/model/template/view")]
    public enum SimplifiedListType
    {
        /// <summary>
        /// A bulleted list
        /// </summary>
        [XmlEnum("bullet")]
        Bullets,
        /// <summary>
        /// A numerical list (1, 2, 3, etc.)
        /// </summary>
        [XmlEnum("number")]
        Number,
        /// <summary>
        /// A definitional list with terms and definitions
        /// </summary>
        [XmlEnum("definition")]
        Definition
    }

    /// <summary>
    /// Simplified view list
    /// </summary>
    [XmlType(nameof(SimplifiedViewListBlock), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewListBlock : SimplifiedViewContentComponent
    {

        /// <summary>
        /// The type of list block which is represented
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public SimplifiedListType Type { get; set; }

        /// <summary>
        /// Items which are in the list
        /// </summary>
        [XmlElement("item"), JsonProperty("item")]
        public List<SimplifiedViewListItem> Items { get; set; }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {

            switch (this.Type) {
                case SimplifiedListType.Definition:
                    renderContext.HtmlWriter.WriteStartElement("dl", NS_XHTML);
                    break;
                case SimplifiedListType.Bullets:
                    renderContext.HtmlWriter.WriteStartElement("ul", NS_XHTML);
                    break;
                case SimplifiedListType.Number:
                    renderContext.HtmlWriter.WriteStartElement("ol", NS_XHTML);
                    break;
            }

            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
            var childContext = renderContext.CreateChildContext(this);
            foreach (var itm in this.Items)
            {
                itm.Render(childContext);
            }
            renderContext.HtmlWriter.WriteEndElement();

        }
    }
}