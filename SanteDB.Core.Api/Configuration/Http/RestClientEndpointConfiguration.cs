/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using Newtonsoft.Json;
using SanteDB.Core.Http.Description;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Http
{

    /// <summary>
    /// Represnts a single endpoint for use in the service client
    /// </summary>
    [XmlType(nameof(RestClientEndpointConfiguration), Namespace = "http://santedb.org/configuration")]
    public class RestClientEndpointConfiguration : IRestClientEndpointDescription
    {
        /// <summary>
        /// Timeout of 4 sec
        /// </summary>
        public RestClientEndpointConfiguration()
        {
            this.Timeout = new TimeSpan(0, 1, 0);
        }

        /// <summary>
        /// Create a new endpoint configuration with the specified address
        /// </summary>
        public RestClientEndpointConfiguration(String address, TimeSpan? timeout = null)
        {
            this.Address = address;
            this.Timeout = timeout ?? new TimeSpan(0, 1, 0);
        }

        /// <summary>
        /// Gets or sets the service client endpoint's address
        /// </summary>
        /// <value>The address.</value>
        [XmlAttribute("address"), JsonProperty("address")]
        public string Address
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        [XmlAttribute("timeout"), JsonProperty("timeout")]
        public String TimeoutXml
        {
            get => XmlConvert.ToString(this.Timeout);
            set
            {
                if (TimeSpan.TryParse(value, out var ts))
                {
                    this.Timeout = ts;
                }
                else
                {
                    this.Timeout = XmlConvert.ToTimeSpan(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the timeout
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public TimeSpan Timeout { get; set; }
    }
}
