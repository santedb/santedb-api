using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Confiugration related to auditing and accountability
    /// </summary>
    [XmlType(nameof(AuditAccountabilityConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class AuditAccountabilityConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// When set to true, enables complete audit trail
        /// </summary>
        [XmlAttribute("completeAuditTrail"), JsonProperty("completeAuditTrail")]
        public bool CompleteAuditTrail { get; set; }

        /// <summary>
        /// Gets or sets filters to apply to the audit trail (i.e. ignore)
        /// </summary>
        [XmlArray("filters"), XmlArrayItem("add"), JsonProperty("filters")]
        public List<AuditFilterConfiguration> AuditFilters { get; set; }
    }
}
