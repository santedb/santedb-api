/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-8-8
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        /// Thread pool size
        /// </summary>
        [XmlAttribute("threadPoolSize")]
        public int ThreadPoolSize { get; set; }

        /// <summary>
        /// Gets the service providers from XML
        /// </summary>
        [XmlArray("serviceProviders"), XmlArrayItem("add"), JsonProperty("service")]
        public List<TypeReferenceConfiguration> ServiceProviders { get; set; }


        /// <summary>
        /// General extended application settings
        /// </summary>
        [XmlArray("appSettings"), XmlArrayItem("add"), JsonProperty("setting")]
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
