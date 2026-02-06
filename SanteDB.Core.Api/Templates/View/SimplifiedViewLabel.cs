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
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A simple text label which is used to document an input's purpose
    /// </summary>
    [XmlType(nameof(SimplifiedViewLabel), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewLabel : SimplifiedViewContentComponent
    {
        /// <summary>
        /// The name of the input for which this label applies
        /// </summary>
        [XmlAttribute("for"), JsonProperty("for")]
        public string ForInput { get; set; }

        /// <summary>
        /// The text of the label
        /// </summary>
        [XmlText(), JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the hint
        /// </summary>
        [XmlElement("hint"), JsonProperty("hint")]
        public string Hint { get; set; }

        /// <summary>
        /// Get the css classes
        /// </summary>
        /// <returns></returns>
        protected override string GetCssClasses(SimplifiedViewRenderContext renderContext)
        {
            return $"control-label {base.GetCssClasses(renderContext)}".Trim();
        }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("label", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", this.GetCssClasses(renderContext));

            renderContext.HtmlWriter.WriteString(this.Text);

            if(!string.IsNullOrEmpty(this.Hint))
            {
                renderContext.HtmlWriter.WriteStartElement("hint-popover", NS_XHTML);
                renderContext.HtmlWriter.WriteAttributeString("hint-title", this.Text);
                renderContext.HtmlWriter.WriteAttributeString("hint-text", this.Hint);
                renderContext.HtmlWriter.WriteEndElement(); // hint-popover
            }

            renderContext.HtmlWriter.WriteEndElement(); // label
        }
    }
}