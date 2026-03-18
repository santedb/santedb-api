/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-12-12
 */
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// File system dataset provider configuration section
    /// </summary>
    [XmlType(nameof(FileSystemDatasetProviderConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class FileSystemDatasetProviderConfigurationSection : IConfigurationSection, IValidatableConfigurationSection
    {


        /// <summary>
        /// Gets or sets the sources of the dataset processing
        /// </summary>
        [XmlArray("sources"), XmlArrayItem("add"), JsonProperty("sources")]
        public List<String> Sources { get; set; }

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Validate()
        {
            foreach (var path in this.Sources.Select(o => o.ToLower()))
            {
                if (!Directory.Exists(path)) // HACK: Might be on linux or have a lower case data file
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "err.config.folder", $"Folder {path} doesn't exist", Guid.Empty);
                }
                else if (!Path.IsPathRooted(path))
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "err.config.abs", $"Folder path {path} should be absolute", Guid.Empty);
                }
                if (!Directory.EnumerateFiles(path, "*.dataset").Any())
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "err.config.nods", $"Folder path {path} does not contain any datasets", Guid.Empty);
                }
            }
        }
    }
}
