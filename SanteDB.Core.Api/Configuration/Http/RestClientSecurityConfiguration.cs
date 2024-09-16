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
using SanteDB.Core.Http.Description;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Security.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Http
{

    /// <summary>
    /// Service client security configuration
    /// </summary>
    [XmlType(nameof(RestClientSecurityConfiguration), Namespace = "http://santedb.org/configuration")]
    public class RestClientSecurityConfiguration : IRestClientSecurityDescription
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public RestClientSecurityConfiguration()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        public RestClientSecurityConfiguration(RestClientSecurityConfiguration copyFrom)
        {
            this.AuthRealm = copyFrom.AuthRealm;
            this.CertificateValidatorXml = copyFrom.CertificateValidatorXml;
            this.ClientCertificate = copyFrom.ClientCertificate;
            this.CredentialProvider = copyFrom.CredentialProvider;
            this.CredentialProviderConfiguration = copyFrom.CredentialProviderConfiguration;
            this.Mode = copyFrom.Mode;
            this.PreemptiveAuthentication = copyFrom.PreemptiveAuthentication;
        }

        /// <summary>
        /// Gets or sets the ICertificateValidator interface which should be called to validate
        /// certificates
        /// </summary>
        /// <value>The serialization binder type xml.</value>
        [XmlElement("certificateValidator"), JsonProperty("certificateValidator")]
        [Editor("SanteDB.Configuration.Editors.TypeSelectorEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing"), Binding(typeof(ICertificateValidator))]
        public TypeReferenceConfiguration CertificateValidatorXml
        {
            get; set;
        }

        /// <summary>
        /// Gets the thumbprint the device should use for authentication
        /// </summary>
        [XmlElement("clientCertificate"), JsonProperty("clientCertificate")]
        public X509ConfigurationElement ClientCertificate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ICredentialProvider
        /// </summary>
        /// <value>The credential provider xml.</value>
        [XmlElement("credentialProvider"), JsonProperty("credentialProvider")]
        [Editor("SanteDB.Configuration.Editors.TypeSelectorEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing"), Binding(typeof(ICredentialProvider))]
        public TypeReferenceConfiguration CredentialProviderXml
        {
            get => new TypeReferenceConfiguration(this.CredentialProvider?.GetType());
            set { this.CredentialProvider = Activator.CreateInstance(value.Type) as ICredentialProvider; }
        }

        /// <summary>
        /// Credential provider configuration
        /// </summary>
        [XmlArray("credentialSettings"), XmlArrayItem("add"), JsonProperty("credentialSettings")]
        public List<RestSecurityCredentialSettings> CredentialProviderConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the authentication realm this client should verify
        /// </summary>
        /// <value>The auth realm.</value>
        [XmlAttribute("authRealm"), JsonProperty("authRealm")]
        public string AuthRealm
        {
            get;
            set;
        }

        /// <summary>
        /// Gets certificate find
        /// </summary>
        X509Certificate2 IRestClientSecurityDescription.ClientCertificate => this.ClientCertificate?.Certificate;

        /// <summary>
        /// Credential provider parameters
        /// </summary>
        IDictionary<String, String> IRestClientSecurityDescription.CredentialProviderParameters { get => this.CredentialProviderConfiguration.ToDictionary(o => o.Name, o => o.Value); }

        /// <summary>
        /// Gets or sets the credential provider.
        /// </summary>
        /// <value>The credential provider.</value>
        [XmlIgnore, JsonIgnore]
        public ICredentialProvider CredentialProvider { get; set; }

        /// <summary>
        /// Preemptive authentication
        /// </summary>
        [XmlAttribute("preAuth"), JsonProperty("preAuth")]
        public bool PreemptiveAuthentication { get; set; }

        /// <summary>
        /// Gets the mode of authentication
        /// </summary>
        [XmlAttribute("mode"), JsonProperty("mode")]
        public SecurityScheme Mode { get; set; }
    }

}
