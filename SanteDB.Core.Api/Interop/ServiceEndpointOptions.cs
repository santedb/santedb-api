/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Interop
{
    /// <summary>
    /// Service endpoint type
    /// </summary>
    [XmlType(nameof(ServiceEndpointType), Namespace = "http://santedb.org/model")]
    public enum ServiceEndpointType
    {
        /// <summary>
        /// The service endpoint is the HDSI
        /// </summary>
        [XmlEnum("hdsi")]
        HealthDataService,
        /// <summary>
        /// The service endpoint is the RISI
        /// </summary>
        [XmlEnum("risi")]
        ReportIntegrationService,
        /// <summary>
        /// The service endpoint is the AMI
        /// </summary>
        [XmlEnum("ami")]
        AdministrationIntegrationService,
        /// <summary>
        /// The service endpoint is a PIX/PDQ interface
        /// </summary>
        [XmlEnum("pixpdq")]
        IhePixPdqInterface,
        /// <summary>
        /// The service endpoint is a FHIR interface
        /// </summary>
        [XmlEnum("fhir")]
        Hl7FhirInterface,
        [XmlEnum("v2")]
        Hl7v2Interface,
        /// <summary>
        /// The service endpoint is a GS1 interface
        /// </summary>
        [XmlEnum("gs1")]
        Gs1StockInterface,
        /// <summary>
        /// The service endpoint is the ACS
        /// </summary>
        [XmlEnum("acs")]
        AuthenticationService
    }

    /// <summary>
    /// Represents service capabilities
    /// </summary>
    [XmlType(nameof(ServiceEndpointCapabilities), Namespace = "http://santedb.org/model"), Flags]
    public enum ServiceEndpointCapabilities
    {
        /// <summary>
        /// No options
        /// </summary>
        [XmlEnum("none")]
        None,
        /// <summary>
        /// Basic auth
        /// </summary>
        [XmlEnum("basic")]
        BasicAuth = 0x2,
        /// <summary>
        /// Bearer auth
        /// </summary>
        [XmlEnum("bearer")]
        BearerAuth = 0x4,
        /// <summary>
        /// Endpoint supports compression
        /// </summary>
        [XmlEnum("compress")]
        Compression = 0x1,
        /// <summary>
        /// Node authentication
        /// </summary>
        [XmlEnum("nodeauth")]
        CertificateAuth = 0x8,
        /// <summary>
        /// Service has CORS
        /// </summary>
        [XmlEnum("cors")]
        Cors = 0x10,
        /// <summary>
        /// Service can produce view model objects
        /// </summary>
        ViewModel = 0x20

    }

    /// <summary>
    /// Service endpoint options
    /// </summary>
    [XmlType(nameof(ServiceEndpointOptions), Namespace = "http://santedb.org/model"), JsonObject(nameof(ServiceEndpointOptions))]
    public class ServiceEndpointOptions
    {
        /// <summary>
        /// Gets or sets the service endpoint type
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public ServiceEndpointType ServiceType { get; set; }

        /// <summary>
        /// Capabilities
        /// </summary>
        [XmlAttribute("cap"), JsonProperty("cap")]
        public ServiceEndpointCapabilities Capabilities { get; set; }

        /// <summary>
        /// Base URL type
        /// </summary>
        [XmlAttribute("url"), JsonProperty("url")]
        public string[] BaseUrl { get; set; }
    }
}