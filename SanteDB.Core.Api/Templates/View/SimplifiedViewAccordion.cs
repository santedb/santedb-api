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
 * Date: 2024-12-30
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Represents a simplified accordion which is a collection of accordion panels where only one panel can be expanded at a time
    /// </summary>
    [XmlType(nameof(SimplifiedViewAccordion), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewAccordion : SimplifiedViewContentComponent
    {

        /// <summary>
        /// Represents a single accordion pane
        /// </summary>
        [XmlElement("panel"), JsonProperty("content")]
        public List<SimplifiedViewAccordionPane> Content { get; set; }


        /// <inheritdoc/>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            return $"accordion {base.GetCssClasses(renderContext)}".Trim();
        }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));

            this.Name = this.Name ?? $"acc{Guid.NewGuid().ToString().Substring(0, 6)}";
            renderContext.HtmlWriter.WriteAttributeString("id", this.Name);

            var childContext = renderContext.CreateChildContext(this);
            foreach(var itm in this.Content)
            {
                itm.Render(childContext);
            }

            renderContext.HtmlWriter.WriteEndElement();
        }
    }
}