using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Any service which provides backup files to the <see cref="IBackupService"/>
    /// </summary>
    public interface IProvideBackupAssets
    {

        /// <summary>
        /// Get the backup asset
        /// </summary>
        /// <returns>A collection of assets which can be backed up</returns>
        IEnumerable<IBackupAsset> GetBackupAssets();

    }
}
