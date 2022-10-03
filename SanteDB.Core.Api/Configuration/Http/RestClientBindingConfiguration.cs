using Newtonsoft.Json;
using SanteDB.Core.Http;
using SanteDB.Core.Http.Description;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration.Http
{

    /// <summary>
    /// Service client binding
    /// </summary>
    [XmlType(nameof(RestClientBindingConfiguration), Namespace = "http://santedb.org/configuration")]
    public class RestClientBindingConfiguration : IRestClientBindingDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientBindingConfiguration"/> class.
        /// </summary>
        public RestClientBindingConfiguration()
        {
            this.ContentTypeMapper = new DefaultContentTypeMapper();
        }

        /// <summary>
        /// Gets or sets the type which dictates how a body maps to a
        /// </summary>
        /// <value>The serialization binder type xml.</value>
        [XmlElement("contentTypeMapper"), JsonProperty("contentTypeMapper")]
        public TypeReferenceConfiguration ContentTypeMapperXml
        {
            get => new TypeReferenceConfiguration(this.ContentTypeMapper.GetType());
            set { this.ContentTypeMapper = Activator.CreateInstance(value.Type) as IContentTypeMapper; }
        }

        /// <summary>
        /// Gets or sets the security configuration
        /// </summary>
        /// <value>The security.</value>
        [XmlElement("security"), JsonProperty("security")]
        public RestClientSecurityConfiguration Security
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RestClientBindingConfiguration"/>
        /// is optimized
        /// </summary>
        /// <value><c>true</c> if optimize; otherwise, <c>false</c>.</value>
        [XmlElement("optimize"), JsonProperty("optimize")]
        public bool Optimize
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the optimization method
        /// </summary>
        [XmlElement("compressionScheme"), JsonProperty("compressionScheme")]
        public HttpOptimizationMethod OptimizationMethod { get; set; }

        /// <summary>
        /// Content type mapper
        /// </summary>
        /// <value>The content type mapper.</value>
        [XmlIgnore, JsonIgnore]
        public IContentTypeMapper ContentTypeMapper
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the security description
        /// </summary>
        IRestClientSecurityDescription IRestClientBindingDescription.Security => this.Security;
    }

}
