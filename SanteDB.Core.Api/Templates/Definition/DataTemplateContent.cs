using Newtonsoft.Json;
using System;
using System.Net.Mime;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.Definition
{
    /// <summary>
    /// Content type
    /// </summary>
    public enum DataTemplateContentType
    {
        content,
        reference
    }

    /// <summary>
    /// Content for a model
    /// </summary>
    [XmlType(nameof(DataTemplateContent), Namespace = "http://santedb.org/model/template")]
    public class DataTemplateContent
    {
        /// <summary>
        /// Gets or sets the JSON template
        /// </summary>
        [XmlChoiceIdentifier(nameof(ContentType)), XmlElement("content"), XmlElement("reference"), JsonProperty("content")]
        public String Content { get; set; }

        /// <summary>
        /// Content type
        /// </summary>
        [XmlIgnore, JsonProperty("type")]
        public DataTemplateContentType ContentType { get; set; }

    }
}