using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// A generic data retention job that reads from the IDataPersistence service and uses 
    /// the IDataArchive service to retain data
    /// </summary>
    public class DataRetentionJob : IReportProgressJob
    {

        // Tracer 
        private Tracer m_tracer = Tracer.GetTracer(typeof(DataRetentionJob));

        // Cancel flag
        private bool m_cancelFlag = false;

        // Configuration 
        private DataRetentionConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<DataRetentionConfigurationSection>();

        /// <summary>
        /// Gets the name of the service
        /// </summary>
        public string Name => "Data Retention Policy Job";

        /// <summary>
        /// Can cancel the job
        /// </summary>
        public bool CanCancel => true;

        /// <summary>
        /// Gets or sets the current state
        /// </summary>
        public JobStateType CurrentState { get; private set; }

        /// <summary>
        /// Gets the parameters
        /// </summary>
        public IDictionary<string, Type> Parameters => null;

        /// <summary>
        /// Last time the job started
        /// </summary>
        public DateTime? LastStarted { get; private set; }

        /// <summary>
        /// Last time the job finished
        /// </summary>
        public DateTime? LastFinished { get; private set; }

        /// <summary>
        /// Get the current progress
        /// </summary>
        public float Progress { get; private set; }

        /// <summary>
        /// Gets the current status
        /// </summary>
        public string StatusText { get; private set; }

        /// <summary>
        /// Cancel the current job
        /// </summary>
        public void Cancel()
        {
            this.m_cancelFlag = true;
            this.CurrentState = JobStateType.Cancelled;
        }

        /// <summary>
        /// Run the data retention job
        /// </summary>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {

                this.CurrentState = JobStateType.Running;
                this.LastStarted = DateTime.Now;
                float ruleProgress = 1.0f / this.m_configuration.RetentionRules.Count;

                for (var ruleIdx = 0; ruleIdx < this.m_configuration.RetentionRules.Count; ruleIdx++)
                {
                    var rule = this.m_configuration.RetentionRules[ruleIdx];

                    this.m_tracer.TraceInfo("Running retention rule {0} ({1} {2})", rule.Name, rule.Action, rule.ResourceTypeXml);
                    this.StatusText = $"Gathering {rule.Name} ({rule.ResourceTypeXml})";
                    this.Progress = ruleIdx * ruleProgress;

                    var pserviceType = typeof(IDataPersistenceService<>).MakeGenericType(rule.ResourceType);
                    var persistenceService = ApplicationServiceContext.Current.GetService(pserviceType) as IBulkDataPersistenceService;
                    if (persistenceService == null)
                        throw new InvalidOperationException("Cannot locate appropriate persistence service");

                    // Included keys for retention
                    IEnumerable<Guid> keys = new Guid[0];
                    for (int inclIdx = 0; inclIdx < rule.IncludeExpressions.Count; inclIdx++)
                    {
                        var expr = QueryExpressionParser.BuildLinqExpression(rule.ResourceType, NameValueCollection.ParseQueryString(rule.IncludeExpressions[inclIdx]));
                        this.Progress = (float)((ruleIdx * ruleProgress) + ((float)inclIdx / rule.IncludeExpressions.Count) * 0.3 * ruleProgress);
                        int offset = 0, totalCount = 1;
                        while (offset < totalCount) // gather the included keys
                        {
                            keys = keys.Union(persistenceService.QueryKeys(expr, offset, 1000, out totalCount));
                            offset += 1000;
                        }
                    }

                    // Exclude keys from retention
                    for (int exclIdx = 0; exclIdx < rule.ExcludeExpressions.Count; exclIdx++)
                    {
                        var expr = QueryExpressionParser.BuildLinqExpression(rule.ResourceType, NameValueCollection.ParseQueryString(rule.ExcludeExpressions[exclIdx]));
                        this.Progress = (float)((ruleIdx * ruleProgress) + (0.3 + ((float)exclIdx / rule.ExcludeExpressions.Count) * 0.3) * ruleProgress);
                        int offset = 0, totalCount = 1;
                        while (offset < totalCount) // gather the included keys 
                        {
                            keys = keys.Except(persistenceService.QueryKeys(expr, offset, 1000, out totalCount));
                            offset += 1000;
                        }
                    }

                    this.StatusText = $"Executing {rule.Action} {rule.ResourceTypeXml} ({rule.Name})";

                    // Now we want to execute the specified action
                    switch (rule.Action)
                    {
                        case DataRetentionActionType.Obsolete:
                            persistenceService.Obsolete(TransactionMode.Commit, AuthenticationContext.SystemPrincipal, keys.ToArray());
                            break;
                        case DataRetentionActionType.Purge:
                            persistenceService.Purge(TransactionMode.Commit, AuthenticationContext.SystemPrincipal, keys.ToArray());
                            break;
                        case DataRetentionActionType.Archive:
                        case DataRetentionActionType.Archive | DataRetentionActionType.Obsolete:
                        case DataRetentionActionType.Archive | DataRetentionActionType.Purge:

                            var archiveService = ApplicationServiceContext.Current.GetService<IDataArchiveService>();
                            if (archiveService == null)
                                throw new InvalidOperationException("Could not find archival service");
                            // Test PURGE
                            if (rule.Action.HasFlag(DataRetentionActionType.Purge))
                            {
                                archiveService.Archive(rule.ResourceType, keys.ToArray());
                                persistenceService.Purge(TransactionMode.Commit, AuthenticationContext.SystemPrincipal, keys.ToArray());
                            }
                            else if (rule.Action.HasFlag(DataRetentionActionType.Obsolete))
                            {
                                archiveService.Archive(rule.ResourceType, keys.ToArray());
                                persistenceService.Obsolete(TransactionMode.Commit, AuthenticationContext.SystemPrincipal, keys.ToArray());
                            }
                            else
                            {
                                archiveService.Archive(rule.ResourceType, keys.ToArray());
                            }
                            break;
                    }
                }

                this.LastFinished = DateTime.Now;

            }
            catch (Exception ex) // Absolute failure
            {
                this.m_tracer.TraceError("Failure running retention job: {0}", ex);
                this.CurrentState = JobStateType.Aborted;
            }
            finally
            {
                this.m_cancelFlag = false;
            }
        }
    }
}
