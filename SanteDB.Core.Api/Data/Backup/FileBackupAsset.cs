using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represents a <see cref="IBackupAsset"/> which is generated based on the contents of a file
    /// </summary>
    public class FileBackupAsset : StreamBackupAsset
    {

        /// <summary>
        /// Creates a new backup asset for a file
        /// </summary>
        public FileBackupAsset(Guid assetClassId, String name, String filePath)
            : base(assetClassId, name, ()=> File.OpenRead(filePath))
        {
        }

    }
}
