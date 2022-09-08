/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// A generic data retention job that reads from the IDataPersistence service and uses
    /// the IDataArchive service to retain data
    /// </summary>
    [DisplayName("Data Retention Job")]
    public class DataRetentionJob : IJob
    {
        private readonly IJobStateManagerService m_stateManager;

        // Tracer 
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DataRetentionJob));

        // Cancel flag
        private bool m_cancelFlag = false;

        // Configuration
        private DataRetentionConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<DataRetentionConfigurationSection>();

        /// <summary>
        /// Data retention job DI constructor
        /// </summary>
        public DataRetentionJob(IJobStateManagerService stateManager)
        {
            this.m_stateManager = stateManager;
        }

        /// <summary>
        /// Gets the identifier of this job
        /// </summary>
        public Guid Id => Guid.Parse("71F82F18-992D-4A71-9D0C-ECCE92490D8C");

        /// <summary>
        /// Gets the name of the service
        /// </summary>
        public string Name => "Data Retention Policy Job";

        /// <inheritdoc/>
        public string Description => "Runs the configured data retention policy rules and moves data to the archival service";

        /// <summary>
        /// Can cancel the job
        /// </summary>
        public bool CanCancel => true;

        /// <summary>
        /// Gets the parameters
        /// </summary>
        public IDictionary<string, Type> Parameters => null;

        /// <summary>
        /// Cancel the current job
        /// </summary>
        public void Cancel()
        {
            this.m_cancelFlag = true;
            this.m_stateManager.SetState(this, JobStateType.Cancelled);
        }

        /// <summary>
        /// Run the data retention job
        /// </summary>
        public void Run(object sender, EventArgs e, object[] parameters)
        {
            try
            {

                if (this.m_stateManager.GetJobState(this).IsRunning())
                {
                    return;
                }

                this.m_stateManager.SetState(this, JobStateType.Running);
                float ruleProgress = 1.0f / this.m_configuration.RetentionRules.Count;

                var variables = this.m_configuration.Variables.ToDictionary(o => o.Name, o => o.CompileFunc());

                for (var ruleIdx = 0; ruleIdx < this.m_configuration.RetentionRules.Count && !this.m_cancelFlag; ruleIdx++)
                {
                    var rule = this.m_configuration.RetentionRules[ruleIdx];

                    this.m_tracer.TraceInfo("Running retention rule {0} ({1} {2})", rule.Name, rule.Action, rule.ResourceType.TypeXml);
                    this.m_stateManager.SetProgress(this, $"Gathering {rule.Name} ({rule.ResourceType.TypeXml})", ruleIdx * ruleProgress);

                    var pserviceType = typeof(IDataPersistenceService<>).MakeGenericType(rule.ResourceType.Type);
                    var persistenceService = ApplicationServiceContext.Current.GetService(pserviceType) as IBulkDataPersistenceService;
                    if (persistenceService == null)
                        throw new InvalidOperationException("Cannot locate appropriate persistence service");

                    // Included keys for retention
                    IEnumerable<Guid> keys = new Guid[0];
                    for (int inclIdx = 0; inclIdx < rule.IncludeExpressions.Length; inclIdx++)
                    {
                        var expr = QueryExpressionParser.BuildLinqExpression(rule.ResourceType.Type, NameValueCollection.ParseQueryString(rule.IncludeExpressions[inclIdx]), "rec", variables);
                        this.m_stateManager.SetProgress(this, $"Gathering {rule.Name} ({rule.ResourceType.TypeXml})", (float)((ruleIdx * ruleProgress) + ((float)inclIdx / rule.IncludeExpressions.Length) * 0.3 * ruleProgress));
                        int offset = 0, totalCount = 1;
                        while (offset < totalCount) // gather the included keys
                        {
                            keys = keys.Union(persistenceService.QueryKeys(expr, offset, 1000, out totalCount));
                            offset += 1000;
                        }
                    }

                    // Exclude keys from retention
                    for (int exclIdx = 0; exclIdx < rule.ExcludeExpressions.Length; exclIdx++)
                    {
                        var expr = QueryExpressionParser.BuildLinqExpression(rule.ResourceType.Type, NameValueCollection.ParseQueryString(rule.ExcludeExpressions[exclIdx]), "rec", variables);
                        this.m_stateManager.SetProgress(this, $"Gathering {rule.Name} ({rule.ResourceType.TypeXml})", (float)((ruleIdx * ruleProgress) + (0.3 + ((float)exclIdx / rule.ExcludeExpressions.Length) * 0.3) * ruleProgress));
                        int offset = 0, totalCount = 1;
                        while (offset < totalCount) // gather the included keys
                        {
                            keys = keys.Except(persistenceService.QueryKeys(expr, offset, 1000, out totalCount));
                            offset += 1000;
                        }
                    }


                    
                    EventHandler<Services.ProgressChangedEventArgs> callback = (o,ev ) => this.m_stateManager.SetProgress(this, $"Executing {rule.Action} {rule.ResourceType.TypeXml} ({rule.Name})", ev.Progress);

                    if (persistenceService is IReportProgressChanged irpc)
                    {
                        irpc.ProgressChanged += callback;
                    }

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
                                archiveService.Archive(rule.ResourceType.Type, keys.ToArray());
                                persistenceService.Purge(TransactionMode.Commit, AuthenticationContext.SystemPrincipal, keys.ToArray());
                            }
                            else if (rule.Action.HasFlag(DataRetentionActionType.Obsolete))
                            {
                                archiveService.Archive(rule.ResourceType.Type, keys.ToArray());
                                persistenceService.Obsolete(TransactionMode.Commit, AuthenticationContext.SystemPrincipal, keys.ToArray());
                            }
                            else
                            {
                                archiveService.Archive(rule.ResourceType.Type, keys.ToArray());
                            }
                            break;
                    }

                    if (persistenceService is IReportProgressChanged irpc2)
                    {
                        irpc2.ProgressChanged -= callback;
                    }
                }

                this.m_stateManager.SetState(this, JobStateType.Completed);
            }
            catch (Exception ex) // Absolute failure
            {
                this.m_tracer.TraceError("Failure running retention job: {0}", ex);
                this.m_stateManager.SetProgress(this, ex.Message, 0.0f);
                this.m_stateManager.SetState(this, JobStateType.Aborted);
            }
            finally
            {
                this.m_cancelFlag = false;
            }
        }
    }
}