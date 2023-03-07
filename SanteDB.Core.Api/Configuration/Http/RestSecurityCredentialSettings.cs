using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Http
{
    /// <summary>
    /// Security credential provider settings
    /// </summary>
    [XmlType(nameof(RestSecurityCredentialSettings), Namespace = "http://santedb.org/configuration")]
    public class RestSecurityCredentialSettings
    {

        /// <summary>
        /// Credential parameter name
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Credential provider value
        /// </summary>
        [XmlAttribute("value"), JsonProperty("value")]
        public string Value { get; set; }

    }
}