using Newtonsoft.Json;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Templates.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.Definition
{
    /// <summary>
    /// Represents a user-defined template
    /// </summary>
    [XmlType(nameof(DataTemplateDefinition), Namespace = "http://santedb.org/model/template")]
    [XmlRoot(nameof(DataTemplateDefinition), Namespace = "http://santedb.org/model/template")]
    [NonCached]
    public class DataTemplateDefinition : IdentifiedData
    {
        // XML serializer
        private static readonly XmlSerializer m_xsz;
        private static Regex m_bindingRegex = new Regex("{{\\s?\\$([A-Za-z0-9_]*?)\\s?}}", RegexOptions.Compiled);
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
        /// <returns></returns>
        public bool ShouldSerializeKey() => false;

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
        public void Save(Stream stream)
        {
            this.m_saving = true;
            m_xsz.Serialize(stream, this);
            this.m_saving = false;
        }

        /// <summary>
        /// Fill the template definition
        /// </summary>
        public String Fill(IDictionary<String, String> parameters, Func<String, String> referenceResolver = null)
        {
            parameters = parameters ?? new Dictionary<String, String>();
            parameters.Add("today", DateTimeOffset.Now.Date.ToString("yyyy-MM-dd"));
            parameters.Add("now", DateTimeOffset.Now.ToString("o"));
            if (this.JsonTemplate.ContentType == DataTemplateContentType.content)
            {
                return m_bindingRegex.Replace(this.JsonTemplate.Content, (m) => parameters.TryGetValue(m.Groups[1].Value, out string v) ? v : m.ToString()); 
            }
            else if(referenceResolver != null)
            {
                return m_bindingRegex.Replace(referenceResolver(this.JsonTemplate.Content), (m) => parameters.TryGetValue(m.Groups[1].Value, out string v) ? v : m.ToString());

            }
            else
            {
                throw new InvalidOperationException(this.JsonTemplate.ContentType.ToString());
            }

        }

        /// <inheritdoc/>
        public override string ToString() => $"Template: {this.Name}";
    }
}
