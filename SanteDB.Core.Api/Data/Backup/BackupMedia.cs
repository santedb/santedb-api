using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Gets or sets the backup media
    /// </summary>
    public enum BackupMedia
    {
        /// <summary>
        /// The backup should be made to the configured external publicly available location
        /// </summary>
        ExternalPublic,
        /// <summary>
        /// The backup should be made to the configured public location
        /// </summary>
        Public,
        /// <summary>
        /// The backup should be made to the configured internal private location
        /// </summary>
        Private
    }

}
