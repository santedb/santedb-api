using Newtonsoft.Json;
using SanteDB.Core.Data.Backup;
using System;
using System.Collections.Generic;
using System.Text;
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
            switch(media)
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
