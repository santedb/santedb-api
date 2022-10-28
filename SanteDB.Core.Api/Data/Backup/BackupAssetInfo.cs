using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Backup
{

    /// <summary>
    /// Backup asset metadata
    /// </summary>
    public class BackupAssetInfo
    {

        /// <summary>
        /// Create backup from asset buffer
        /// </summary>
        internal BackupAssetInfo(byte[] assetBuffer)
        {
            this.AssetClassId = new Guid(assetBuffer.Take(16).ToArray());
            this.AssetName = Encoding.UTF8.GetString(assetBuffer, 16, 256).Trim();
        }

        /// <summary>
        /// Create backup from asset info
        /// </summary>
        internal BackupAssetInfo(IBackupAsset asset)
        {
            this.AssetClassId = asset.AssetClassId;
            this.AssetName = asset.Name;
        }

        /// <summary>
        /// Gets the class identifier of the asset
        /// </summary>
        public Guid AssetClassId { get; set; }

        /// <summary>
        /// Gets the name of the asset
        /// </summary>
        public String AssetName { get; set; }

        /// <summary>
        /// Convert to an entry array for the backup file
        /// </summary>
        internal byte[] ToByteArray()
        {
            var retVal = Enumerable.Range(0, 272).Select(o => (byte)' ').ToArray();
            Array.Copy(this.AssetClassId.ToByteArray(), 0, retVal, 0, 16);
            var nameBytes = Encoding.UTF8.GetBytes(this.AssetName).Take(256).ToArray();
            Array.Copy(nameBytes, 0, retVal, 16, nameBytes.Length);
            return retVal;
        }
    }
}
