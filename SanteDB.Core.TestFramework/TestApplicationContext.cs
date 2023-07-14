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
using SanteDB.Core.Data;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Diagnostics.Tracing;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Services.Impl;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace SanteDB.Core.TestFramework
{
    /// <summary>
    /// Represents the test context
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TestApplicationContext : SanteDBContextBase
    {
        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(TestApplicationContext));

        /// <summary>
        /// Gets the service name
        /// </summary>
        public override string ServiceName => "SanteDB Test Service Context";

        /// <summary>
        /// Gets or set sthe test assembly
        /// </summary>
        public static Assembly TestAssembly { get; set; }

        /// <summary>
        /// Application context name
        /// </summary>
        public override string ApplicationName => "TEST";

        /// <summary>
        /// Creastes a new test context
        /// </summary>
        private TestApplicationContext() : base(SanteDBHostType.Test, new TestConfigurationService())
        {
            this.ActivityUuid = Guid.NewGuid();

            EntitySource.Current = new EntitySource(new PersistenceEntitySource());

            this.AddServiceProvider(typeof(TestLocalizationService));
            this.AddServiceProvider(typeof(DefaultThreadPoolService));
            this.AddServiceProvider(typeof(SanteDB.Core.Security.SHA256PasswordHashingService));
            this.AddServiceProvider(typeof(SanteDB.Core.Security.DefaultPolicyDecisionService));
            this.AddServiceProvider(typeof(SanteDB.Core.Security.DefaultPolicyEnforcementService));
            this.AddServiceProvider(typeof(DefaultOperatingSystemService));
            this.AddServiceProvider(typeof(TestAuditService));
#if DEBUG
            if (Tracer.GetWriter<DebugDiagnosticsTraceWriter>() == null)
            {
                Tracer.AddWriter(new DebugDiagnosticsTraceWriter(), System.Diagnostics.Tracing.EventLevel.Informational);
            }
#endif

        }

        /// <summary>
        /// Initialize the test context
        /// </summary>
        /// <param name="deploymentDirectory"></param>
        public static void Initialize(String deploymentDirectory)
        {
            if (ApplicationServiceContext.Current != null)
            {
                return;
            }

            AppDomain.CurrentDomain.SetData(
               "DataDirectory",
               Path.Combine(deploymentDirectory, string.Empty));

            ServiceUtil.Start(Guid.NewGuid(), new TestApplicationContext());

        }

        /// <summary>
        /// Start the application context
        /// </summary>
        public override void Start()
        {
            if (!this.IsRunning)
            {
                Tracer.AddWriter(new SanteDB.Core.Diagnostics.Tracing.SystemDiagnosticsTraceWriter(), System.Diagnostics.Tracing.EventLevel.LogAlways);
            }
            base.Start();
        }

    }
}