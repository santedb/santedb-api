/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using Newtonsoft.Json;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Interop
{
    /// <summary>
    /// Service options
    /// </summary>
    [XmlType(nameof(ServiceOptions), Namespace = "http://santedb.org/model"), JsonObject(nameof(ServiceOptions))]
    public class ServiceOptions : IdentifiedData
    {
        /// <summary>
        /// Services offered
        /// </summary>
        public ServiceOptions()
        {
            this.Resources = new List<ServiceResourceOptions>();
            this.Endpoints = new List<ServiceEndpointOptions>();
        }

        /// <summary>
        /// Gets or sets the version of the service interface
        /// </summary>
        [XmlAttribute("version"), JsonProperty("version")]
        public String InterfaceVersion { get; set; }

        /// <summary>
        /// Gets the service resource options
        /// </summary>
        [XmlElement("resource"), JsonProperty("resource")]
        public List<ServiceResourceOptions> Resources { get; set; }

        /// <summary>
        /// Gets or sets the endpoint options
        /// </summary>
        [XmlElement("endpoint"), JsonProperty("endpoint")]
        public List<ServiceEndpointOptions> Endpoints { get; set; }

        /// <summary>
        /// Gets or sets the flags on the service
        /// </summary>
        [XmlElement("flag"), JsonProperty("flags")]
        public List<String> Flags { get; set; }

        /// <summary>
		/// Gets or sets the modified on date time of the service options.
		/// </summary>
        public override DateTimeOffset ModifiedOn => DateTimeOffset.Now;
    }
}
