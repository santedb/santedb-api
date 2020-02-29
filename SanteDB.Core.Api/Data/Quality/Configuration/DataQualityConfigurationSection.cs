using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Configuration for data quality configuration
    /// </summary>
    [XmlType(nameof(DataQualityConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class DataQualityConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Gets or sets the rule sets for the data quality engine
        /// </summary>
        [XmlElement("ruleSet")]
        public List<DataQualityRulesetConfiguration> RuleSets { get; set; }

    }
}
