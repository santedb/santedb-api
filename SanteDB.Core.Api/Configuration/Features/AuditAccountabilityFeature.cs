/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using System;
using System.Collections.Generic;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Services;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents a feature for the audit and accountability framework
    /// </summary>
    public class AuditAccountabilityFeature : IFeature, IConfigurationTask
    {
	    /// <summary>
        /// Gets or sets the configuration
        /// </summary>
        public object Configuration { get; set; }

	    /// <summary>
        /// Gets the type of configuration
        /// </summary>
        public Type ConfigurationType => typeof(AuditAccountabilityConfigurationSection);

	    /// <summary>
        /// Create the installation tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[] { this };
        }

	    /// <summary>
        /// Create uninstall tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            throw new NotSupportedException();
        }

	    /// <summary>
        /// Gets the description of the feature
        /// </summary>
        public string Description => "Controls the auditing and accountability framework found within the SanteDB server";

	    /// <summary>
        /// Execute the installation task
        /// </summary>
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
                    SourceInformation = new AuditSourceConfiguration() { 
                        EnterpriseSite = Environment.MachineName,
                        SiteLocation = "DEFAULT"
                    }

	            };
            }



            configuration.AddSection(this.Configuration);
            return true;
        }

	    /// <summary>
        /// Gets the feature this is configuring
        /// </summary>
        public IFeature Feature => this;

	    /// <summary>
        /// Gets or sets the flags of the feature
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.AlwaysConfigure | FeatureFlags.AutoSetup | FeatureFlags.SystemFeature;

	    /// <summary>
        /// Gets the group
        /// </summary>
        public string Group => FeatureGroup.Security;

	    /// <summary>
        /// Gets the name of the feature
        /// </summary>
        public string Name => "Audit and Accountability";

	    /// <summary>
        /// Fired when progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

	    /// <summary>
        /// Query the state of the feature
        /// </summary>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            

            return configuration.GetSection<AuditAccountabilityConfigurationSection>() != null ? FeatureInstallState.Installed : FeatureInstallState.NotInstalled;
        }

	    /// <summary>
        /// Rollback the task
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            // Configure the option
            if (configuration.GetSection<AuditAccountabilityConfigurationSection>() != null)
            {
	            configuration.RemoveSection<AuditAccountabilityConfigurationSection>();
            }

            return true;
        }

	    /// <summary>
        /// Verify that this can be configured
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            return true;
        }
    }
}
