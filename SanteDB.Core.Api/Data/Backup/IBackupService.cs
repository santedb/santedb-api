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
 * Date: 2023-6-21
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Backup
{

    /// <summary>
    /// Represents a service that can back-up data to/from another location
    /// </summary>
    public interface IBackupService : IServiceImplementation, IRequestRestarts
    {

        /// <summary>
        /// Backup media
        /// </summary>
        /// <param name="media">Where the backup should be stored</param>
        /// <param name="password">The password to protect the backup</param>
        /// <returns>The backup descriptor</returns>
        IBackupDescriptor Backup(BackupMedia media, string password = null);

        /// <summary>
        /// Restore from media
        /// </summary>
        /// <param name="backupDescriptorLabel">The identifier of the backup set to restore</param>
        /// <param name="media">The media from which the restore should occur</param>
        /// <param name="password">The password for the restore</param>
        /// <returns>True if the restore was successful</returns>
        /// <remarks>If the restore is successful, the caller should make the determination of whether to terminate the 
        /// current application environment and restart the application host</remarks>
        bool Restore(BackupMedia media, string backupDescriptorLabel, string password = null);

        /// <summary>
        /// Has backup on the specified media
        /// </summary>
        bool HasBackup(BackupMedia media);

        /// <summary>
        /// Gets the backup descriptors for the specified media
        /// </summary>
        IEnumerable<IBackupDescriptor> GetBackupDescriptors(BackupMedia media);

        /// <summary>
        /// Get all registered backup asset classes
        /// </summary>
        IDictionary<Guid, Type> GetBackupAssetClasses();

        /// <summary>
        /// Remove a backup
        /// </summary>
        void RemoveBackup(BackupMedia media, string backupDescriptorLabel);

        /// <summary>
        /// Get the descriptor for a specific backup
        /// </summary>
        /// <param name="media">The media on which the backup is located</param>
        /// <param name="backupDescriptorLabel">The descirptor label</param>
        /// <returns>The backup descriptor</returns>
        IBackupDescriptor GetBackup(BackupMedia media, string backupDescriptorLabel);

        /// <summary>
        /// Get the backup with the specified <paramref name="backupDescriptorLabel"/> from any backup source
        /// </summary>
        /// <param name="backupDescriptorLabel">The descriptor label of the backup to load</param>
        /// <param name="locatedOnMedia">The location of the media</param>
        /// <returns>The retrieved backup or null if not found</returns>
        IBackupDescriptor GetBackup(string backupDescriptorLabel, out BackupMedia locatedOnMedia);
        
        /// <summary>
        /// Get a backup descriptor from an absolute file
        /// </summary>
        /// <param name="backupFile">The backup file</param>
        /// <returns>The backup descriptor</returns>
        IBackupDescriptor GetBackupDescriptorFromFile(string backupFile);

        /// <summary>
        /// Restore a backup from afile
        /// </summary>
        /// <param name="backupFile">The backup file to restore</param>
        /// <param name="password">The password for the backup file</param>
        bool RestoreFromFile(string backupFile, String password);
    }
}
