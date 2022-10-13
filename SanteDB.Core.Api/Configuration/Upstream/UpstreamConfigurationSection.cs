using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Upstream
{
    /// <summary>
    /// Upstream configuration section controls the configuration of this instance of SanteDB and information on the upstream 
    /// service
    /// </summary>
    [XmlType(nameof(UpstreamConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class UpstreamConfigurationSection : IEncryptedConfigurationSection
    {

        /// <summary>
        /// The upstream domain configuration information
        /// </summary>
        [XmlElement("domain"), JsonProperty("domain")]
        public UpstreamDomainConfiguration Domain { get; set; }

        /// <summary>
        /// Credentials that are used to contact the upstream
        /// </summary>
        [XmlElement("credentials"), JsonProperty("credentials")]
        public List<UpstreamCredentialConfiguration> Credentials { get; set; }

    }

}
