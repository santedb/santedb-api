﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a configuration for MDM
    /// </summary>
    [XmlType(nameof(ResourceManagementConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class ResourceManagementConfigurationSection : IConfigurationSection, IDisclosedConfigurationSection
    {

        /// <summary>
        /// Resources managed under RM
        /// </summary>
        public const string ResourcesUnderRMSetting = "managedResource";

        /// <summary>
        /// MDM configuration
        /// </summary>
        public ResourceManagementConfigurationSection()
        {
            this.ResourceTypes = new List<ResourceTypeReferenceConfiguration>();
        }

        /// <summary>
        /// Gets or sets the resource types
        /// </summary>
        [XmlArray("resources"), XmlArrayItem("add"), JsonProperty("resources")]
        [Editor("SanteDB.Configuration.Editors.ResourceTypePicker, SanteDB.Configuration", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public List<ResourceTypeReferenceConfiguration> ResourceTypes { get; set; }

        /// <summary>
        /// Gets or sets the master data deletion mode for old data
        /// </summary>
        [XmlElement("oldMasterRetention"), Description("Specifies the retention mode for old master relationship data which is not needed"), DisplayName("Master Data Retention")]
        public DeleteMode MasterDataDeletionMode { get; set; }

        /// <inheritdoc/>
        public IEnumerable<AppSettingKeyValuePair> ForDisclosure() => this.ResourceTypes?.Select(o => new AppSettingKeyValuePair(ResourcesUnderRMSetting, o.TypeXml));

        /// <inheritdoc/>
        public void Injest(IEnumerable<AppSettingKeyValuePair> remoteSettings)
        {
            this.ResourceTypes = remoteSettings.Where(o => o.Key == ResourcesUnderRMSetting).Select(o => new ResourceTypeReferenceConfiguration(o.Value)).ToList();
        }
    }
}