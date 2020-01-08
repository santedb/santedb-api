using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SanteDB.Core.Services;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents a feature for the audit and accountability framework
    /// </summary>
    public class AuditAccountabilityFeature : IFeature, IConfigurationTask
    {
        /// <summary>
        /// Gets the name of the feature
        /// </summary>
        public string Name => "Audit and Accountability";

        /// <summary>
        /// Gets the description of the feature
        /// </summary>
        public string Description => "Controls the auditing and accountability framework found within the SanteDB server";

        /// <summary>
        /// Gets the group
        /// </summary>
        public string Group => FeatureGroup.Security;

        /// <summary>
        /// Gets the type of configuration
        /// </summary>
        public Type ConfigurationType => typeof(AuditAccountabilityConfigurationSection);

        /// <summary>
        /// Gets or sets the configuration
        /// </summary>
        public object Configuration { get; set; }

        /// <summary>
        /// Gets or sets the flags of the feature
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.AlwaysConfigure | FeatureFlags.AutoSetup | FeatureFlags.SystemFeature;

        /// <summary>
        /// Gets the feature this is configuring
        /// </summary>
        public IFeature Feature => this;

        /// <summary>
        /// Fired when progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

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
        /// Execute the installation task
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {
            // Configure the option
            if (configuration.GetSection<AuditAccountabilityConfigurationSection>() != null)
                configuration.RemoveSection<AuditAccountabilityConfigurationSection>();
            if (this.Configuration == null)
                this.Configuration = new AuditAccountabilityConfigurationSection()
                {
                    CompleteAuditTrail = true,
                    AuditFilters = new List<AuditFilterConfiguration>()
                    {
                        new AuditFilterConfiguration(Auditing.ActionType.Execute, Auditing.EventIdentifierType.NetworkActivity | Auditing.EventIdentifierType.SecurityAlert, Auditing.OutcomeIndicator.Success, false, false),
                        new AuditFilterConfiguration(Auditing.ActionType.Create | Auditing.ActionType.Read | Auditing.ActionType.Update | Auditing.ActionType.Delete, null, null, true, true)
                    }
                };
            configuration.AddSection(this.Configuration);
            return true;
        }

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
                configuration.RemoveSection<AuditAccountabilityConfigurationSection>();
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
