using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Implementation of a <see cref="IBackupDescriptor"/> which derives its information from a file
    /// </summary>
    public class FileBackupDescriptor : IBackupDescriptor
    {
        private readonly DateTime m_timestamp;
        private readonly BackupAssetInfo[] m_assets;
        private readonly string m_createdBy;

        /// <summary>
        /// Creates a backup descriptor from the specified <paramref name="backupFile"/>
        /// </summary>
        /// <param name="backupFile">The file to use as the basis for this backup descriptior</param>
        public FileBackupDescriptor(FileInfo backupFile)
        {
            this.Label = Path.GetFileNameWithoutExtension(backupFile.Name);
            this.Size = backupFile.Length;

            using (var fs = backupFile.OpenRead())
            {
                if(!BackupReader.OpenDescriptor(fs, out this.m_timestamp, out this.m_assets, out this.m_createdBy, out var iv))
                {
                    throw new BackupException(ErrorMessages.INVALID_FILE_FORMAT);
                }

                // The file is encrypted if the IV segment (at the end of the descriptor) is null
                this.IsEnrypted = !iv.All(o => o == 0);

            }
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
