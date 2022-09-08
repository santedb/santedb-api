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
using System;
using System.Collections.Generic;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Services;

#pragma warning disable  CS1587
/// <summary>
/// The Features namespace in the core API is used to define the core configuration features (to be shown in the configuration tooling) for the
/// SanteDB instance.
/// </summary>
#pragma warning restore  CS1587

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// An implementation of the <see cref="IFeature"/> interface for the auditing and accountability panel
    /// </summary>
    public class AuditAccountabilityFeature : IFeature, IConfigurationTask
    {
        /// <inheritdoc/>
        public object Configuration { get; set; }

        /// <inheritdoc/>
        public Type ConfigurationType => typeof(AuditAccountabilityConfigurationSection);

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[] { this };
        }

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public string Description => "Controls the auditing and accountability framework found within the SanteDB server";

        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            // Configure the option
            if (configuration.GetSection<AuditAccountabilityConfigurationSection>() != null)
            {
                configuration.RemoveSection<AuditAccountabilityConfigurationSection>();
            }

            if (this.Configuration == null)
            {
                this.Configuration = new AuditAccountabilityConfigurationSection
                {
                    CompleteAuditTrail = true,
                    AuditFilters = new List<AuditFilterConfiguration>
                    {
                        new AuditFilterConfiguration(ActionType.Execute, EventIdentifierType.NetworkActivity | EventIdentifierType.SecurityAlert, OutcomeIndicator.Success, false, false),
                        new AuditFilterConfiguration(ActionType.Create | ActionType.Read | ActionType.Update | ActionType.Delete, null, null, true, true)
                    },
                    SourceInformation = new AuditSourceConfiguration()
                    {
                        EnterpriseSite = Environment.MachineName,
                        SiteLocation = "DEFAULT"
                    }
                };
            }

            configuration.AddSection(this.Configuration);
            return true;
        }

        /// <inheritdoc/>
        public IFeature Feature => this;

        /// <inheritdoc/>
        public FeatureFlags Flags => FeatureFlags.AlwaysConfigure | FeatureFlags.AutoSetup | FeatureFlags.SystemFeature;

        /// <inheritdoc/>
        public string Group => FeatureGroup.Security;

        /// <inheritdoc/>
        public string Name => "Audit and Accountability";

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            return configuration.GetSection<AuditAccountabilityConfigurationSection>() != null ? FeatureInstallState.Installed : FeatureInstallState.NotInstalled;
        }

        /// <inheritdoc/>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            // Configure the option
            if (configuration.GetSection<AuditAccountabilityConfigurationSection>() != null)
            {
                configuration.RemoveSection<AuditAccountabilityConfigurationSection>();
            }

            return true;
        }

        /// <inheritdoc/>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            return true;
        }
    }
}