/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
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
