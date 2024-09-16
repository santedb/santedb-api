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
using SanteDB.Core.Configuration;
using SanteDB.Core.Data.Management.Jobs;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Jobs;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Data.Management
{
    /// <summary>
    /// Represents a <see cref="IDataManagementPattern"/> which uses destructive merge and matching in order
    /// to contain a single instance.
    /// </summary>
    /// <remarks>
    /// <para>The SIM data management service implements the <see href="https://help.santesuite.org/santedb/data-storage-patterns#single-instance-mode">Single Instance Mode</see> of
    /// storage pattern. The single instance mode:</para>
    /// <list type="bullet">
    ///     <item>Maintains a single copy of a record in the CDR</item>
    ///     <item>Attempts to perform duplicate detection between these single instances</item>
    ///     <item>When a merge occurs, the subsumed record is obsoleted (and later purged)</item>
    ///     <item>Unmerge is not possible</item>
    /// </list>
    /// </remarks>
    public class SimDataManagementService : IDaemonService, IDataManagementPattern
    {

        // Tracer for SIM
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(SimDataManagementService));
        private readonly ResourceManagementConfigurationSection m_configuration;
        private readonly IServiceManager m_serviceManager;
        private readonly IJobManagerService m_jobManager;

        /// <summary>
        /// DI ctor
        /// </summary>
        public SimDataManagementService(IConfigurationManager configurationManager,
            IServiceManager serviceManager,
            IJobManagerService jobManager
            )
        {
            this.m_configuration = configurationManager.GetSection<ResourceManagementConfigurationSection>();
            this.m_serviceManager = serviceManager;
            this.m_jobManager = jobManager;
        }

        // Merge services
        private List<IDisposable> m_mergeServices = new List<IDisposable>();

        /// <summary>
        /// True if is running
        /// </summary>
        public bool IsRunning => false;

        /// <summary>
        /// Service name
        /// </summary>
        public string ServiceName => "Single Instance Data Management";

        /// <summary>
        /// Service is starting
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Service has started
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Service is stopping
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// Service has stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Start the service
        /// </summary>
        public bool Start()
        {
            Starting?.Invoke(this, EventArgs.Empty);

            if (m_configuration.ResourceTypes?.Any() == true)
            {
                // Register mergers for all types in configuration
                foreach (var i in m_configuration.ResourceTypes)
                {
                    m_tracer.TraceInfo("Creating record management service for {0}", i.Type.Name);
                    var idt = typeof(SimResourceInterceptor<>).MakeGenericType(i.Type);
                    var manager = this.m_serviceManager.CreateInjected(idt) as IDisposable;
                    this.m_serviceManager.AddServiceProvider(manager);
                    this.m_mergeServices.Add(manager);

                    idt = typeof(MatchJob<>).MakeGenericType(i.Type);
                    if (!this.m_jobManager.IsJobRegistered(idt))
                    {
                        var matchJob = this.m_serviceManager.CreateInjected(idt) as IJob;
                        this.m_jobManager.AddJob(matchJob, JobStartType.TimerOnly);
                        if (!this.m_jobManager.GetJobSchedules(matchJob).Any())
                        {
                            this.m_jobManager.SetJobSchedule(matchJob, new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday }, DateTime.Now.Date);
                        }
                    }
                }

                this.m_mergeServices.Add(new SimBundleResourceInterceptor(this.m_mergeServices.OfType<ISimResourceInterceptor>()));
            }

            Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Stop the service
        /// </summary>
        public bool Stop()
        {
            Stopping?.Invoke(this, EventArgs.Empty);

            foreach (var s in m_mergeServices)
            {
                if (null == s)
                {
                    continue;
                }

                ApplicationServiceContext.Current.GetService<IServiceManager>()?.RemoveServiceProvider(s.GetType());
                s.Dispose();
            }
            m_mergeServices.Clear();

            Stopped?.Invoke(this, EventArgs.Empty);

            return true;
        }

        /// <inheritdoc/>
        public IDataManagedLinkProvider<T> GetLinkProvider<T>() where T : IdentifiedData => null;

        /// <inheritdoc/>
        public IDataManagedLinkProvider GetLinkProvider(Type forType) => null;
    }
}