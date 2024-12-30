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