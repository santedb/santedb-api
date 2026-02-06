using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Stream based backup descriptor
    /// </summary>
    public class StreamBackupDescriptor : IBackupDescriptor
    {
        private readonly DateTime m_timestamp;
        private readonly BackupAssetInfo[] m_assets;
        private readonly string m_createdBy;

        /// <summary>
        /// Creates a backup descriptor from the specified <paramref name="backupStream"/>
        /// </summary>
        /// <param name="backupStream">The stream to use as the basis for this backup descriptior</param>
        public StreamBackupDescriptor(Stream backupStream)
        {
            if (backupStream == null)
            {
                throw new ArgumentNullException(nameof(backupStream));
            }
            else if (!backupStream.CanSeek)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, backupStream.GetType().Name, "CanSeek"));
            }

            this.Label = backupStream.GetType().Name;
            this.Size = backupStream.Length;
            if (!BackupReader.OpenDescriptor(backupStream, out this.m_timestamp, out this.m_assets, out this.m_createdBy, out var iv))
            {
                throw new BackupException(ErrorMessages.INVALID_FILE_FORMAT);
            }

            // The file is encrypted if the IV segment (at the end of the descriptor) is null
            this.IsEnrypted = !iv.All(o => o == 0);
            backupStream.Seek(0, SeekOrigin.Begin);
        }

        /// <inheritdoc/>
        public string Label { get; }

        /// <inheritdoc/>
        public String CreatedBy => this.m_createdBy;

        /// <inheritdoc/>
        public DateTime Timestamp => this.m_timestamp;

        /// <inheritdoc/>
        public long Size { get; }

        /// <inheritdoc/>
        public bool IsEnrypted { get; }

        /// <inheritdoc/>
        public IEnumerable<IBackupAssetDescriptor> Assets => this.m_assets;
    }
}
