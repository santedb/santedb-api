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
 * Date: 2021-8-27
 */

using SanteDB.Core.Diagnostics;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Quality
{
    /// <summary>
    /// Represents a job that will prune the data quality extensions
    /// </summary>
    public class DataQualityExtensionCleanJob : IReportProgressJob
    {
        // Clean obsolete tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DataQualityExtensionCleanJob));
        private readonly IDataPersistenceServiceEx<EntityExtension> m_entityExtensionPersistence;
        private readonly IDataPersistenceServiceEx<ActExtension> m_actExtensionPersistence;

        /// <summary>
        /// Gets the id of the job
        /// </summary>
        public Guid Id => Guid.Parse("FC00A663-F670-4E3E-8766-196610186B37");

        /// <summary>
        /// Gets the name of the job
        /// </summary>
        public string Name => "Data Quality Extension Clean";

        /// <inheritdoc/>
        public string Description => "Cleans obsolete or otherwise amended data quality extension tags";

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
        /// Gets the progress
        /// </summary>
        public float Progress { get; private set; }

        /// <summary>
        /// Gets or sets the status text
        /// </summary>
        public string StatusText { get; private set; }

        /// <summary>
        /// DI constructor
        /// </summary>
        public DataQualityExtensionCleanJob(IDataPersistenceServiceEx<EntityExtension> entityExtensionPersistence, IDataPersistenceServiceEx<ActExtension> actExtensionPersistence)
        {
            this.m_entityExtensionPersistence = entityExtensionPersistence;
            this.m_actExtensionPersistence = actExtensionPersistence;
            if(entityExtensionPersistence is IReportProgressChanged irp)
            {
                irp.ProgressChanged += (o, e) =>
                {
                    this.StatusText = "Clearing data quality extensions";
                    this.Progress = e.Progress * 0.5f;
                };
            }

            if (actExtensionPersistence is IReportProgressChanged irp2)
            {
                irp2.ProgressChanged += (o, e) =>
                {
                    this.StatusText = "Clearing data quality extensions";
                    this.Progress = (e.Progress * 0.5f) + 0.5f;
                };
            }
        }


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



                this.m_tracer.TraceInfo("Cleaning Entity extensions...");
                using (DataPersistenceControlContext.Create(DeleteMode.PermanentDelete))
                {
                    this.m_entityExtensionPersistence.DeleteAll(o => o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension && o.ObsoleteVersionSequenceId != null, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
                    this.m_actExtensionPersistence.DeleteAll(o => o.ExtensionTypeKey == ExtensionTypeKeys.DataQualityExtension && o.ObsoleteVersionSequenceId != null, TransactionMode.Commit, AuthenticationContext.SystemPrincipal);
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