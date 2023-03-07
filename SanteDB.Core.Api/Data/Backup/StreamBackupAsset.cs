using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// A stream based backup asset
    /// </summary>
    public class StreamBackupAsset : IBackupAsset
    {

        private Stream m_stream;
        private readonly Func<Stream> m_getStreamFunc;

        /// <summary>
        /// Create a new stream backup asset
        /// </summary>
        public StreamBackupAsset(Guid assetClassId, String assetName, Func<Stream> getStreamFunc)
        {
            this.AssetClassId = assetClassId;
            this.Name = assetName;
            this.m_getStreamFunc = getStreamFunc;
        }

        /// <inheritdoc/>
        public Guid AssetClassId { get; }

        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        public void Dispose()
        {
            if(this.m_stream != null)
            {
                this.m_stream.Dispose();
                this.m_stream = null;
            }
        }

        /// <summary>
        /// Open or return the stream
        /// </summary>
        public Stream Open()
        {
            if(this.m_stream == null)
            {
                this.m_stream = this.m_getStreamFunc();
            }
            return this.m_stream;
        }
    }
}
