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
        /// Default ctor
        /// </summary>
        public RestClientEndpointConfiguration()
        {
            this.ConnectTimeout = new TimeSpan(0, 1, 0);
            this.ReceiveTimeout = null;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public RestClientEndpointConfiguration(RestClientEndpointConfiguration copyFrom)
        {
            this.Address = copyFrom.Address;
            this.ConnectTimeoutXml = copyFrom.ConnectTimeoutXml;
            this.ReceiveTimeoutXml = copyFrom.ReceiveTimeoutXml;
        }

        /// <summary>
        /// Create a new endpoint configuration with the specified address
        /// </summary>
        public RestClientEndpointConfiguration(String address, TimeSpan? timeout = null)
        {
            this.Address = address;
            this.ConnectTimeout = timeout ?? new TimeSpan(0, 1, 0);
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
        /// This property is part of the configuration system and should not be used directly in code. Use <see cref="ConnectTimeout"/> instead.
        /// </summary>
        [XmlAttribute("timeout"), JsonProperty("timeout")]
        public String ConnectTimeoutXml
        {
            get => XmlConvert.ToString(this.ConnectTimeout);
            set
            {
                if (TimeSpan.TryParse(value, out var ts))
                {
                    this.ConnectTimeout = ts;
                }
                else
                {
                    this.ConnectTimeout = XmlConvert.ToTimeSpan(value);
                }
            }
        }

        /// <inheritdoc />
        [XmlIgnore, JsonIgnore]
        public TimeSpan ConnectTimeout { get; set; }

        /// <summary>
        /// This property is part of the configuration system and should not be used directly in code. Use <see cref="ReceiveTimeout"/> instead.
        /// </summary>
        [XmlAttribute("receiveTimeout"), JsonProperty("receiveTimeout")]
        public String ReceiveTimeoutXml
        {
            get => ReceiveTimeout.HasValue ? XmlConvert.ToString(this.ReceiveTimeout.Value) : null;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (TimeSpan.TryParse(value, out var ts))
                    {
                        this.ReceiveTimeout = ts;
                    }
                    else
                    {
                        this.ReceiveTimeout = XmlConvert.ToTimeSpan(value); //Tries to use an intermediary internal type to convert.

                    }
                }
                else
                {
                    ReceiveTimeout = null;
                }
            }
        }

        /// <inheritdoc />
        [XmlIgnore, JsonIgnore]
        public TimeSpan? ReceiveTimeout { get; set; }
    }
}
