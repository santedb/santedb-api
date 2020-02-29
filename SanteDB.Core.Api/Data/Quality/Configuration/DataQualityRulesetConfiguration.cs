using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Represents a data quality rule set
    /// </summary>
    [XmlType(nameof(DataQualityRulesetConfiguration), Namespace = "http://santedb.org/configuration")]
    public class DataQualityRulesetConfiguration
    {

        /// <summary>
        /// Gets or sets whether the rule set is enabled
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the rule set
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the rule set
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Adds the specified resources
        /// </summary>
        [XmlArray("resources"), XmlArrayItem("add")]
        public List<DataQualityResourceConfiguration> Resources { get; set; }

    }
}