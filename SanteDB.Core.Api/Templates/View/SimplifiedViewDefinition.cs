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
 * Date: 2024-12-12
 */
using Newtonsoft.Json;
using SanteDB.Core.Templates.Definition;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.View
{
    /// <summary>
    /// Layout type
    /// </summary>
    [XmlType(nameof(ViewDefinitionLayoutType), Namespace = "http://santedb.org/model/template/view")]
    public enum ViewDefinitionLayoutType
    {
        [XmlEnum("grid")]
        grid,
        [XmlEnum("flex")]
        flex
    }

    /// <summary>
    /// Gets or sets the simplified data entry view
    /// </summary>
    [XmlType(nameof(SimplifiedViewDefinition), Namespace = "http://santedb.org/model/template/view")]
    [XmlRoot(nameof(SimplifiedViewDefinition), Namespace = "http://santedb.org/model/template/view")]
    public class SimplifiedViewDefinition
    {
        private const string NS_XHTML = "http://www.w3.org/1999/xhtml";
        private static readonly XmlSerializer m_xsz;

        /// <summary>
        /// STATIC CTOR
        /// </summary>
        static SimplifiedViewDefinition()
        {
            var rt = AppDomain.CurrentDomain.GetAllTypes().Where(t => typeof(SimplifiedViewComponent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.HasCustomAttribute<XmlTypeAttribute>()).ToArray();
            m_xsz = new XmlSerializer(typeof(SimplifiedViewDefinition), rt);
        }

        /// <summary>
        /// Gets the layout pattern
        /// </summary>
        [XmlIgnore(), JsonProperty("layout")]
        public ViewDefinitionLayoutType LayoutPattern { get; set; }

        /// <summary>
        /// Gets or sets the layout
        /// </summary>
        [XmlChoiceIdentifier(nameof(LayoutPattern)), 
            XmlElement("grid", Type = typeof(SimplifiedViewGridLayout)),
            XmlElement("flex", Type = typeof(SimplifiedViewFlexLayout)), JsonProperty("content")]
        public object ContentXml { get; set; }

        /// <summary>
        /// Get the content
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public ISimplifiedViewLayout Content { get => (ISimplifiedViewLayout)this.ContentXml; set => this.ContentXml = value; }

        /// <summary>
        /// Render the simplified view to <paramref name="xw"/>
        /// </summary>
        public void Render(XmlWriter xw)
        {
            xw.WriteStartElement("div", NS_XHTML);
            var context = SimplifiedViewRenderContext.Create(xw, this.Content as SimplifiedViewComponent);
            this.Content?.Render(context);
            xw.WriteEndElement(); // div
        }

        /// <summary>
        /// Parse the view content definition
        /// </summary>
        public static SimplifiedViewDefinition Parse(string str)
        {
            using (var sr = new StringReader(str))
            using (var xr = XmlReader.Create(sr))
            {
                return m_xsz.Deserialize(xr) as SimplifiedViewDefinition;
            }
        }

        /// <summary>
        /// Load a view definition from <paramref name="stream"/>
        /// </summary>
        /// <param name="stream">The stream from which the view definition should be loaded</param>
        /// <returns>The loaded view definition</returns>
        public static SimplifiedViewDefinition Load(Stream stream)
        {
            using(var xr = XmlReader.Create(stream))
            {
                return m_xsz.Deserialize(xr) as SimplifiedViewDefinition;
            }
        }

        /// <summary>
        /// Save the view definition
        /// </summary>
        /// <param name="stream">The stream to which the view definition should be saved</param>
        public void Save(Stream stream)
        {
            m_xsz.Serialize(stream, this);
        }

    }
}