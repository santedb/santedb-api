using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Templates.Definition
{
    /// <summary>
    /// Metadata about the template definition
    /// </summary>
    [XmlType(nameof(DataTemplateDefinitionMetadata), Namespace = "http://santedb.org/model/template")]
    public class DataTemplateDefinitionMetadata
    {

        /// <summary>
        /// Gets or sets the version of the definition file
        /// </summary>
        [XmlElement("version"), JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the authors of the definition
        /// </summary>
        [XmlElement("author"), JsonProperty("author")]
        public List<string> Author { get; set; }

        /// <summary>
        /// Gets or sets the icones
        /// </summary>
        [XmlElement("icon"), JsonProperty("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the definition
        /// </summary>
        [XmlElement("documentation"), JsonProperty("documentation")]
        public string Documentation { get; set; }

        /// <summary>
        /// Get the last modified time
        /// </summary>
        [XmlElement("lastModified"), JsonProperty("lastModified")]
        public DateTimeOffset LastUpdated { get; set; }
    }
}