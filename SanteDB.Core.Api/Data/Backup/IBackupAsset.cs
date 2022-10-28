using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represents a single asset (file, database, etc.) which can be backed up
    /// </summary>
    public interface IBackupAsset : IDisposable
    {

        /// <summary>
        /// Gets the asset type identifier used for restoring 
        /// </summary>
        Guid AssetClassId { get; }

        /// <summary>
        /// Get the name of the asset
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Open the backup stream 
        /// </summary>
        /// <remarks>Implementers of this interface should ensure that on an Open() the file 
        /// or backing source is frozen - so that changes from other threads cannot be written</remarks>
        /// <returns>A stream containing the data to be backed up</returns>
        Stream Open();

    }

}
