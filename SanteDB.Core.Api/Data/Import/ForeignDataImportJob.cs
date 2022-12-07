using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Jobs;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A <see cref="IJob"/> implementation that executes all <see cref="IForeignDataSubmission"/> instructions in a 
    /// state of ready.
    /// </summary>
    public class ForeignDataImportJob : IJob
    {

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(ForeignDataImportJob));
        private readonly IJobStateManagerService m_jobStateManager;
        private readonly IForeignDataManagerService m_fdManager;
        private readonly IForeignDataImporter m_fdTransformer;
        private float m_fdManagerProgress = 0.0f;
        private float m_fdImportProgress = 0.0f;

        /// <summary>
        /// DI constructor
        /// </summary>
        public ForeignDataImportJob(IJobStateManagerService jobStateManagerService, 
            IForeignDataManagerService foreignDataManagerService,
            IForeignDataImporter foreignDataTransformerService)
        {
            this.m_jobStateManager = jobStateManagerService;
            this.m_fdManager = foreignDataManagerService;
            this.m_fdTransformer = foreignDataTransformerService;
            if(this.m_fdTransformer is IReportProgressChanged irpc)
            {
                irpc.ProgressChanged += (o, e) => this.m_fdImportProgress = e.Progress;
            }
            if(this.m_fdManager is IReportProgressChanged irpc2)
            {
                irpc2.ProgressChanged += (o, e) => this.m_fdManagerProgress = e.Progress;
            }
        }

        /// <inheritdoc/>
        public Guid Id => Guid.Parse("2EBF8094-6628-4CEC-93B8-3D623F227722");

        /// <inheritdoc/>
        public string Name => "Foreign Data Import Background Job";

        /// <inheritdoc/>
        public string Description => "Processes staged foreign data imports";

        /// <inheritdoc/>
        public bool CanCancel => false;

        /// <inheritdoc/>
        public IDictionary<string, Type> Parameters => new Dictionary<String, Type>();

        /// <inheritdoc/>
        public void Cancel()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {
                this.m_jobStateManager.SetState(this, JobStateType.Running);

                var scheduledJobs = this.m_fdManager.Find(o => o.Status == ForeignDataStatus.Scheduled).ToArray();
                var progressPerFile = 1.0f / (float)scheduledJobs.Length;

                for (int i = 0; i < scheduledJobs.Length; i++)
                {
                    this.m_jobStateManager.SetProgress(this, String.Format(UserMessages.IMPORTING_NAME, scheduledJobs[i].Name), this.m_fdManagerProgress + i * progressPerFile);
                    this.m_fdManager.Execute(scheduledJobs[i].Key.Value);
                }

                this.m_jobStateManager.SetState(this, JobStateType.Completed);
            }
            catch(Exception ex)
            {
                this.m_tracer.TraceError("Error executing transform job: {0}", ex);
                this.m_jobStateManager.SetState(this, JobStateType.Aborted);
            }
        }
    }
}
