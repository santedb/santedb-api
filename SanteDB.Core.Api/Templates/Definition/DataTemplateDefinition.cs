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
 * Date: 2024-12-12
 */
using Newtonsoft.Json;
using SanteDB.Core.ViewModel.Json;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Templates.View;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Services;
using SanteDB.Core.Cdss;

namespace SanteDB.Core.Templates.Definition
{
    /// <summary>
    /// Represents a user-defined template
    /// </summary>
    [XmlType(nameof(DataTemplateDefinition), Namespace = "http://santedb.org/model/template")]
    [XmlRoot(nameof(DataTemplateDefinition), Namespace = "http://santedb.org/model/template")]
    [NonCached]
    public class DataTemplateDefinition : NonVersionedEntityData
    {
        // XML serializer
        private static readonly XmlSerializer m_xsz;
        private static Regex m_bindingRegex = new Regex("{{\\s?(\\$[A-Za-z0-9_]*?)\\s?}}", RegexOptions.Compiled);
        private static Regex m_repeatRegex = new Regex(@"[#""]for\s+(?:(\$\w+)|\(((?:\$\w+:?){1,})\))\s+in\s+\@(\w+)(?:"",)?(.*?)(?:,.*?""|#)end""?", RegexOptions.Compiled | RegexOptions.Singleline);
        private bool m_saving;

        /// <summary>
        /// Static CTOR
        /// </summary>
        static DataTemplateDefinition()
        {
            var rt = AppDomain.CurrentDomain.GetAllTypes().Where(t => typeof(SimplifiedViewComponent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.HasCustomAttribute<XmlTypeAttribute>()).ToArray();
            m_xsz = new XmlSerializer(typeof(DataTemplateDefinition), rt);
        }

        /// <summary>
        /// Gets or sets the unique identifier for the template
        /// </summary>
        [XmlElement("uuid"), JsonProperty("uuid")]
        public Guid Uuid { get => this.Key.GetValueOrDefault(); set => this.Key = value; }

        /// <summary>
        /// Gets or sets the version of the definition file
        /// </summary>
        [XmlElement("version"), JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// Don't serialize key
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeVersion() => !this.m_saving;

        /// <summary>
        /// Don't serialize key
        /// </summary>
        public new bool ShouldSerializeKey() => false;

        /// <summary>
        /// Don't serialize obs time
        /// </summary>
        public bool ShouldSerializeObsoletionTime() => !this.m_saving;

        /// <summary>
        /// Don't serialize create time
        /// </summary>
        public bool ShouldSerializeCreationTime() => !this.m_saving;

        /// <summary>
        /// Don't serialize created by key
        /// </summary>
        public new bool ShouldSerializeCreatedByKey() => !this.m_saving;

        /// <summary>
        /// Don't serialize obs key
        /// </summary>
        /// <returns></returns>
        public new bool ShouldSerializeObsoletedByKey() => !this.m_saving;

        /// <summary>
        /// Don't serialize updated time
        /// </summary>
        public bool ShouldSerializeUpdatedTime() => !this.m_saving;

        /// <summary>
        /// Dont' serialize updated by key
        /// </summary>
        public new bool ShouldSerializeUpdatedByKey() => !this.m_saving;

        /// <summary>
        /// Modified on
        /// </summary>
        public override DateTimeOffset ModifiedOn => this.LastUpdated;

        /// <summary>
        /// True if the template is readonly
        /// </summary>
        [XmlElement("isReadonly"), JsonProperty("isReadonly")]
        public bool Readonly { get; set; }

        /// <summary>
        /// Gets or sets the authors of the definition
        /// </summary>
        [XmlElement("author"), JsonProperty("author")]
        public List<string> Author { get; set; }

        /// <summary>
        /// Gets or sets the icons
        /// </summary>
        [XmlElement("icon"), JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Get the last modified time
        /// </summary>
        [XmlElement("lastModified"), JsonProperty("lastModified")]
        public DateTime LastUpdated { get; set; }

        /// <inheritdoc/>
        public bool ShouldSerializeReadonly() => !this.m_saving;

        /// <inheritdoc/>
        public bool ShouldSerializeLastUpdated() => !this.m_saving;

        /// <summary>
        /// True if the data template appears in lists
        /// </summary>
        [XmlElement("public"), JsonProperty("public")]
        public bool Public { get; set; }

        /// <summary>
        /// Gets or sets the mnemonic identifier
        /// </summary>
        [XmlElement("mnemonic"), JsonProperty("mnemonic")]
        public String Mnemonic { get; set; }

        /// <summary>
        /// Gets or sets the OID
        /// </summary>
        [XmlElement("oid"), JsonProperty("oid")]
        public String Oid { get; set; }

        /// <summary>
        /// Get or sets the human name of the template
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        [XmlElement("description"), JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the guard conditions under which the template can be added
        /// </summary>
        [XmlElement("guard"), JsonProperty("guard")]
        public string Guard { get; set; }

        /// <summary>
        /// Gets or sets the scopes of the 
        /// </summary>
        [XmlArray("scopes"), XmlArrayItem("scope"), JsonProperty("scopes")]
        public List<String> Scopes { get; set; }

        /// <inheritdoc/>
        public bool ShouldSerializeScopes() => this.Scopes?.Any() == true;
 
        /// <summary>
        /// Gets or sets the json template
        /// </summary>
        [XmlElement("template"), JsonProperty("template")]
        public DataTemplateContent JsonTemplate { get; set; }

        /// <summary>
        /// Gets the views which are contained in the user defined template definition
        /// </summary>
        [XmlArray("views"), XmlArrayItem("view"), JsonProperty("views")]
        public List<DataTemplateView> Views { get; set; }

        /// <summary>
        /// True if the definition is active
        /// </summary>
        [XmlElement("active"), JsonProperty("active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Identifies the CDSS calback hook 
        /// </summary>
        [XmlElement("cdss"), JsonProperty("cdss")]
        public DataTemplateCdssCallback CdssCallback { get; set; }

        /// <inheritdoc/>
        public bool ShouldSerializeIsActive() => !this.m_saving;


        /// <summary>
        /// Load a data template definition from <paramref name="stream"/>
        /// </summary>
        /// <param name="stream">The stream to load from</param>
        /// <returns>The loaded data template definition</returns>
        public static DataTemplateDefinition Load(Stream stream)
        {
            return (DataTemplateDefinition)m_xsz.Deserialize(stream);
        }

        /// <summary>
        /// Save the data template definition to <paramref name="stream"/>
        /// </summary>
        /// <param name="stream">The stream to which the data in this template definition should be written</param>
        public void Save(Stream stream, bool includeMetadata = false)
        {
            this.m_saving = !includeMetadata;
            m_xsz.Serialize(stream, this);
            this.m_saving = false;
        }

        /// <summary>
        /// Fill this template as an object
        /// </summary>
        /// <returns></returns>
        public IdentifiedData FillObject(IDictionary<String, String> parameters, Func<String, String> referenceResolver)
        {
            using(var modelSer = new JsonViewModelSerializer())
            {
                var retVal = modelSer.DeSerialize<IdentifiedData>(this.FillJson(parameters, referenceResolver));
                retVal.Key = retVal.Key ?? Guid.NewGuid();
                if(retVal is IHasTemplate iht)
                {
                    iht.TemplateKey = this.Uuid;
                }

                // Is there CDSS to be applied?
                if(this.CdssCallback != null)
                {
                    this.CdssCallback.AddCdssActions(this, retVal);
                    
                }
                return retVal;
            }
        }

        /// <summary>
        /// Fill the template definition
        /// </summary>
        public String FillJson(IDictionary<String, String> parameters, Func<String, String> referenceResolver)
        {
            parameters = parameters ?? new Dictionary<String, String>();
            parameters.Add("today", DateTimeOffset.Now.Date.ToString("yyyy-MM-dd"));
            parameters.Add("now", DateTimeOffset.Now.ToString("o"));
            parameters.Add("nowMinute", DateTimeOffset.Now.ToString("yyyy-MM-ddTHH:mm"));

            var jsonContentRaw = this.JsonTemplate.ContentType == DataTemplateContentType.content ? this.JsonTemplate.Content : referenceResolver(this.JsonTemplate.Content);

            // Perform repeat instructions
            jsonContentRaw = m_repeatRegex.Replace(jsonContentRaw, (m) =>
            {
                var variableNameRaw = !String.IsNullOrEmpty(m.Groups[1].Value) ? m.Groups[1].Value : m.Groups[2].Value;
                var sourceArrayName = m.Groups[3].Value;
                var contents = m.Groups[4].Value;
                if(parameters.TryGetValue(sourceArrayName, out string sourceArray))
                {
                    // Repeat
                    return String.Join(",", sourceArray.Split(',').Select(parmValueSource =>
                        m_bindingRegex.Replace(contents, (m2) => {
                            var variableValues = parmValueSource.Split(':');
                            var variableNames = variableNameRaw.Split(':');
                            var varIdx = Array.FindIndex<String>(variableNames, o => o.Equals(m2.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                            if(varIdx != -1 && varIdx < variableValues.Length)
                            {
                                return variableValues[varIdx];
                            }
                            else
                            {
                                return String.Empty;
                            }
                        }
                    )));
                }
                else
                {
                    return ""; // No repeat
                }
            });
            jsonContentRaw = m_bindingRegex.Replace(jsonContentRaw, (m) => parameters.TryGetValue(m.Groups[1].Value.Substring(1), out string v) ? v : "");
            return jsonContentRaw;
        }

        /// <inheritdoc/>
        public override string ToString() => $"Template: {this.Name}";
    }
}
