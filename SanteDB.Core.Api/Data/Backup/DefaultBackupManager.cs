using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// The default backup manager
    /// </summary>
    public class DefaultBackupManager : IBackupService, IReportProgressChanged, IRequestRestarts
    {
        // Backup configuration section
        private readonly BackupConfigurationSection m_configuration;
        private readonly IServiceManager m_serviceManager;
        private readonly IPolicyEnforcementService m_pepService;
        private ILocalizationService m_localizationService;

        private const string BACKUP_EXTENSION = "sdbk.bz2";
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
            ILocalizationService localizationService)
        {
            this.m_configuration = configurationManager.GetSection<BackupConfigurationSection>();
            this.m_serviceManager = serviceManager;
            this.m_pepService = pepService;
            this.m_localizationService = localizationService;
        }

        /// <inheritdoc/>
        public string ServiceName => "Default Backup Manager";

        /// <inheritdoc/>
        public String Backup(BackupMedia media, string password = null)
        {
            if(this.m_configuration.RequireEncryptedBackups && String.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_POLICY_REQUIRES_ENCRYPTION));
            }


            // Set file and output
            String backupDescriptor = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            if(!this.m_configuration.TryGetBackupPath(media, out var backupPath))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }
            backupPath = Path.Combine(backupPath, Path.ChangeExtension(backupDescriptor, BACKUP_EXTENSION));

            if(media == BackupMedia.Private)
            {
                this.m_pepService.Demand(PermissionPolicyIdentifiers.CreatePrivateBackup);
            }
            else
            {
                this.m_pepService.Demand(PermissionPolicyIdentifiers.CreateAnyBackup);
            }
           
            try
            {

                this.m_tracer.TraceInfo("Will perform backup to {0}...", backupPath);
                var assets = this.m_serviceManager.GetServices()
                    .OfType<IProvideBackupAssets>()
                    .Distinct()
                    .SelectMany(o => o.GetBackupAssets())
                    .ToArray();

                using(var fs = File.Create(backupPath))
                {
                    using(var bw = BackupWriter.Create(fs, assets, password))
                    {
                        for(var i = 0; i < assets.Length; i++)
                        {
                            this.m_tracer.TraceInfo("Adding {0} to {1}", assets[i].Name, backupDescriptor);
                            this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs((float)i / assets.Length, this.m_localizationService.GetString(UserMessageStrings.BACKUP_CREATE_PROGRESS)));
                            bw.WriteAssetEntry(assets[i]);
                        }
                    }
                }

                return backupDescriptor;
            }
            catch(Exception ex)
            {
                throw new BackupException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_GEN_ERR), ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetBackupDescriptors(BackupMedia media)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ManageBackups);

            if(!this.m_configuration.TryGetBackupPath(media, out var backupPath))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }

            return Directory.GetFiles(backupPath, $"*.{BACKUP_EXTENSION}").Select(o=>Path.GetFileNameWithoutExtension(o));
        }

        /// <inheritdoc/>
        public bool HasBackup(BackupMedia media) => this.GetBackupDescriptors(media).Any();

        /// <inheritdoc/>
        public void RemoveBackup(BackupMedia media, string backupDescriptor)
        {
            if(String.IsNullOrEmpty(backupDescriptor))
            {
                throw new ArgumentNullException(nameof(backupDescriptor));
            }

            this.m_pepService.Demand(PermissionPolicyIdentifiers.ManageBackups);

            if(!this.m_configuration.TryGetBackupPath(media, out var backupPath))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }

            try
            {
                File.Delete(Path.Combine(backupPath, Path.ChangeExtension(backupDescriptor, BACKUP_EXTENSION)));
            }
            catch(Exception e)
            {
                throw new BackupException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_GEN_ERR), e);

            }

        }

        /// <inheritdoc/>
        public bool Restore(BackupMedia media, string backupDescriptor, string password = null)
        {
            if(String.IsNullOrEmpty(backupDescriptor))
            {
                throw new ArgumentNullException(nameof(backupDescriptor));
            }

            this.m_pepService.Demand(PermissionPolicyIdentifiers.ManageBackups);

            if(!this.m_configuration.TryGetBackupPath(media, out var backupFile))
            {
                throw new BackupException(String.Format(ErrorMessages.DEPENDENT_CONFIGURATION_MISSING, media));
            }

            backupFile = Path.Combine(backupFile, Path.ChangeExtension(backupFile, BACKUP_EXTENSION));
            if(!File.Exists(backupFile))
            {
                throw new FileNotFoundException(backupFile);
            }

            try
            {
                this.m_tracer.TraceInfo("Restoring {0}...", backupFile);
                var restoreProviderReference = new Dictionary<Guid, IRestoreBackupAssets>();
                this.m_serviceManager.GetServices()
                    .OfType<IRestoreBackupAssets>()
                    .ToList()
                    .ForEach(irba => irba.AssetClassIdentifiers.ToList().ForEach(c => restoreProviderReference.Add(c, irba)));

                int i = 0;
                using (var fs = File.OpenRead(backupFile)) {
                    using (var br = BackupReader.Open(fs, password))
                    {
                        while(br.GetNextEntry(out var backupAsset))
                        {
                            this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(((float)i++) / br.BackupAsset.Length, this.m_localizationService.GetString(UserMessageStrings.BACKUP_RESTORE_PROGRESS)));
                            using (backupAsset)
                            {
                                if (restoreProviderReference.TryGetValue(backupAsset.AssetClassId, out var restoreProvider))
                                {
                                    this.m_tracer.TraceInfo("Restoring {0}...", backupAsset.Name);
                                    restoreProvider.Restore(backupAsset);
                                }
                            }
                        }
                    }
                }

                this.RestartRequested?.Invoke(this, EventArgs.Empty);

                return true;
            }
            catch(Exception e)
            {
                throw new BackupException(this.m_localizationService.GetString(ErrorMessageStrings.BACKUP_RESTORE_ERR), e);
            }
        }
    }
}
