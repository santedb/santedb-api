using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represents a backup asset which is derived from a byte array
    /// </summary>
    public class ByteArrayBackupAsset : StreamBackupAsset
    {
        /// <summary>
        /// Create anew byte array backup asset
        /// </summary>
        public ByteArrayBackupAsset(Guid assetClassId, string assetName, byte[] data) : base(assetClassId, assetName, ()=> new MemoryStream(data))
        {
        }
    }
}
