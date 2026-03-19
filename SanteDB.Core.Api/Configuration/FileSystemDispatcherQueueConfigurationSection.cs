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
 * Date: 2023-6-21
 */
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a configuration section for file system queueing
    /// </summary>
    [XmlType(nameof(FileSystemDispatcherQueueConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class FileSystemDispatcherQueueConfigurationSection : IConfigurationSection, IValidatableConfigurationSection
    {
        /// <summary>
        /// Gets or sets the path to the queue location
        /// </summary>
        [XmlAttribute("queueRoot")]
        [Description("Identifies where file system queues should be created")]
        [Editor("System.Windows.Forms.Design.FolderNameEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        public String QueuePath { get; set; }

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Validate()
        {
            if (!Directory.Exists(this.QueuePath))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "config.err.path", $"Queue path {this.QueuePath} does not exist", Guid.Empty);
            }
            else if(!Path.IsPathRooted(this.QueuePath))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "config.warn.root", $"Queue path {this.QueuePath} should be rooted", Guid.Empty);
            }

            // Warn about using this in prod
            yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "config.patterns.prod", $"The file system dispatcher is intended for debug and evaluation deployments only - Use a more robust solution in production", Guid.Empty);
        }
    }
}