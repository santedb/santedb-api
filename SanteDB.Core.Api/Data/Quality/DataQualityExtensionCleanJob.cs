using SanteDB.Core.Diagnostics;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Represents a job that will prune the data quality extensions
    /// </summary>
    public class DataQualityExtensionCleanJob : IJob
    {

        // Clean obsolete tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityExtensionCleanJob));

        /// <summary>
        /// Gets the name of the job
        /// </summary>
        public string Name => "Data Quality Extension Clean";

        /// <summary>
        /// True if can cancel
        /// </summary>
        public bool CanCancel => false;

        /// <summary>
        /// Gets the current state
        /// </summary>
        public JobStateType CurrentState { get; private set; }

        /// <summary>
        /// Gets the parameters for this job
        /// </summary>
        public IDictionary<string, Type> Parameters => null;

        /// <summary>
        /// Gets the time that the job was last run
        /// </summary>
        public DateTime? LastStarted { get; private set; }

        /// <summary>
        /// Gets the time that the job was last finished
        /// </summary>
        public DateTime? LastFinished { get; private set; }

        /// <summary>
        /// Cancel the job
        /// </summary>
        public void Cancel()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Run the specified job
        /// </summary>
        public void Run(object sender, EventArgs e, object[] parameters)
        {

            this.m_tracer.TraceInfo("Starting clean of data quality extensions...");
            try
            {
                this.CurrentState = JobStateType.Running;
                this.LastStarted = DateTime.Now;

                var entityService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<EntityExtension>>();
                var actService = ApplicationServiceContext.Current.GetService<IDataPersistenceService<ActExtension>>();

                this.m_tracer.TraceInfo("Cleaning Entity extensions...");
                int ofs = 0, tr = 1;
                while (ofs < tr)
                {
                    var results = entityService.Query(o => o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension && o.ObsoleteVersionSequenceId != null, ofs, 100, out tr, AuthenticationContext.SystemPrincipal) as IEnumerable;
                    foreach (EntityExtension r in results)
                        entityService.Obsolete(r, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    ofs += 100;
                }

                this.m_tracer.TraceInfo("Cleaning Act extensions...");
                ofs = 0;
                tr = 1;
                while (ofs < tr)
                {
                    var results = actService.Query(o => o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension && o.ObsoleteVersionSequenceId != null, ofs, 100, out tr, AuthenticationContext.SystemPrincipal) as IEnumerable;
                    foreach (ActExtension r in results)
                        actService.Obsolete(r, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    ofs += 100;
                }

                this.m_tracer.TraceInfo("Completed cleaning extensions...");

                this.CurrentState = JobStateType.Completed;
                this.LastFinished = DateTime.Now;
            }
            catch (Exception ex)
            {
                this.m_tracer.TraceInfo("Error cleaning data quality extensions: {0}", ex);
                this.CurrentState = JobStateType.Aborted;
            }
        }
    }
}
