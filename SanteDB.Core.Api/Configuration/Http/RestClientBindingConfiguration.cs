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
        [XmlElement("optimizeRequests"), JsonProperty("optimizeRequests")]
        public bool CompressRequests
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the optimization method
        /// </summary>
        [XmlElement("compressionScheme"), JsonProperty("compressionScheme")]
        public HttpCompressionAlgorithm OptimizationMethod { get; set; }

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
