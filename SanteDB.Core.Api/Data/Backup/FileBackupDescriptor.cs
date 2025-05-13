/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2024-6-21
 */
using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                if (!BackupReader.OpenDescriptor(fs, out this.m_timestamp, out this.m_assets, out this.m_createdBy, out var iv))
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
