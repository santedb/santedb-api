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
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// The default backup manager
    /// </summary>
    public class DefaultBackupManager : IBackupService, IReportProgressChanged, IRequestRestarts
    {
        // Backup configuration section
        private readonly BackupConfigurationSection m_configuration;
        private readonly bool m_allowPublicBackups;
        private readonly IServiceManager m_serviceManager;
        private readonly IPolicyEnforcementService m_pepService;
        private ILocalizationService m_localizationService;
        private readonly IPlatformSecurityProvider m_platformSecurity;
        private IDictionary<Guid, IRestoreBackupAssets> m_backupAssetClasses;

        public const string BACKUP_EXTENSION = "bin";
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultBackupManager));

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public event EventHandler RestartRequested;

        /// <summary>
        /// Default backup manager DI constructor
        /// </summary>
        public DefaultBackupManager(IConfigurationManager configurationManager,
            IServiceManager serviceManager,
            IPolicyEnforcementService pepService,
            ILocalizationService localizationService,
            IPlatformSecurityProvider platformSecurityProvider)
        {
            this.m_configuration = configurationManager.GetSection<BackupConfigurationSection>();
            this.m_allowPublicBackups = configurationManager.GetSection<SecurityConfigurationSection>().GetSecurityPolicy(SecurityPolicyIdentification.AllowPublicBackups, false);
            this.m_serviceManager = serviceManager;
            this.m_pepService = pepService;
            this.m_localizationService = localizationService;
            this.m_platformSecurity = platformSecurityProvider;
            this.m_backupAssetClasses = new Dictionary<Guid, IRestoreBackupAssets>();

        }

        /// <summary>
        /// Allow public backups
        /// </summary>
        protected bool AllowPublicBackups => this.m_allowPublicBackups;

        /// <summary>
        /// Configuration
        /// </summary>
        protected BackupConfigurationSection Configuration => this.m_configuration;

        /// <summary>
        /// Get backup classes
        /// </summary>
        private IDictionary<Guid, IRestoreBackupAssets> GetBackupRestoreServices()
        {
            lock (this.m_backupAssetClasses)
            {
                if (this.m_backupAssetClasses?.Any() == false)
                {
                    this.m_backupAssetClasses = new Dictionary<Guid, IRestoreBackupAssets>();
                    this.m_serviceManager.GetServices()
                        .OfType<IRestoreBackupAssets>()
                        .ToList()
                        .ForEach(irba => irba.AssetClassIdentifiers.ToList().ForEach(c => this.m_backupAssetClasses.Add(c, irba)));
                    // Now fetch the backup services that in the context for restoration which may not be in our configured
                    this.m_serviceManager.CreateInjectedOfAll<IRestoreBackupAssets>()
                        .ForEach(irba =>
                            irba.AssetClassIdentifiers.Where(c => !this.m_backupAssetClasses.ContainsKey(c)).ForEach(a => this.m_backupAssetClasses.Add(a, irba))
                        );


                }
                return this.m_backupAssetClasses;
            }
        }

        /// <inheritdoc/>
        public string ServiceName => "Default Backup Manager";

        /// <inheritdoc/>
        public virtual IBackupDescriptor Backup(BackupMedia media, string password = null)
        {
            if (this.m_configuration.RequireEncryptedBackups && String.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_POLICY_REQUIRES_ENCRYPTION));
            }
            else if ((media == BackupMedia.ExternalPublic || media == BackupMedia.Public) && (!this.m_allowPublicBackups || !this.m_platformSecurity.DemandPlatformServicePermission(PlatformServicePermission.ExternalMedia)))
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.POLICY_PREVENTS_ACTION, SecurityPolicyIdentification.AllowPublicBackups));
            }

            // Set file and output
            String backupDescriptorLabel = DateTime.Now.Ticks.ToString();
            if (!this.m_configuration.TryGetBackupPath(media, out var backupPath))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }
            else if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            backupPath = Path.Combine(backupPath, Path.ChangeExtension(backupDescriptorLabel, BACKUP_EXTENSION));

            if (media == BackupMedia.Private)
            {
                this.m_pepService.Demand(PermissionPolicyIdentifiers.CreatePrivateBackup);
            }
            else
            {
                this.m_pepService.Demand(PermissionPolicyIdentifiers.CreateAnyBackup);
            }

            return this.BackupToFile(backupPath, password);
        }


        /// <summary>
        /// Backup the system to a specified file
        /// </summary>
        public IBackupDescriptor BackupToFile(string backupPath, string password)
        {
            try
            {

                this.m_tracer.TraceInfo("Will perform backup to {0}...", backupPath);

                this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(nameof(DefaultBackupManager), 0f, this.m_localizationService.GetString(UserMessageStrings.BACKUP_CREATE_PROGRESS)));


                using (var fs = File.Create(backupPath))
                {
                    this.BackupToStream(fs, password);
                }

                this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(nameof(DefaultBackupManager), 1f, this.m_localizationService.GetString(UserMessageStrings.BACKUP_CREATE_PROGRESS)));


                return new FileBackupDescriptor(new FileInfo(backupPath));
            }
            catch (Exception ex)
            {
                throw new BackupException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_GEN_ERR), ex);
            }
        }

        /// <summary>
        /// Backup to the stream
        /// </summary>
        public void BackupToStream(Stream stream, string password)
        {
            try
            {
                var assets = this.m_serviceManager.GetServices()
                        .OfType<IProvideBackupAssets>()
                        .Distinct()
                        .SelectMany(o => o.GetBackupAssets())
                        .ToArray();

                using (var bw = BackupWriter.Create(stream, assets, password: password, keepOpen: true))
                {
                    for (var i = 0; i < assets.Length; i++)
                    {
                        this.m_tracer.TraceInfo("Adding {0} to backup", assets[i].Name);
                        this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(nameof(DefaultBackupManager), (float)i / assets.Length, this.m_localizationService.GetString(UserMessageStrings.BACKUP_CREATE_PROGRESS)));
                        bw.WriteAssetEntry(assets[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new BackupException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_GEN_ERR), ex);
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IBackupDescriptor> GetBackupDescriptors(BackupMedia media)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ManageBackups);
            if (!this.m_configuration.TryGetBackupPath(media, out var backupPath))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }
            else if (media == BackupMedia.Private || this.m_platformSecurity.DemandPlatformServicePermission(PlatformServicePermission.ExternalMedia))
            {
                this.m_tracer.TraceInfo("Getting backup descriptors for {0} ({1})", media, backupPath);
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                return Directory.EnumerateFiles(backupPath, $"*.{BACKUP_EXTENSION}").Where(f => !f.StartsWith(".")).Select(o => new FileBackupDescriptor(new FileInfo(o)));
            }
            else
            {
                this.m_tracer.TraceWarning("Application does not have permission");
                return new IBackupDescriptor[0];
            }
        }

        /// <inheritdoc/>
        public virtual IBackupDescriptor GetBackup(BackupMedia media, String backupDescriptorLabel)
        {
            var retVal = this.GetBackupInternal(media, backupDescriptorLabel);
            if (retVal == null)
            {
                throw new KeyNotFoundException(backupDescriptorLabel);
            }
            else
            {
                return retVal;
            }
        }

        /// <inheritdoc/>
        public virtual IBackupDescriptor GetBackup(String backupDescriptorLabel, out BackupMedia locatedOnMedia)
        {
            foreach (var bm in new[] { BackupMedia.Private, BackupMedia.Public, BackupMedia.ExternalPublic })
            {
                var backup = this.GetBackupInternal(bm, backupDescriptorLabel);
                if (backup != null)
                {
                    locatedOnMedia = bm;
                    return backup;
                }
            }
            locatedOnMedia = default(BackupMedia);
            return null;
        }

        /// <summary>
        /// Get backup file descriptor
        /// </summary>
        public virtual IBackupDescriptor GetBackupInternal(BackupMedia media, String backupDescriptorLabel)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ManageBackups);

            if (!this.m_configuration.TryGetBackupPath(media, out var backupPath))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }
            else if (media != BackupMedia.Private && !this.m_platformSecurity.DemandPlatformServicePermission(PlatformServicePermission.ExternalMedia))
            {
                throw new BackupException(ErrorMessages.PLATFORM_SECURITY_ERROR);
            }

            var backupFile = Path.Combine(backupPath, Path.ChangeExtension(backupDescriptorLabel, BACKUP_EXTENSION));
            if (!File.Exists(backupFile))
            {
                return null;
            }
            else
            {
                return new FileBackupDescriptor(new FileInfo(backupFile));
            }

        }

        /// <inheritdoc/>
        public virtual bool HasBackup(BackupMedia media) => this.GetBackupDescriptors(media).Any();

        /// <inheritdoc/>
        public virtual void RemoveBackup(BackupMedia media, string backupDescriptorLabel)
        {
            if (String.IsNullOrEmpty(backupDescriptorLabel))
            {
                throw new ArgumentNullException(nameof(backupDescriptorLabel));
            }

            this.m_pepService.Demand(PermissionPolicyIdentifiers.ManageBackups);

            if (!this.m_configuration.TryGetBackupPath(media, out var backupPath))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }
            else if (media != BackupMedia.Private && !this.m_platformSecurity.DemandPlatformServicePermission(PlatformServicePermission.ExternalMedia))
            {
                throw new BackupException(ErrorMessages.PLATFORM_SECURITY_ERROR);
            }

            try
            {
                File.Delete(Path.Combine(backupPath, Path.ChangeExtension(backupDescriptorLabel, BACKUP_EXTENSION)));
            }
            catch (Exception e)
            {
                throw new BackupException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_GEN_ERR), e);

            }

        }

        /// <inheritdoc/>
        public virtual bool Restore(BackupMedia media, string backupDescriptorLabel, string password = null)
        {
            if (String.IsNullOrEmpty(backupDescriptorLabel))
            {
                throw new ArgumentNullException(nameof(backupDescriptorLabel));
            }

            this.m_pepService.Demand(PermissionPolicyIdentifiers.ManageBackups);

            if (!this.m_configuration.TryGetBackupPath(media, out var backupFile))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }
            else if (media != BackupMedia.Private && !this.m_platformSecurity.DemandPlatformServicePermission(PlatformServicePermission.ExternalMedia))
            {
                throw new BackupException(ErrorMessages.PLATFORM_SECURITY_ERROR);
            }

            backupFile = Path.Combine(backupFile, Path.ChangeExtension(backupDescriptorLabel, BACKUP_EXTENSION));
            if (!File.Exists(backupFile))
            {
                throw new FileNotFoundException(backupFile);
            }


            return this.RestoreFromFile(backupFile, password);
        }

        /// <summary>
        /// Get backup classes
        /// </summary>
        public IDictionary<Guid, Type> GetBackupAssetClasses() => this.GetBackupRestoreServices().ToDictionary(o => o.Key, o => o.Value.GetType());

        /// <inheritdoc/>
        public IBackupDescriptor GetBackupDescriptorFromFile(string backupFile)
        {
            try
            {
                return new FileBackupDescriptor(new FileInfo(backupFile));
            }
            catch (Exception e)
            {
                throw new BackupException(ErrorMessages.BACKUP_DESCRIPTOR_ERROR, e);
            }
        }

        /// <inheritdoc/>
        public IBackupDescriptor GetBackupDescriptorFromStream(Stream backupStream)
        {
            try
            {
                return new StreamBackupDescriptor(backupStream);
            }
            catch (Exception e)
            {
                throw new BackupException(ErrorMessages.BACKUP_DESCRIPTOR_ERROR, e);
            }
        }

        /// <inheritdoc/>
        public bool RestoreFromFile(string backupFile, string password)
        {
            try
            {
                this.m_tracer.TraceInfo("Restoring {0}...", backupFile);
                using (var fs = File.OpenRead(backupFile))
                {
                    return this.RestoreFromStream(fs, password);
                }
            }
            catch (Exception e)
            {
                throw new BackupException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_RESTORE_ERR), e);
            }
        }

        /// <summary>
        /// Restore from a stream source
        /// </summary>
        public bool RestoreFromStream(Stream stream, string password)
        {
            try
            {
                int i = 0;
                using (AuthenticationContext.EnterSystemContext())
                {
                    using (var br = BackupReader.Open(stream, password))
                    {
                        while (br.GetNextEntry(out var backupAsset))
                        {
                            this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(nameof(DefaultBackupManager), ((float)i++) / br.BackupAsset.Length, this.m_localizationService.GetString(UserMessageStrings.BACKUP_RESTORE_PROGRESS)));
                            using (backupAsset)
                            {
                                if (this.GetBackupRestoreServices().TryGetValue(backupAsset.AssetClassId, out var restoreProvider))
                                {
                                    this.m_tracer.TraceInfo("Restoring {0}...", backupAsset.Name);
                                    restoreProvider.Restore(backupAsset);
                                }
                                else
                                {
                                    this.m_tracer.TraceWarning("{0} cannot be restored as the asset class {1} is unknown", backupAsset.Name, backupAsset.AssetClassId);
                                }
                            }
                        }
                    }
                }
                this.RestartRequested?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception e)
            {
                throw new BackupException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_RESTORE_ERR), e);
            }
        }
    }
}
