using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Upstream
{
    /// <summary>
    /// Configuration related to an upstream domain
    /// </summary>
    [XmlType(nameof(UpstreamDomainConfiguration), Namespace = "http://santedb.org/configuration")]
    public class UpstreamDomainConfiguration
    {

        /// <summary>
        /// The name of the upstream domain
        /// </summary>
        [XmlAttribute("domain"), JsonProperty("domain")]
        public string DomainName { get; set; }

    }
}