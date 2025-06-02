/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
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
        /// Copy configuration
        /// </summary>
        public RestClientDescriptionConfiguration(RestClientDescriptionConfiguration copyFrom)
        {
            this.Accept = copyFrom.Accept;
            this.Binding = new RestClientBindingConfiguration(copyFrom.Binding);
            this.Endpoint = copyFrom.Endpoint.Select(o => new RestClientEndpointConfiguration(o)).ToList();
            this.Name = copyFrom.Name;
            this.ProxyAddress = copyFrom.ProxyAddress;
            this.Trace = copyFrom.Trace;
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
                ConnectTimeout = o.ConnectTimeout
            }));
            return retVal;
        }
    }
}