/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Represents a job that will prune the data quality extensions
    /// </summary>
    public class DataQualityExtensionCleanJob : IJob
    {
        // Clean obsolete tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityExtensionCleanJob));
        // State manager
        private readonly IJobStateManagerService m_stateManagerService;
        private readonly IDataQualityConfigurationProviderService m_dataQualityConfigurationProvider;
        private readonly IDataPersistenceServiceEx<EntityExtension> m_entityExtensionPersistence;
        private readonly IDataPersistenceServiceEx<ActExtension> m_actExtensionPersistence;

        /// <summary>
        /// Gets the id of the job
        /// </summary>
        public Guid Id => Guid.Parse("FC00A663-F670-4E3E-8766-196610186B37");

        /// <summary>
        /// Gets the name of the job
        /// </summary>
        public string Name => "Data Quality Job";

        /// <inheritdoc/>
        public string Description => "Cleans and re-runs data quality rules for all data in the database";

        /// <summary>
        /// DI constructor
        /// </summary>
        public DataQualityExtensionCleanJob(IJobStateManagerService stateManagerService,
            IDataQualityConfigurationProviderService dataQualityConfigurationProviderService,
            IDataPersistenceServiceEx<EntityExtension> entityExtensionPersistence,
            IDataPersistenceServiceEx<ActExtension> actExtensionPersistence)
        {
            this.m_dataQualityConfigurationProvider = dataQualityConfigurationProviderService;
            this.m_entityExtensionPersistence = entityExtensionPersistence;
            this.m_actExtensionPersistence = actExtensionPersistence;
            this.m_stateManagerService = stateManagerService;
        }

        /// <summary>
        /// True if can cancel
        /// </summary>
        public bool CanCancel => false;

        /// <summary>
        /// Gets the parameters for this job
        /// </summary>
        public IDictionary<string, Type> Parameters => new Dictionary<String, Type>()
        {
            { "re-calculate", typeof(bool) },
            { "clear", typeof(bool) }
        };

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
                this.m_stateManagerService.SetState(this, JobStateType.Running);

                this.m_tracer.TraceInfo("Cleaning Entity extensions...");
                var shouldClear = parameters.Length == 0 || parameters.Length >= 2 && (parameters[1] == null || (bool?)parameters[1] == true);
                var shouldCalc = parameters.Length == 0 || parameters.Length >= 1 && (bool?)parameters[0] == true; // For backwards compatibility

                if (shouldClear)
                {
                    using (DataPersistenceControlContext.Create(DeleteMode.PermanentDelete))
                    {
                        this.m_entityExtensionPersistence.DeleteAll(o => o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension && o.ObsoleteVersionSequenceId != null, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                        this.m_actExtensionPersistence.DeleteAll(o => o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension && o.ObsoleteVersionSequenceId != null, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    }
                }
                if (shouldCalc)
                {
                    foreach (var config in this.m_dataQualityConfigurationProvider.GetRuleSets().SelectMany(o => o.Resources.Select(r => new { res = r, conf = o })).GroupBy(o => o.res))
                    {
                        var persistenceType = typeof(IDataPersistenceService<>).MakeGenericType(config.Key.ResourceType);
                        var persistenceService = ApplicationServiceContext.Current.GetService(persistenceType) as IDataPersistenceService;
                        if (persistenceService == null)
                        {
                            throw new InvalidOperationException();
                        }
                        var query = QueryExpressionParser.BuildLinqExpression(config.Key.ResourceType, String.Join("&", StatusKeys.ActiveStates.Select(o => $"statusConcept={o}")).ParseQueryString());
                        foreach (var resource in persistenceService.Query(query).OfType<IExtendable>())
                        {
                            var tag = resource.TagDataQualityIssues(false);
                            switch (tag)
                            {
                                case EntityExtension ee:
                                    this.m_entityExtensionPersistence.Insert(ee, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                                    break;
                                case ActExtension ae:
                                    this.m_actExtensionPersistence.Insert(ae, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                                    break;
                            }
                        }
                    }
                }
                this.m_tracer.TraceInfo("Completed cleaning extensions...");

                this.m_stateManagerService.SetState(this, JobStateType.Completed);
            }
            catch (Exception ex)
            {
                this.m_tracer.TraceInfo("Error cleaning data quality extensions: {0}", ex);
                this.m_stateManagerService.SetProgress(this, ex.Message, 0.0f);
                this.m_stateManagerService.SetState(this, JobStateType.Aborted, ex.ToHumanReadableString());

            }
        }
    }
}