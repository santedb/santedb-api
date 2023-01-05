using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Jobs;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A <see cref="IJob"/> implementation that executes all <see cref="IForeignDataSubmission"/> instructions in a 
    /// state of ready.
    /// </summary>
    public class ForeignDataImportJob : IJob
    {
        /// <summary>
        /// JOB ID
        /// </summary>
        public static readonly Guid JOB_ID = Guid.Parse("2EBF8094-6628-4CEC-93B8-3D623F227722");
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(ForeignDataImportJob));
        private readonly IJobStateManagerService m_jobStateManager;
        private readonly IForeignDataManagerService m_fdManager;
        private float m_fdManagerProgress = 0.0f;
        private string m_fdManagerState = String.Empty;
        private bool m_cancelRequested = false;

        /// <summary>
        /// DI constructor
        /// </summary>
        public ForeignDataImportJob(IJobStateManagerService jobStateManagerService,
            IForeignDataManagerService foreignDataManagerService)
        {
            this.m_jobStateManager = jobStateManagerService;
            this.m_fdManager = foreignDataManagerService;
            if (this.m_fdManager is IReportProgressChanged irpc2)
            {
                irpc2.ProgressChanged += (o, e) =>
                {
                    this.m_fdManagerProgress = e.Progress;
                    this.m_fdManagerState = e.State.ToString();
                };
            }


        }

        /// <inheritdoc/>
        public Guid Id => JOB_ID;

        /// <inheritdoc/>
        public string Name => "Foreign Data Import Background Job";

        /// <inheritdoc/>
        public string Description => "Processes staged foreign data imports";

        /// <inheritdoc/>
        public bool CanCancel => true;

        /// <inheritdoc/>
        public IDictionary<string, Type> Parameters => new Dictionary<String, Type>();

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_cancelRequested = true;
            this.m_jobStateManager.SetState(this, JobStateType.Cancelled);

        }

        /// <inheritdoc/>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {
                this.m_jobStateManager.SetState(this, JobStateType.Running);

                var scheduledJobs = this.m_fdManager.Find(o => o.Status == ForeignDataStatus.Scheduled).ToArray();
                var progressPerFile = 1.0f / (float)scheduledJobs.Length;
                int fileIndex = 0;

                using (new Timer((o) => this.m_jobStateManager.SetProgress(this, this.m_fdManagerState, this.m_fdManagerProgress * progressPerFile + fileIndex * progressPerFile), null, 100, 1000))
                {
                    using (AuthenticationContext.EnterSystemContext())
                    {
                        this.m_cancelRequested = false;
                        for (fileIndex = 0; fileIndex < scheduledJobs.Length && !this.m_cancelRequested; fileIndex++)
                        {
                            this.m_fdManager.Execute(scheduledJobs[fileIndex].Key.Value);
                        }

                    }
                }
                if (this.m_cancelRequested)
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Cancelled);
                }
                else
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Completed);
                }
            }
            catch (Exception ex)
            {
                this.m_tracer.TraceError("Error executing transform job: {0}", ex);
                this.m_jobStateManager.SetState(this, JobStateType.Aborted);
            }
        }
    }
}
