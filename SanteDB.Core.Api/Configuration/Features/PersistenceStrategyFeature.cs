using SanteDB.Core.Data;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents the persistence strategy feature - which is optional
    /// </summary>
    public class PersistenceStrategyFeature : IFeature
    {

        /// <summary>
        /// The name of the resource name
        /// </summary>
        public const string RESOURCE_MANAGER_NAME = "Resource Manager";
        /// <summary>
        /// Manager configuration
        /// </summary>
        public const string RESOURCE_MERGE_CONFIG = "Manager Configuration";

        // The configuration reference
        private GenericFeatureConfiguration m_configuration;

        /// <summary>
        /// Gets the configuration for the object
        /// </summary>
        public object Configuration
        {
            get => this.m_configuration;
            set => this.m_configuration = value as GenericFeatureConfiguration;
        }

        /// <summary>
        /// Gets the configuration type
        /// </summary>
        public Type ConfigurationType => typeof(GenericFeatureConfiguration);

        /// <summary>
        /// Gets the description of the feature
        /// </summary>
        public string Description => "Sets a resource manager which controls the persisting and linking of objects. None is recommended unless you require central registry functions.";

        /// <summary>
        /// Gets the flags for this option
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.None;

        /// <summary>
        /// Gets the group for this feature
        /// </summary>
        public string Group => FeatureGroup.Persistence;

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name => "Resource Management";

        /// <summary>
        /// Create the installation tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            yield return new InstallPersistenceStrategyTask(this, this.m_configuration);
        }

        /// <summary>
        /// Create the uninstall tasks
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            yield return new UninstallPersistenceStrategyTask(this);
        }

        /// <summary>
        /// Query for the current state of this feature
        /// </summary>
        /// <param name="configuration">The configuration on which to check the feature state</param>
        /// <returns>The install status of the feature</returns>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {

            var appService = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            var currentStrategy = appService.ServiceProviders.Find(o => typeof(IDataManagementPattern).IsAssignableFrom(o.Type));

            // Resource manager config section
            var resourceMergeConfiguration = configuration.GetSection<ResourceManagementConfigurationSection>();
            if (resourceMergeConfiguration == null)
            {
                resourceMergeConfiguration = new ResourceManagementConfigurationSection();
            }

            // Get strategies
            this.m_configuration = new GenericFeatureConfiguration();
            this.m_configuration.Options.Add(RESOURCE_MANAGER_NAME, () => AppDomain.CurrentDomain.GetAllTypes().Where(o => typeof(IDataManagementPattern).IsAssignableFrom(o)).ToArray());
            this.m_configuration.Options.Add(RESOURCE_MERGE_CONFIG, () => ConfigurationOptionType.Object);
            this.m_configuration.Values.Add(RESOURCE_MANAGER_NAME, currentStrategy?.Type);
            this.m_configuration.Values.Add(RESOURCE_MERGE_CONFIG, resourceMergeConfiguration);

            return currentStrategy == null ? FeatureInstallState.NotInstalled : FeatureInstallState.Installed;
        }
    }

    /// <summary>
    /// Uninstall the persistence strategy
    /// </summary>
    internal class UninstallPersistenceStrategyTask : IConfigurationTask
    {
        /// <summary>
        /// Generate new persistnece strategy feature
        /// </summary>
        public UninstallPersistenceStrategyTask(PersistenceStrategyFeature persistenceStrategyFeature)
        {
            this.Feature = persistenceStrategyFeature;
        }

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description => "Removes Resource Persistence management strategy configuration, and the service";

        /// <summary>
        /// Gets the feature to which this belongs
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the name of the feature
        /// </summary>
        public string Name => "Remove Resource Manager";

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the removal procedure
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {

            var appSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            appSection.ServiceProviders.RemoveAll(o => typeof(IDataManagementPattern).IsAssignableFrom(o.Type));
            configuration.RemoveSection<ResourceManagementConfigurationSection>();
            return true;

        }

        /// <summary>
        /// Rollback the configuration
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Verify the state
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            var appSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            return appSection.ServiceProviders.Any(o => typeof(IDataManagementPattern).IsAssignableFrom(o.Type));
        }
    }

    /// <summary>
    /// Install the persistence strategy task
    /// </summary>
    internal class InstallPersistenceStrategyTask : IConfigurationTask
    {

        // The resource merge
        private GenericFeatureConfiguration m_resourceMergeConfiguration;

        /// <summary>
        /// Creates the new installation persistence task
        /// </summary>
        public InstallPersistenceStrategyTask(PersistenceStrategyFeature persistenceStrategyFeature, GenericFeatureConfiguration resourceMergeConfiguration)
        {
            this.Feature = persistenceStrategyFeature;
            this.m_resourceMergeConfiguration = resourceMergeConfiguration;
        }

        /// <summary>
        /// Configures the description
        /// </summary>
        public string Description => $"Configures the resource management strategy to {this.m_resourceMergeConfiguration.Values[PersistenceStrategyFeature.RESOURCE_MANAGER_NAME]}";

        /// <summary>
        /// Gets the feature which controls 
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the name of the feature
        /// </summary>
        public string Name => $"Install Resource Manager";

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the configuration 
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {
            var appSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>();

            appSection.ServiceProviders.RemoveAll(o => typeof(IDataManagementPattern).IsAssignableFrom(o.Type));
            appSection.ServiceProviders.Add(new TypeReferenceConfiguration(this.m_resourceMergeConfiguration.Values[PersistenceStrategyFeature.RESOURCE_MANAGER_NAME] as Type));
            configuration.RemoveSection<ResourceManagementConfigurationSection>();
            configuration.AddSection(this.m_resourceMergeConfiguration.Values[PersistenceStrategyFeature.RESOURCE_MERGE_CONFIG]);
            return true;
        }

        /// <summary>
        /// Rollback the configuration
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Verify state
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration) => true;
    }
}
