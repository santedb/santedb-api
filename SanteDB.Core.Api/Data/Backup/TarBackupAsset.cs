using SharpCompress.Common;
using System;
using System.IO;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represents a backup asset which is from a backup file
    /// </summary>
    internal class TarBackupAsset : IBackupAsset
    {
        private readonly Stream m_underlyingStream;

        /// <summary>
        /// A new backup asset from a TAR file
        /// </summary>
        public TarBackupAsset(string assetName, Guid assetClassId, Stream entryStream)
        {
            this.Name = assetName;
            this.AssetClassId = assetClassId;
            this.m_underlyingStream = entryStream;
        }

        /// <summary>
        /// Gets the asset class identifier
        /// </summary>
        public Guid AssetClassId { get; }


        /// <summary>
        /// Get the asset name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            this.m_underlyingStream.Dispose();
        }

        /// <summary>
        /// Open the stream
        /// </summary>
        public Stream Open() => this.m_underlyingStream;
    }
}