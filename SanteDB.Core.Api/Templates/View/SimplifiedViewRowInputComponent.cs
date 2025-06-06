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
 * Date: 2024-12-22
 */
using Newtonsoft.Json;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Simplified view component
    /// </summary>
    [XmlType(nameof(SimplifiedViewRowInputComponent), Namespace = "http://santedb.org/model/template/view")]
    public abstract class SimplifiedViewRowInputComponent : SimplifiedViewContentComponent
    {

        /// <summary>
        /// True if the value of this input is required for the visit to be submitted
        /// </summary>
        [XmlAttribute("required"), JsonProperty("required")]
        public bool Required { get; set; }

        /// <summary>
        /// True if the changing of this value should trigger a CDSS execution or analysis
        /// </summary>
        [XmlAttribute("cdss"), JsonProperty("cdss")]
        public bool CdssCallback { get; set; }

        /// <summary>
        /// The HDSI path where the value captured in this input should be placed
        /// </summary>
        [XmlAttribute("binding"), JsonProperty("binding")]
        public string Binding { get; set; }

        /// <summary>
        /// Render input core attributes
        /// </summary>
        protected void RenderInputCoreAttributes(XmlWriter htmlWriter)
        {
            if (this.Required)
            {
                htmlWriter.WriteAttributeString("required", "required");
            }
            if (this.CdssCallback)
            {
                htmlWriter.WriteAttributeString("cdss-interactive", "act");
            }

            htmlWriter.WriteAttributeString("ng-model", $"act.{this.Binding}");
            htmlWriter.WriteAttributeString("name", this.Name);

        }

        /// <summary>
        /// Render validation error HTML
        /// </summary>
        protected void RenderValidationError(XmlWriter htmlWriter, string errorType)
        {
            htmlWriter.WriteStartElement("div", NS_XHTML);
            htmlWriter.WriteAttributeString("class", "text-danger");
            htmlWriter.WriteAttributeString("ng-if", $"(ownerForm || $parent.ownerForm || $parent.$parent.ownerForm).{this.Name}.$error.{errorType}");

            if (!String.Equals(errorType, "cdss", StringComparison.CurrentCultureIgnoreCase))
            {
                htmlWriter.WriteStartElement("i", NS_XHTML);
                htmlWriter.WriteAttributeString("class", "fas fa-fw fa-exclamation-triangle");
                htmlWriter.WriteRaw(" ");
                htmlWriter.WriteEndElement(); // i
                htmlWriter.WriteString($"{{{{ 'ui.error.{errorType}' | i18n }}}}");
            }
            else
            {
                htmlWriter.WriteStartElement("ul", NS_XHTML);
                htmlWriter.WriteAttributeString("class", "list-unstyled");
                htmlWriter.WriteStartElement("li", NS_XHTML);
                htmlWriter.WriteAttributeString("ng-repeat", $"issue in (ownerForm || $parent.ownerForm || $parent.$parent.ownerForm).{this.Name}.$cdss track by $index");
                htmlWriter.WriteAttributeString("ng-class", "{ 'text-danger' : issue.priority == 'Error', 'text-warning' : issue.priority == 'Warning', 'text-info' : issue.priority == 'Information' }");
                htmlWriter.WriteStartElement("i", NS_XHTML);
                htmlWriter.WriteAttributeString("class", "fas fa-fw");
                htmlWriter.WriteAttributeString("ng-class", "{ 'fa-exclamation-triangle' : issue.priority == 'Error', 'fa-info-circle' : issue.priority == 'Warning' || issue.priority == 'Information' }");
                htmlWriter.WriteEndElement();// i
                htmlWriter.WriteString("{{ issue.text }}");
                htmlWriter.WriteEndElement(); // li
                htmlWriter.WriteEndElement(); // ul
            }
            htmlWriter.WriteEndElement(); // div
        }
    }
}