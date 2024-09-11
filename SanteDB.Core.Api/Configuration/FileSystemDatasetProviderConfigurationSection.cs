using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// File system dataset provider configuration section
    /// </summary>
    [XmlType(nameof(FileSystemDatasetProviderConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class FileSystemDatasetProviderConfigurationSection : IConfigurationSection
    {


        /// <summary>
        /// Gets or sets the sources of the dataset processing
        /// </summary>
        [XmlArray("sources"), XmlArrayItem("add"), JsonProperty("sources")]
        public List<String> Sources { get; set; }

    }
}
