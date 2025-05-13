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
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Grid Layout - Items are placed into a grid
    /// </summary>
    [XmlType(nameof(SimplifiedViewGridLayout), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewGridLayout : SimplifiedViewContentComponent, ISimplifiedViewLayout
    {

        /// <summary>
        /// Rows which are to be used in the layout
        /// </summary>
        [XmlElement("row"), JsonProperty("content")]
        public List<SimplifiedViewRow> Content { get; set; }

        /// <summary>
        /// Get CSS classes
        /// </summary>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            return $"container-fluid {base.GetCssClasses(renderContext)}".Trim();
        }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
            renderContext = renderContext.CreateChildContext(this);
            foreach (var itm in this.Content)
            {
                itm.Render(renderContext);
            }
            renderContext.HtmlWriter.WriteEndElement();
        }

    }
}