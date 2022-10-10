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
            get => new TypeReferenceConfiguration(this.CredentialProvider.GetType());
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
        /// Gets or sets the certificate validator.
        /// </summary>
        /// <value>The certificate validator.</value>
        [XmlIgnore, JsonIgnore]
        ICertificateValidator IRestClientSecurityDescription.CertificateValidator { get => Activator.CreateInstance(this.CertificateValidatorXml.Type) as ICertificateValidator; }

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
        /// Security mode
        /// </summary>
        /// <value>The mode.</value>
        [XmlAttribute("authScheme"), JsonProperty("authScheme")]
        public SecurityScheme Mode
        {
            get;
            set;
        }

        /// <summary>
        /// Preemptive authentication
        /// </summary>
        [XmlAttribute("preAuth"), JsonProperty("preAuth")]
        public bool PreemptiveAuthentication { get; set; }
    }

}
