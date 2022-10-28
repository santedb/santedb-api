using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represents a class which can read a <see cref="IBackupAsset"/> and restore the necessary file information to the proper 
    /// location
    /// </summary>
    public interface IRestoreBackupAssets
    {

        /// <summary>
        /// True if the application host needs to be restarted after a restore
        /// </summary>
        bool RequiresRestartAfterRestore { get; }

        /// <summary>
        /// The list of asset class identifiers this restore tool can restore
        /// </summary>
        Guid[] AssetClassIdentifiers { get; }

        /// <summary>
        /// Restore the asset to the proper location
        /// </summary>
        /// <param name="backupAsset">The backup asset to be restored</param>
        /// <returns>True if the asset was restored</returns>
        bool Restore(IBackupAsset backupAsset);

    }
}
