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
 * Date: 2024-12-23
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{

    /// <summary>
    /// Base class for a component collection
    /// </summary>
    [XmlType(nameof(SimplifiedViewComponentCollection), Namespace = "http://santedb.org/model/template/view")]
    public abstract class SimplifiedViewComponentCollection : SimplifiedViewComponent
    {

        /// <summary>
        /// Gets or sets the content
        /// </summary>
        [JsonProperty("content"),
            XmlElement("accordion", typeof(SimplifiedViewAccordion)),
            XmlElement("label", typeof(SimplifiedViewLabel)),
            XmlElement("input", typeof(SimplifiedViewInput)),
            XmlElement("select", typeof(SimplifiedViewSelect)),
            XmlElement("concept", typeof(SimplifiedViewConceptSelect)),
            XmlElement("flex", typeof(SimplifiedViewFlexLayout)),
            XmlElement("grid", typeof(SimplifiedViewGridLayout)),
            XmlElement("checkbox", typeof(SimplifiedCheckboxInput)),
            XmlElement("value", typeof(SimplifiedViewValue)),
            XmlElement("html", typeof(XElement), Namespace = NS_XHTML),
            XmlElement("span", typeof(SimplifiedViewTextBlock)),
            XmlElement("list", typeof(SimplifiedViewListBlock)),
            XmlElement("hint", typeof(SimplifiedViewHintComponent)),
            XmlElement("component", typeof(SimplifiedViewComponent))]
        public List<object> Content { get; set; }

        /// <summary>
        /// Render the content
        /// </summary>
        /// <param name="renderContext"></param>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext = renderContext.CreateChildContext(this);
            foreach(var itm in this.Content)
            {
                if (itm is SimplifiedViewComponent svc)
                {
                    svc.Render(renderContext);
                }
                else if(itm is XElement xe)
                {
                    xe.WriteTo(renderContext.HtmlWriter);
                }
            }
        }
    }
}