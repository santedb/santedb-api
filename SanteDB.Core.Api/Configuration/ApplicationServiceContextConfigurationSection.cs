/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// SanteDB server configuration 
    /// </summary>
    [XmlType(nameof(ApplicationServiceContextConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class ApplicationServiceContextConfigurationSection : IValidatableConfigurationSection
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

        /// <summary>
        /// Gets or sets the instance name
        /// </summary>
        [XmlAttribute("instanceName"), JsonProperty("instanceName")]
        [DisplayName("Instance Name"), Description("A human readable identifier for this instance (if running multiples on the same system)")]
        public string InstanceName { get; set; }

        /// <summary>
        /// Validate the configuration section
        /// </summary>
        public IEnumerable<DetectedIssue> Validate()
        {
            foreach (var itm in this.ServiceProviders)
            {
                if (!itm.IsValid())
                {
                    // Is there a new type?
                    var tName = itm.TypeXml.Split(',')[0].Split('.').Last();
                    var candidateType = AppDomain.CurrentDomain.GetAllTypes().FirstOrDefault(t => t.Name == tName);
                    if (candidateType != null)
                    {
                        yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "movedType", $"Type {itm.TypeXml} has been moved to {candidateType.FullName},{candidateType.Assembly.GetName().Name}", Guid.Empty);
                        itm.Type = candidateType;
                    }
                    else
                    {
                        yield return new DetectedIssue(DetectedIssuePriorityType.Error, "missingtype", $"Type {itm.TypeXml} is not valid", Guid.Empty);
                    }
                }
            }

            if (this.ThreadPoolSize > Environment.ProcessorCount)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "resources", $"Max thread pool will have {this.ThreadPoolSize} but machine only has {Environment.ProcessorCount}", Guid.Empty);
            }

            foreach (var itm in this.AppSettings)
            {
                if (String.IsNullOrEmpty(itm.Key) || String.IsNullOrEmpty(itm.Value))
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "appsetting", $"App setting {itm.Key}={itm.Value} is missing key or value", Guid.Empty);
                }
            }
        }
    }


    /// <summary>
    /// Application key/value pair setting
    /// </summary>
    [XmlType(nameof(AppSettingKeyValuePair), Namespace = "http://santedb.org/configuration")]
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
