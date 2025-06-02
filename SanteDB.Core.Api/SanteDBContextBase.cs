/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using System;
using System.Diagnostics;
using System.Linq;
#pragma warning disable CS0612

namespace SanteDB.Core
{
    /// <summary>
    /// A basic service context upon which other service contexts may be instantiated
    /// </summary>
    public abstract class SanteDBContextBase : IApplicationServiceContext, IDisposable
    {

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(SanteDBContextBase));

        // Service proider
        private DependencyServiceManager m_serviceProvider = new DependencyServiceManager();

        /// <summary>
        /// Gets the identifier for this context
        /// </summary>
        public Guid ActivityUuid { get; protected set; }

        /// <summary>
        /// Gets the start time of the applicaton
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Application name
        /// </summary>
        public virtual String ApplicationName => "SanteDB";

        /// <summary>
        /// Gets whether the domain is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets the host type
        /// </summary>
        public SanteDBHostType HostType { get; private set; }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public virtual string ServiceName => "Core Service Context";

        /// <summary>
        /// Dependency service manager
        /// </summary>
        protected DependencyServiceManager DependencyServiceManager => this.m_serviceProvider;

        /// <summary>
        /// Creates a new instance of the host context
        /// </summary>
        protected SanteDBContextBase(SanteDBHostType hostEnvironment, IConfigurationManager configurationManager)
        {
            this.ActivityUuid = Guid.NewGuid();
            this.HostType = hostEnvironment;
            this.m_serviceProvider.AddServiceProvider(configurationManager);
            
        }

        /// <summary>
        /// Add service provider
        /// </summary>
        protected void AddServiceProvider(Type serviceType) => this.m_serviceProvider.AddServiceProvider(serviceType);

        #region IServiceProvider Members

        /// <summary>
        /// Fired when the application context starting
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// Fired after application startup is complete
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Fired wehn the application context commences stop
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// Fired after the appplication context is stopped
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Start the application context
        /// </summary>
        public virtual void Start()
        {
            if (!this.IsRunning)
            {
                Stopwatch startWatch = new Stopwatch();

                using (AuthenticationContext.EnterSystemContext())
                {
                    try
                    {
                        startWatch.Start();

                        if (this.Starting != null)
                        {
                            this.Starting(this, null);
                        }

                        // If there is no configuration manager then add the local
                        Trace.TraceInformation("STAGE0 START: Load Configuration");

                        // Assign diagnostics
                        var config = this.GetService<IConfigurationManager>().GetSection<DiagnosticsConfigurationSection>();

                        if (config != null)
                        {
                            foreach (var writer in config.TraceWriter)
                            {
                                Tracer.AddWriter(Activator.CreateInstance(writer.TraceWriter, writer.Filter, writer.InitializationData, (writer.Sources?.Any() == true ? writer.Sources : config.Sources).ToDictionary(o => o.SourceName, o => o.Filter)) as TraceWriter, writer.Filter);
                            }
                        }
#if DEBUG
                        else
                        {
                            Tracer.AddWriter(new SanteDB.Core.Diagnostics.Tracing.SystemDiagnosticsTraceWriter(), System.Diagnostics.Tracing.EventLevel.LogAlways);
                        }
#endif

                        Trace.TraceInformation("STAGE1 START: Start Dependency Injection Manager");
                        this.m_serviceProvider.AddServiceProvider(this);
                        this.m_serviceProvider.Start();

                        Trace.TraceInformation("STAGE2 START: Notify start");
                        this.Started?.Invoke(this, EventArgs.Empty);
                        this.StartTime = DateTime.Now;

                        this.GetAuditService().Audit().ForApplicationStart().Send();

                        Trace.TraceInformation("SanteDB startup completed successfully in {0} ms...", startWatch.ElapsedMilliseconds);
                    }
                    catch (Exception e)
                    {
                        m_tracer.TraceError("Error starting up context: {0}", e);
                        this.IsRunning = false;
                        Trace.TraceError("Server is running in Maintenance Mode due to error {0}...", e.Message);
                        throw new InvalidOperationException(ErrorMessages.CANNOT_INITIALIZE_APPLICATION, e);
                    }
                    finally
                    {
                        startWatch.Stop();
                    }
                }
                this.IsRunning = true;
            }
        }

        /// <summary>
        /// Stop the application context
        /// </summary>
        public virtual void Stop()
        {
            if (this.Stopping != null)
            {
                this.Stopping(this, EventArgs.Empty);
            }

            if (this.IsRunning)
            {
                this.GetAuditService().Audit().ForApplicationStop().Send();
            }

            this.IsRunning = false;
            this.m_serviceProvider.Stop();

            if (this.Stopped != null)
            {
                this.Stopped(this, EventArgs.Empty);
            }

            this.Dispose();
        }

        /// <summary>
        /// Get a service from this host context
        /// </summary>
        public object GetService(Type serviceType) => this.m_serviceProvider.GetService(serviceType);

        #endregion IServiceProvider Members

        #region IDisposable Members

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            this.m_serviceProvider.Dispose();
            Tracer.DisposeWriters();
        }

        #endregion IDisposable Members

    }
}
#pragma warning restore
