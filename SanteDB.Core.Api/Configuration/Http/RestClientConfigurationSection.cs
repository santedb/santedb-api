using Newtonsoft.Json;
using SanteDB.Core.Http;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Http
{


    /// <summary>
    /// Configuration section which is used for the configuration of all HTTP clients which use the <see cref="RestClient"/>
    /// </summary>
    [XmlType(nameof(RestClientConfigurationSection), Namespace = "http://santedb.org/configuration")]
    [JsonObject(nameof(RestClientConfigurationSection))]
    public class RestClientConfigurationSection : IConfigurationSection
    {
        /// <summary>
        /// Initializes a new instance of the <see href="SanteDB.DisconnectedClient.Configuration.ServiceClientConfigurationSection"/> class.
        /// </summary>
        public RestClientConfigurationSection()
        {
            this.Client = new List<RestClientDescriptionConfiguration>();
            this.RestClientType = new TypeReferenceConfiguration(typeof(RestClient));
        }

        /// <summary>
        /// Gets or sets the proxy address.
        /// </summary>
        /// <value>The proxy address.</value>
        [XmlElement("proxyAddress")]
        [JsonProperty("proxyAddress")]
        public string ProxyAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Represents a service client
        /// </summary>
        /// <value>The client.</value>
        [XmlArray("clients"), XmlArrayItem("add")]
        [JsonProperty("clients")]
        public List<RestClientDescriptionConfiguration> Client
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rest client implementation
        /// </summary>
        /// <value>The type of the rest client.</value>
        [XmlElement("clientType")]
        [JsonProperty("clientType")]
        public TypeReferenceConfiguration RestClientType
        {
            get;
            set;
        }

    }
}
