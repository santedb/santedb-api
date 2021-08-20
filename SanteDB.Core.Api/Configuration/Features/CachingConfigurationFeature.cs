using SanteDB.Core.Configuration;
using SanteDB.Core.Configuration.Features;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Memory cache configuration feature
    /// </summary>
    public class CachingConfigurationFeature : IFeature
    {

        internal const string DATA_CACHE_SERVICE = "Data Object Cache";
        internal const string ADHOC_CACHE_SERVICE = "Ad-Hoc Cache";

        // Configuration feature        
        private GenericFeatureConfiguration m_configuration;

        /// <summary>
        /// Creates a new memory cache configuration feature
        /// </summary>
        public CachingConfigurationFeature()
        {

        }

        /// <summary>
        /// Gets or sets the configuration
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
        /// Gets the description of this feature
        /// </summary>
        public string Description => "Controls the caching of data which offloads traffic from the primary data store";

        /// <summary>
        /// Gets the flags for this feature
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.AutoSetup;

        /// <summary>
        /// Gets the group of the feature
        /// </summary>
        public string Group => FeatureGroup.Performance;

        /// <summary>
        /// Gets the name of the feature
        /// </summary>
        public string Name => "Caching";

        /// <summary>
        /// Create installation tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            yield return new InstallCacheServiceTask(this, this.m_configuration);
            yield return new InstallAdHocCacheServiceTask(this, this.m_configuration);
        }

        /// <summary>
        /// Create uninstall tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            yield return new UninstallCacheServiceTask(this, this.m_configuration);
            yield return new UninstallAdHocCacheServiceTask(this, this.m_configuration);
        }

        /// <summary>
        /// Query for the feature state of this feature
        /// </summary>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            Type[] cacheProviders = AppDomain.CurrentDomain.GetAllTypes().Where(o => typeof(IDataCachingService).IsAssignableFrom(o) && !o.IsAbstract && !o.IsInterface).ToArray(),
                adhocProvider = AppDomain.CurrentDomain.GetAllTypes().Where(o => typeof(IAdhocCacheService).IsAssignableFrom(o) && !o.IsAbstract && !o.IsInterface).ToArray();

            this.m_configuration = new GenericFeatureConfiguration();

            var appServiceConfigurationSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>()?.ServiceProviders;

            // Add the provider handlers
            var objCache = appServiceConfigurationSection.FirstOrDefault(t => typeof(IDataCachingService).IsAssignableFrom(t.Type));
            var adCache = appServiceConfigurationSection.FirstOrDefault(t => typeof(IAdhocCacheService).IsAssignableFrom(t.Type));

            // What we want to do is to setup defaults if possible
            if (objCache == null)
            {
                objCache = new TypeReferenceConfiguration(cacheProviders.FirstOrDefault(t => t.Name == "MemoryCacheService"));
            }
            if(adCache == null)
            {
                adCache = new TypeReferenceConfiguration(adhocProvider.FirstOrDefault(t => t.Name == "MemoryAdhocCacheService"));
            }

            this.m_configuration.Categories.Add("Services", new string[] { DATA_CACHE_SERVICE, ADHOC_CACHE_SERVICE });
            this.m_configuration.Values.Add(DATA_CACHE_SERVICE, objCache);
            this.m_configuration.Values.Add(ADHOC_CACHE_SERVICE, adCache);
            this.m_configuration.Options.Add(DATA_CACHE_SERVICE, () => cacheProviders);
            this.m_configuration.Options.Add(ADHOC_CACHE_SERVICE, () => adhocProvider);

            // Add the distinct configuration sections
            foreach (var ct in cacheProviders.Union(adhocProvider).Select(o => o.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration).Distinct())
            {
                if (ct == null) continue; // no configuration available

                // Configuration for feature
                var categoryName = $"{ct.Name}";
                if (!this.m_configuration.Values.ContainsKey(categoryName))
                {
                    this.m_configuration.Options.Add(categoryName, () => ConfigurationOptionType.Object);
                    this.m_configuration.Values.Add(categoryName, configuration.GetSection(ct) ?? Activator.CreateInstance(ct));
                }
            }

           
            return objCache != null && adCache != null ? FeatureInstallState.Installed : objCache != null || adCache != null ? FeatureInstallState.PartiallyInstalled : FeatureInstallState.NotInstalled;
        }
    }

    /// <summary>
    /// Install the ad-hoc cache service
    /// </summary>
    internal class InstallAdHocCacheServiceTask : IConfigurationTask
    {
        // Configuration for the feature
        private Type m_cacheType;
        // Configuration
        private object m_configuration;

        /// <summary>
        /// Install the ad-hoc cache service
        /// </summary>
        public InstallAdHocCacheServiceTask(CachingConfigurationFeature cachingConfigurationFeature, GenericFeatureConfiguration configuration)
        {
            this.Feature = cachingConfigurationFeature;
            configuration.Values.TryGetValue(CachingConfigurationFeature.ADHOC_CACHE_SERVICE, out object cacheType);
            m_cacheType = ((TypeReferenceConfiguration)cacheType).Type;
            var configType = this.m_cacheType.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration;
            this.m_configuration = configuration.Values.FirstOrDefault(t => t.Value.GetType() == configType);

        }

        /// <summary>
        /// Gets a description of the feature
        /// </summary>
        public string Description => "Installs the selected ad-hoc cache service into the core service layer";

        /// <summary>
        /// Gets the feature on which this task is attached
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the name of this task
        /// </summary>
        public string Name => "Install Ad-Hoc Cache";

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the specified task configuration
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {
            // First - remove the service instance
            var appService = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            var typeConfiguration = appService.ServiceProviders.Find(o => typeof(IAdhocCacheService).IsAssignableFrom(o.Type));

            // Next we remove the existing configuration
            if (typeConfiguration != null)
            {
                appService.ServiceProviders.Remove(typeConfiguration);
            }

            // Then we add our own service
            appService.ServiceProviders.Add(new TypeReferenceConfiguration(this.m_cacheType));

            // Next remove the old configuration section
            var configType = typeConfiguration.Type.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration;
            if (configType != null)
            {
                configuration.RemoveSection(configType);
            }
            // Then we add our own configuration
            configuration.AddSection(this.m_configuration);
            return true;
        }

        /// <summary>
        /// Rollback the confguration
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Verify the state of this configuration file
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration) => true;
    }

    /// <summary>
    /// Install the data cache service task
    /// </summary>
    internal class InstallCacheServiceTask : IConfigurationTask
    {
        // Configuration for the feature
        private Type m_cacheType;
        // Configuration
        private object m_configuration;

        /// <summary>
        /// Install the ad-hoc cache service
        /// </summary>
        public InstallCacheServiceTask(CachingConfigurationFeature cachingConfigurationFeature, GenericFeatureConfiguration configuration)
        {
            this.Feature = cachingConfigurationFeature;
            configuration.Values.TryGetValue(CachingConfigurationFeature.DATA_CACHE_SERVICE, out object cacheType);
            m_cacheType = ((TypeReferenceConfiguration)cacheType).Type;
            var configType = this.m_cacheType.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration;
            this.m_configuration = configuration.Values.FirstOrDefault(t => t.Value?.GetType() == configType);

        }

        /// <summary>
        /// Gets a description of the feature
        /// </summary>
        public string Description => "Installs the selected data cache service into the core service layer";

        /// <summary>
        /// Gets the feature on which this task is attached
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the name of this task
        /// </summary>
        public string Name => "Install Data Cache";

        /// <summary>
        /// Progress has changed
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the specified task configuration
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {
            // First - remove the service instance
            var appService = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            var typeConfiguration = appService.ServiceProviders.Find(o => typeof(IAdhocCacheService).IsAssignableFrom(o.Type));

            // Next we remove the existing configuration
            if (typeConfiguration != null)
            {
                appService.ServiceProviders.Remove(typeConfiguration);
            }

            // Then we add our own service
            appService.ServiceProviders.Add(new TypeReferenceConfiguration(this.m_cacheType));

            // Next remove the old configuration section
            var configType = typeConfiguration.Type.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration;
            if (configType != null)
            {
                configuration.RemoveSection(configType);
            }
            // Then we add our own configuration
            configuration.AddSection(this.m_configuration);
            return true;
        }

        /// <summary>
        /// Rollback the confguration
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Verify the state of this configuration file
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration) => true;
    }

    /// <summary>
    /// Removes the ad-hoc cache service
    /// </summary>
    internal class UninstallAdHocCacheServiceTask : IConfigurationTask
    {

        // Configuration for the feature
        private Type m_cacheType;

        /// <summary>
        /// Un-install the ad-hoc caching service
        /// </summary>
        public UninstallAdHocCacheServiceTask(CachingConfigurationFeature cachingConfigurationFeature, GenericFeatureConfiguration configuration)
        {
            this.Feature = cachingConfigurationFeature;
            configuration.Values.TryGetValue(CachingConfigurationFeature.ADHOC_CACHE_SERVICE, out object cacheType);
            m_cacheType = ((TypeReferenceConfiguration)cacheType).Type;

        }

        /// <summary>
        /// Gets the description of the installation task
        /// </summary>
        public string Description => "Removes the ad-hoc caching service and the related configuration";

        /// <summary>
        /// Gets the feature to which this task is bound
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Get the name of this task
        /// </summary>
        public string Name => "Remove Ad-hoc Cache Service";

        /// <summary>
        /// Fired when the progress changes
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the installation task
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {

            // First - remove the service instance
            var appService = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            var typeConfiguration = appService.ServiceProviders.Find(o => o.Type == this.m_cacheType);
            if (typeConfiguration != null)
            {
                appService.ServiceProviders.Remove(typeConfiguration);
            }

            // Next remove the configuration section
            var configType = this.m_cacheType.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration;
            if (configType != null)
            {
                configuration.RemoveSection(configType);
            }
            return true;
        }

        /// <summary>
        /// Rollback the installation
        /// </summary>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Verify the state of the configuration
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration) => this.m_cacheType != null;
    }

    /// <summary>
    /// Uninstall the data caching service task
    /// </summary>
    internal class UninstallCacheServiceTask : IConfigurationTask
    {
        // Configuration for the feature
        private Type m_cacheType;

        /// <summary>
        /// Removes the data cache service
        /// </summary>
        public UninstallCacheServiceTask(CachingConfigurationFeature cachingConfigurationFeature, GenericFeatureConfiguration configuration)
        {
            this.Feature = cachingConfigurationFeature;
            configuration.Values.TryGetValue(CachingConfigurationFeature.DATA_CACHE_SERVICE, out object cacheType);
            m_cacheType = ((TypeReferenceConfiguration)cacheType).Type;

        }

        /// <summary>
        /// Gets the description of the feature
        /// </summary>
        public string Description => "Removes the Data Object Cache service from the server configuration";

        /// <summary>
        /// Gets the feature
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Get the name of the task
        /// </summary>
        public string Name => "Remove Data Object Cache";

        /// <summary>
        /// Fired when progress changes
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <summary>
        /// Execute the process
        /// </summary>
        public bool Execute(SanteDBConfiguration configuration)
        {
            // First - remove the service instance
            var appService = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            var typeConfiguration = appService.ServiceProviders.Find(o => o.Type == this.m_cacheType);
            if (typeConfiguration != null)
            {
                appService.ServiceProviders.Remove(typeConfiguration);
            }

            // Next remove the configuration section
            var configType = this.m_cacheType.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration;
            if (configType != null)
            {
                configuration.RemoveSection(configType);
            }
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
        /// Validate the service can be removed
        /// </summary>
        public bool VerifyState(SanteDBConfiguration configuration) => this.m_cacheType != null;
    }


}
