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
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A simple static text block
    /// </summary>
    [XmlType(nameof(SimplifiedViewTextBlock), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewTextBlock : SimplifiedViewContentComponent
    {

        /// <summary>
        /// Gets or sets the text 
        /// </summary>
        [XmlText, JsonProperty("text")]
        public string Text { get; set; }

        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            var isInRow = renderContext.Component is SimplifiedViewRow;

            if (isInRow)
            {
                renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            }
            else
            {
                renderContext.HtmlWriter.WriteStartElement("span", NS_XHTML);
            }

            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));
            renderContext.HtmlWriter.WriteString(this.Text);
            renderContext.HtmlWriter.WriteEndElement(); // span or div
        }
    }
}