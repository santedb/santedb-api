﻿/*
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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Data.Initialization
{
    /// <summary>
    /// Data initialization service
    /// </summary>
    /// <remarks>
    /// <para>This service will read all datasets provided by any registered <see cref="IDatasetProvider"/> and will instal them via the 
    /// configured <see cref="IDatasetInstallerService"/></para>
    /// </remarks>
    [ServiceProvider("Dataset Installation Service")]
    public class DataInitializationService : IDaemonService, IReportProgressChanged
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "DataSet Initialization Service";

        // Trace source
        private readonly Tracer m_traceSource = Tracer.GetTracer(typeof(DataInitializationService));
        private readonly IDatasetInstallerService m_datasetInstaller;
        private readonly IEnumerable<IDatasetProvider> m_datasetProviders;

        /// <summary>
        /// True when the service is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Fired when the service has started
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Fired when the service is starting
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Fired when the service has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Fired when the service is stopping
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// Fired when progress changes
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// DI constructor
        /// </summary>
        public DataInitializationService(IDatasetInstallerService datasetInstaller, IServiceManager serviceManager)
        {
            this.m_datasetInstaller = datasetInstaller;
            this.m_datasetProviders = serviceManager.CreateInjectedOfAll<IDatasetProvider>().Distinct();
        }

        /// <summary>
        /// Start the service
        /// </summary>
        public bool Start()
        {
            using (AuthenticationContext.EnterSystemContext())
            {
                foreach (var dataset in this.m_datasetProviders.SelectMany(o => o.GetDatasets()))
                {
                    this.m_datasetInstaller.Install(dataset);
                }
                return true;
            }
        }

        /// <summary>
        /// Stopped
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            return true;
        }
    }
}