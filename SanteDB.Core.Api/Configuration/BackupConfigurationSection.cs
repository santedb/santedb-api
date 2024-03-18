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
 * User: fyfej
 * Date: 2023-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Data.Backup;
using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a configuration section that sets the backup locations and prefrences
    /// </summary>
    [XmlType(nameof(BackupConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class BackupConfigurationSection : IConfigurationSection
    {

        /// <summary>
        /// True if backups must be encrypted
        /// </summary>
        [XmlAttribute("requireEncryption"), JsonProperty("requireEncryption")]
        public bool RequireEncryptedBackups { get; set; }

        /// <summary>
        /// Location where private backups should be stored
        /// </summary>
        [XmlElement("private"), JsonProperty("private")]
        public String PrivateBackupLocation { get; set; }

        /// <summary>
        /// Public backup location
        /// </summary>
        [XmlElement("public"), JsonProperty("public")]
        public String PublicBackupLocation { get; set; }

        /// <summary>
        /// External backup location
        /// </summary>
        [XmlElement("extern"), JsonProperty("extern")]
        public String ExternalBackupLocation { get; set; }

        /// <summary>
        /// Try to get the specified backup path
        /// </summary>
        /// <param name="media">The media to attempt to retrieve</param>
        /// <param name="backupPath">The configured backup path</param>
        /// <returns>True if the backup path is configured</returns>
        internal bool TryGetBackupPath(BackupMedia media, out string backupPath)
        {
            switch (media)
            {
                case BackupMedia.Private:
                    backupPath = this.PrivateBackupLocation;
                    break;
                case BackupMedia.Public:
                    backupPath = this.PublicBackupLocation;
                    break;
                case BackupMedia.ExternalPublic:
                    backupPath = this.ExternalBackupLocation;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return !String.IsNullOrEmpty(backupPath);
        }
    }
}
