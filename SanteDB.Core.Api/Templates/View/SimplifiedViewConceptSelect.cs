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

            renderContext.HtmlWriter.WriteEndElement(); // div
        }
    }
}