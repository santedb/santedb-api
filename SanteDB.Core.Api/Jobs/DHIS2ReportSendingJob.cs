/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using Newtonsoft.Json;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Notifications;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core.Notifications.Email;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model;
using System.Reflection;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Notifications.RapidPro;
using System.Globalization;
using Newtonsoft.Json.Linq;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Map;
using SanteDB;

namespace SanteDB.Core.Jobs
{
    /// <summary>
    /// </summary>
    public class DHIS2ReportSendingJob : IJob
    {
        private readonly Tracer m_tracer;
        private bool m_cancelRequested = false;
        private readonly IJobStateManagerService m_jobStateManager;

        /// <summary>
        /// Job Id
        /// </summary>
        public static readonly Guid JOB_ID = Guid.Parse("9099649e-c147-4c44-a487-f2f22d2ed336");

        /// <inheritdoc/>
        public Guid Id => JOB_ID;

        /// <summary>
        /// Completed successfully state key
        /// </summary>
        private static readonly Guid COMPLETED_SUCCESSFULLY_STATE_KEY = Guid.Parse("2726BC79-A55A-4FEA-BE2C-627265872DB5");

        /// <summary>
        /// Completed with errors state key
        /// </summary>
        private static readonly Guid COMPLETED_WITH_ERRORS_STATE_KEY = Guid.Parse("92455ACD-ECC2-4E89-9227-94B77B27420D");

        /// <inheritdoc/>
        public DateTime? LastStarted { get; private set; }

        /// <inheritdoc/>
        public DateTime? LastFinished { get; private set; }

        /// <inheritdoc/>
        public string Name => "DHIS2 Report Sending Job Job";

        /// <inheritdoc/>
        public string Description => "Sends the indicator reports to the DHIS2 endpoint";

        /// <inheritdoc/>87
        public IDictionary<string, Type> Parameters => null;

        /// <inheritdoc/>
        public bool CanCancel => true;

        /// <summary>
        /// Dependency injected constructor
        /// </summary>
        public DHIS2ReportSendingJob()
        {
            this.m_tracer = Tracer.GetTracer(this.GetType());
        }

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
                this.m_cancelRequested = false;
                this.LastStarted = DateTime.Now;
                if (this.m_cancelRequested)
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Cancelled);
                }
                else
                {
                    this.m_jobStateManager.SetState(this, JobStateType.Completed);
                }

                this.LastFinished = DateTime.Now;
            }
            catch (Exception ex)
            {
                this.m_jobStateManager.SetState(this, JobStateType.Aborted, ex.ToHumanReadableString());
            }
        }
    }
}
