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
using SanteDB.Core.Configuration;
using SanteDB.Core.Model.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
        /// <summary>
        /// The service endpoint is the v2 service
        /// </summary>
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
        AuthenticationService,
        /// <summary>
        /// The service endpoint represents API
        /// </summary>
        [XmlEnum("meta")]
        Metadata,
        /// <summary>
        /// The service endpoint represents the BIS
        /// </summary>
        [XmlEnum("bis")]
        BusinessIntelligenceService,
        /// <summary>
        /// The service is som other sort of service
        /// </summary>
        [XmlEnum("other")]
        Other
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
        [XmlEnum("viewModel")]
        ViewModel = 0x20,
        /// <summary>
        /// Standards based API based on REST
        /// </summary>
        [XmlEnum("standardApi")]
        StandardsBasedApi = ViewModel | BearerAuth | Compression,
        /// <summary>
        /// Standards based API with REST and CORS
        /// </summary>
        [XmlEnum("standardApiCors")]
        StandardsBasedCorsApi = ViewModel | BearerAuth | Compression | Cors,
        /// <summary>
        /// Internal API
        /// </summary>
        [XmlEnum("internalApi")]
        InternalApi = ViewModel | BearerAuth |Compression,
        /// <summary>
        /// IdP API
        /// </summary>
        [XmlEnum("idpApi")]
        IdpApi = BasicAuth | Compression
    }

    /// <summary>
    /// Service endpoint options
    /// </summary>
    [XmlType(nameof(ServiceEndpointOptions), Namespace = "http://santedb.org/model"), JsonObject(nameof(ServiceEndpointOptions))]
    public class ServiceEndpointOptions
    {

        /// <summary>
        /// Default ctor for serialization
        /// </summary>
        public ServiceEndpointOptions()
        {

        }

        /// <summary>
        /// Create a new endpoint option from the specified provider
        /// </summary>
        public ServiceEndpointOptions(IApiEndpointProvider provider)
        {
            this.ServiceType = provider.ApiType;
            this.BaseUrl = provider.Url;
            this.Capabilities = provider.Capabilities;
            if (provider.BehaviorType != null)
            {
                this.Behavior = new TypeReferenceConfiguration(provider.BehaviorType);
                this.Contracts = provider.BehaviorType.GetInterfaces()
                    .Where(t => t.GetCustomAttributes(Type.GetType("RestSrvr.Attributes.ServiceContractAttribute, RestSrvr, Version=1.31.0.0")) != null)
                    .Select(t=>new TypeReferenceConfiguration(t))
                    .ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the service endpoint type
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        [DisplayName("Service Type"), Description("The classification of the service (such as HDSI, AMI, IDP, etc.) which this registration represents")]
        public ServiceEndpointType ServiceType { get; set; }

        /// <summary>
        /// Capabilities
        /// </summary>
        [XmlAttribute("cap"), JsonProperty("cap")]
        [DisplayName("Capabilities"), Description("The capabilities of this service (compression, bearer authentication, etc.) which is used by clients to negotiate server capacity")]
        public ServiceEndpointCapabilities Capabilities { get; set; }

        /// <summary>
        /// Base URL type
        /// </summary>
        [XmlAttribute("url"), JsonProperty("url")]
        [DisplayName("Service URL(S)"), Description("The URLs where the remote service described by the service type can be accessed")]
        public string[] BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the contract
        /// </summary>
        [XmlElement("contract"), JsonProperty("contract")]
        [Browsable(false)]
        public TypeReferenceConfiguration[] Contracts { get; set; }

        /// <summary>
        /// Gets or sets the behaviors
        /// </summary>
        [XmlElement("behavior"), JsonProperty("behavior")]
        [DisplayName("Service Behavior"), Description("The service implementation which this remote service implements (which is used to drive metadata collection)")]
        [Editor("SanteDB.Configuration.Editors.TypeSelectorEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing"), Binding("SanteDB.Rest.Common.IRestApiContractImplementation, SanteDB.Rest.Common")]
        public TypeReferenceConfiguration Behavior { get; set; }

        /// <summary>
        /// Remote service string
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Remote {this.ServiceType}";
    }
}