using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Upstream
{
    /// <summary>
    /// Configuration related to an upstream domain
    /// </summary>
    [XmlType(nameof(UpstreamRealmConfiguration), Namespace = "http://santedb.org/configuration")]
    public class UpstreamRealmConfiguration
    {

        /// <summary>
        /// The name of the upstream domain name (domain.deployment.com)
        /// </summary>
        [XmlAttribute("domain"), JsonProperty("domain")]
        public string DomainName { get; set; }

        /// <summary>
        /// The port of the domain
        /// </summary>
        [XmlAttribute("port"), JsonProperty("port")]
        public int PortNumber { get; set; }

        /// <summary>
        /// Use TLS on this connection
        /// </summary>
        [XmlAttribute("tls"), JsonProperty("tls")]
        public bool UseTls { get; set; }
    }
}