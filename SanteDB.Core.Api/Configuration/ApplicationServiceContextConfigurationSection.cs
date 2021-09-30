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
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// SanteDB server configuration 
    /// </summary>
    [XmlType(nameof(ApplicationServiceContextConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class ApplicationServiceContextConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// Create new santedb configuration object
        /// </summary>
        public ApplicationServiceContextConfigurationSection()
        {
            this.ServiceProviders = new List<TypeReferenceConfiguration>();
            this.AppSettings = new List<AppSettingKeyValuePair>();
        }

        /// <summary>
        /// Allow unsigned assemblies
        /// </summary>
        [XmlAttribute("allowUnsignedAssemblies"), DisplayName("Allow Unsigned Assemblies"), Description("When true, the application host context will allow unsigned service plugins to operate")]
        public bool AllowUnsignedAssemblies { get; set; }

        /// <summary>
        /// Thread pool size
        /// </summary>
        [XmlAttribute("threadPoolSize"), DisplayName("Thread Pool"), Description("Sets the number of threads to allocate in the default thread pool")]
        public int ThreadPoolSize { get; set; }

        /// <summary>
        /// Gets the service providers from XML
        /// </summary>
        [XmlArray("serviceProviders"), XmlArrayItem("add"), JsonProperty("service"), DisplayName("Service Providers"), Description("The service providers which are enabled on this host instance of SanteDB")]
        [Editor("SanteDB.Configuration.Editors.TypeSelectorEditor, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing"), Binding(typeof(IServiceImplementation))]
        public List<TypeReferenceConfiguration> ServiceProviders { get; set; }

        /// <summary>
        /// General extended application settings
        /// </summary>
        [XmlArray("appSettings"), XmlArrayItem("add"), JsonProperty("setting")]
        [DisplayName("App Settings"), Description("Custom, non-structured application settings")]
        public List<AppSettingKeyValuePair> AppSettings
        {
            get;
            set;
        }

    }


    /// <summary>
    /// Application key/value pair setting
    /// </summary>
    [XmlType(nameof(AppSettingKeyValuePair), Namespace = "http://santedb.org/mobile/configuration")]
    public class AppSettingKeyValuePair
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public AppSettingKeyValuePair()
        {

        }

        /// <summary>
        /// Creates a new key/value pair
        /// </summary>
        public AppSettingKeyValuePair(String key, String value)
        {
            this.Key = key;
            this.Value = value;
        }
        /// <summary>
        /// The key of the setting
        /// </summary>
        [XmlAttribute("key"), JsonProperty("key")]
        public String Key { get; set; }

        /// <summary>
        /// The value of the setting
        /// </summary>
        [XmlAttribute("value"), JsonProperty("value")]
        public String Value { get; set; }

    }
}
