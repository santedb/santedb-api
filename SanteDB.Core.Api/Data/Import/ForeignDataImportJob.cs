using SanteDB.Core.Diagnostics;
using SanteDB.Core.Jobs;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A <see cref="IJob"/> implementation that executes all <see cref="IForeignDataInfo"/> instructions in a 
    /// state of ready.
    /// </summary>
    public class ForeignDataImportJob : IJob
    {

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(ForeignDataImportJob));
        private readonly IJobStateManagerService m_jobStateManager;
        private readonly IForeignDataManagerService m_fdManager;
        private readonly IForeignDataImporter m_fdTransformer;

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
                irpc.ProgressChanged += this.OnExecutionStateChange;
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

        /// <summary>
        /// When the underlying exueciton state has changed capture and forward
        /// </summary>
        public void OnExecutionStateChange(object sender, ProgressChangedEventArgs e)
        {
            if(this.m_jobStateManager.GetJobState(this).CurrentState == JobStateType.Running)
            {
                this.m_jobStateManager.SetProgress(this, e.State.ToString(), e.Progress);
            }
        }

        /// <inheritdoc/>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {
                this.m_jobStateManager.SetState(this, JobStateType.Running);

                foreach(var readyState in this.m_fdManager.Find(o=>o.Status == ForeignDataStatus.Scheduled))
                {
                    this.m_fdManager.Execute(readyState.Key.Value);
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
