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
    /// A single selectio option
    /// </summary>
    [XmlType(nameof(SimplifiedViewSelectOption), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewSelectOption
    {

        /// <summary>
        /// The value that should be placed into the model binding when this value is selected
        /// </summary>
        [XmlAttribute("value"), JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// The display of this option (as it appears to the user)
        /// </summary>
        [XmlText(), JsonProperty("text")]
        public string Display { get; set; }

        /// <summary>
        /// Render out
        /// </summary>
        internal void Render(XmlWriter writer)
        {
            writer.WriteStartElement("option", "http://www.w3.org/1999/xhtml");
            writer.WriteAttributeString("value", this.Value);
            writer.WriteString(this.Display);
            writer.WriteEndElement();// option
        }
    }
}