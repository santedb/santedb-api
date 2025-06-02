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
 * Date: 2024-12-22
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// A concept-selection input which allows the user to select a value from a codified list configured by the administrator
    /// </summary>
    [XmlType(nameof(SimplifiedViewConceptSelect), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewConceptSelect : SimplifiedViewRowInputComponent
    {

        /// <summary>
        /// The concept set which is to be used to populate this dropdown, for example: AdministrativeGenderConcept
        /// </summary>
        [XmlAttribute("concept-set"), JsonProperty("conceptSet")]
        public string ConceptSet { get; set; }

        /// <summary>
        /// Additional concepts which should be included in the dropdown
        /// </summary>
        [XmlElement("include"), JsonProperty("include")]
        public List<Guid> IncludeConcepts { get; set; }

        /// <summary>
        /// Concepts which should be excluded from the concept set
        /// </summary>
        [XmlElement("exclude"), JsonProperty("exclude")]
        public List<Guid> ExcludeConcepts { get; set; }

        /// <inheritdoc/>
        public override void Render(SimplifiedViewRenderContext renderContext)
        {
            renderContext.HtmlWriter.WriteStartElement("div", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("class", base.GetCssClasses(renderContext));

            // Add an interpretation code 
            renderContext.HtmlWriter.WriteStartElement("concept-select", NS_XHTML);
            renderContext.HtmlWriter.WriteAttributeString("concept-set", $"'{this.ConceptSet}'");

            if (this.IncludeConcepts.Any())
            {
                renderContext.HtmlWriter.WriteAttributeString("add-concept", $"[ {String.Join(",", this.IncludeConcepts.Select(o => $"'{o}'"))} ]");
            }
            if (this.ExcludeConcepts.Any())
            {
                renderContext.HtmlWriter.WriteAttributeString("exclude-concepts", $"[ {String.Join(",", this.IncludeConcepts.Select(o => $"'{o}'"))} ]");
            }
            renderContext.HtmlWriter.WriteAttributeString("class", "form-control");

            base.RenderInputCoreAttributes(renderContext.HtmlWriter);

            renderContext.HtmlWriter.WriteEndElement(); // concept-select

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