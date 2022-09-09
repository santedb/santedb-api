using Newtonsoft.Json;
using SanteDB.Core.Http.Description;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Http
{

    /// <summary>
    /// A service client reprsent a single client to a service
    /// </summary>
    [XmlType(nameof(RestClientDescriptionConfiguration), Namespace = "http://santedb.org/configuration")]
    public class RestClientDescriptionConfiguration : IRestClientDescription
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public RestClientDescriptionConfiguration()
        {
            this.Endpoint = new List<RestClientEndpointConfiguration>();
        }

        /// <summary>
        /// Gets or sets the accept
        /// </summary>
        [XmlElement("accept"), JsonProperty("accept")]
        public string Accept { get; set; }

        /// <summary>
        /// The endpoints of the client
        /// </summary>
        /// <value>The endpoint.</value>
        [XmlArray("endpoints"), XmlArrayItem("add")]
        [JsonProperty("endpoints")]
        public List<RestClientEndpointConfiguration> Endpoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binding for the service client.
        /// </summary>
        /// <value>The binding.</value>
        [XmlElement("binding"), JsonProperty("binding")]
        public RestClientBindingConfiguration Binding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the service client
        /// </summary>
        /// <value>The name.</value>
        [XmlAttribute("name"), JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the proxy address
        /// </summary>
        [XmlElement("proxyAddress"), JsonProperty("proxyAddress")]
        public string ProxyAddress { get; set; }

        /// <summary>
        /// Gets the binding
        /// </summary>
        IRestClientBindingDescription IRestClientDescription.Binding => this.Binding;

        /// <summary>
        /// Gets the endpoints
        /// </summary>
        List<IRestClientEndpointDescription> IRestClientDescription.Endpoint => this.Endpoint.OfType<IRestClientEndpointDescription>().ToList();

        /// <summary>
        /// Gets or sets the trace
        /// </summary>
        [XmlElement("trace"), JsonProperty("trace")]
        public bool Trace { get; set; }

        /// <summary>
        /// Clone the object
        /// </summary>
        public RestClientDescriptionConfiguration Clone()
        {
            var retVal = this.MemberwiseClone() as RestClientDescriptionConfiguration;
            retVal.Endpoint = new List<RestClientEndpointConfiguration>(this.Endpoint.Select(o => new RestClientEndpointConfiguration
            {
                Address = o.Address,
                Timeout = o.Timeout
            }));
            return retVal;
        }
    }
}