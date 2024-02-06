using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represents a single description for  a single backup taken on this machine
    /// </summary>
    public interface IBackupDescriptor
    {

        /// <summary>
        /// Gets the label for the backup
        /// </summary>
        String Label { get; }

        /// <summary>
        /// Gets the timestamp 
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets the user that created the backup
        /// </summary>
        String CreatedBy { get; }

        /// <summary>
        /// Gets the size of the backup
        /// </summary>
        long Size { get; }

        /// <summary>
        /// True if the backup is encrypted
        /// </summary>
        bool IsEnrypted { get; }

        /// <summary>
        /// Gets the descriptors of the assets in this backup
        /// </summary>
        IEnumerable<IBackupAssetDescriptor> Assets { get; }
    }
}
