using SanteDB.Core.Data.Backup;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// Routinely backs up a server process.
    /// </summary>
    public class ServerBackupJob : IJob
    {
        public static readonly Guid JOB_ID = Guid.Parse("AC5AC4C3-5C48-49D3-83C7-5BF6649CF3F6");

        public Guid Id => JOB_ID;

        public string Name => "Server Backup Job";

        public string Description => "Regularly backs up the server.";

        public bool CanCancel => false;

        private int _MaxBackups = 5;

        public IDictionary<string, Type> Parameters => new Dictionary<string, Type>();

        readonly IJobStateManagerService _JobStateManager;
        readonly IBackupService _BackupService;
        readonly Tracer _Tracer = Tracer.GetTracer(typeof(ServerBackupJob));

        /// <summary>
        /// Instantiates a new instance of the <see cref="ServerBackupJob"/>.
        /// </summary>
        /// <param name="jobStateManagerService">DI Injected</param>
        /// <param name="backupService">DI Injected</param>
        public ServerBackupJob(IJobStateManagerService jobStateManagerService, IBackupService backupService)
        {
            _JobStateManager = jobStateManagerService;
            _BackupService = backupService;
        }


        /// <summary>
        /// This operation is not supported on <see cref="ServerBackupJob"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">The exception type thrown if this job is mistakenly called.</exception>
        public void Cancel()
        {
            throw new NotSupportedException("This job cannot be cancelled.");
        }

        ///<inheritdoc />
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    _JobStateManager.SetState(this, JobStateType.Running);

                    // Backup this device
                    _Tracer.TraceInfo("Performing server system backup - ");
                    _BackupService.Backup(BackupMedia.Private, $"{Environment.UserName}@{Environment.UserDomainName}@{Environment.MachineName}");

                    // Remove any unnecessary backups
                    foreach (var backup in _BackupService.GetBackupDescriptors(BackupMedia.Private).OrderByDescending(o => o.Timestamp).Skip(_MaxBackups))
                    {
                        _Tracer.TraceInfo("Removing old backup {0}", backup);
                        _BackupService.RemoveBackup(BackupMedia.Private, backup.Label);
                    }

                    _JobStateManager.SetState(this, JobStateType.Completed);
                }
            }
            catch (Exception ex)
            {
                _Tracer.TraceError("Error running backup job: {0}", ex.ToHumanReadableString());
                _JobStateManager.SetState(this, JobStateType.Aborted, ex.ToHumanReadableString());
            }
        }
    }
}
