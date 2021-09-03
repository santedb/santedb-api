/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-17
 */
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
        internal const string QUERY_STATE_SERVICE = "Stateful Query Cache";

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
        public FeatureFlags Flags => FeatureFlags.AutoSetup ;

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
            yield return new InstallCacheServiceTask(this, this.m_configuration, QUERY_STATE_SERVICE);
            yield return new InstallCacheServiceTask(this, this.m_configuration, DATA_CACHE_SERVICE);
            yield return new InstallCacheServiceTask(this, this.m_configuration, ADHOC_CACHE_SERVICE);
        }

        /// <summary>
        /// Create uninstall tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            yield return new UninstallCacheServiceTask(this, this.m_configuration, QUERY_STATE_SERVICE);
            yield return new UninstallCacheServiceTask(this, this.m_configuration, DATA_CACHE_SERVICE);
            yield return new UninstallCacheServiceTask(this, this.m_configuration, ADHOC_CACHE_SERVICE);
        }

        /// <summary>
        /// Query for the feature state of this feature
        /// </summary>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            Type[] cacheProviders = AppDomain.CurrentDomain.GetAllTypes().Where(o => typeof(IDataCachingService).IsAssignableFrom(o) && !o.IsAbstract && !o.IsInterface).ToArray(),
                adhocProvider = AppDomain.CurrentDomain.GetAllTypes().Where(o => typeof(IAdhocCacheService).IsAssignableFrom(o) && !o.IsAbstract && !o.IsInterface).ToArray(),
                queryProvider = AppDomain.CurrentDomain.GetAllTypes().Where(o => typeof(IQueryPersistenceService).IsAssignableFrom(o) && !o.IsAbstract && !o.IsInterface).ToArray();

            this.m_configuration = new GenericFeatureConfiguration();

            var appServiceConfigurationSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>()?.ServiceProviders;

            // Add the provider handlers
            TypeReferenceConfiguration objCache = appServiceConfigurationSection.FirstOrDefault(t => typeof(IDataCachingService).IsAssignableFrom(t.Type)),
                adCache = appServiceConfigurationSection.FirstOrDefault(t => typeof(IAdhocCacheService).IsAssignableFrom(t.Type)),
                qrCache = appServiceConfigurationSection.FirstOrDefault(t => typeof(IQueryPersistenceService).IsAssignableFrom(t.Type)),
                originalCache = objCache,
                originalAdCache = adCache,
                originalQuery = qrCache;

            // What we want to do is to setup defaults if possible
            if (objCache == null)
            {
                objCache = new TypeReferenceConfiguration(cacheProviders.FirstOrDefault(t => t.Name == "MemoryCacheService"));
            }
            if(adCache == null)
            {
                adCache = new TypeReferenceConfiguration(adhocProvider.FirstOrDefault(t => t.Name == "MemoryAdhocCacheService"));
            }
            if (qrCache == null)
            {
                qrCache = new TypeReferenceConfiguration(queryProvider.FirstOrDefault(t => t.Name == "MemoryQueryPersistenceService"));
            }

            this.m_configuration.Categories.Add("Services", new string[] { DATA_CACHE_SERVICE, ADHOC_CACHE_SERVICE, QUERY_STATE_SERVICE });
            this.m_configuration.Values.Add(DATA_CACHE_SERVICE, objCache.Type);
            this.m_configuration.Values.Add(ADHOC_CACHE_SERVICE, adCache.Type);
            this.m_configuration.Values.Add(QUERY_STATE_SERVICE, qrCache.Type);
            this.m_configuration.Options.Add(DATA_CACHE_SERVICE, () => cacheProviders);
            this.m_configuration.Options.Add(ADHOC_CACHE_SERVICE, () => adhocProvider);
            this.m_configuration.Options.Add(QUERY_STATE_SERVICE, () => queryProvider);

            // Add the distinct configuration sections
            foreach (var ct in cacheProviders.Union(adhocProvider).Union(queryProvider).Select(o => o.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration).Distinct())
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

           
            return originalCache != null && originalAdCache != null && originalQuery != null ? FeatureInstallState.Installed : originalQuery != null || originalCache != null || originalAdCache != null ? FeatureInstallState.PartiallyInstalled : FeatureInstallState.NotInstalled;
        }
    }

    /// <summary>
    /// Uninstall the data caching service task
    /// </summary>
    internal class UninstallCacheServiceTask : IConfigurationTask
    {
        // Configuration for the feature
        private Type m_cacheType;

        // configuration name
        private string m_configurationName;

        /// <summary>
        /// Removes the data cache service
        /// </summary>
        public UninstallCacheServiceTask(CachingConfigurationFeature cachingConfigurationFeature, GenericFeatureConfiguration configuration, string configurationName)
        {
            this.Feature = cachingConfigurationFeature;
            this.m_configurationName = configurationName;
            configuration.Values.TryGetValue(this.m_configurationName, out object cacheType);
            m_cacheType = (Type)cacheType;
        }

        /// <summary>
        /// Gets the description of the feature
        /// </summary>
        public string Description => $"Remove {this.m_configurationName}";

        /// <summary>
        /// Gets the feature
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Get the name of the task
        /// </summary>
        public string Name => $"Remove {this.m_configurationName}";

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
            var typeConfiguration = appService.ServiceProviders.Find(o => typeof(IDataCachingService).IsAssignableFrom(o.Type) && o.Type != this.m_cacheType);

            // Next we remove the existing configuration
            appService.ServiceProviders.RemoveAll(o => typeof(IDataCachingService).IsAssignableFrom(o.Type));

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


    /// <summary>
    /// Install the data cache service task
    /// </summary>
    internal class InstallCacheServiceTask : IConfigurationTask
    {
        // Configuration for the feature
        private Type m_cacheType;
        // Configuration
        private object m_configuration;
        // Configuration name
        private String m_configurationName;
        // Alternate options
        private Type[] m_alternateOptions;

        /// <summary>
        /// Install the ad-hoc cache service
        /// </summary>
        public InstallCacheServiceTask(CachingConfigurationFeature cachingConfigurationFeature, GenericFeatureConfiguration configuration, String configurationName)
        {
            this.m_configurationName = configurationName;
            this.Feature = cachingConfigurationFeature;
            configuration.Values.TryGetValue(this.m_configurationName, out object cacheType);
            m_cacheType = (Type)cacheType;
            var configType = this.m_cacheType.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration;
            this.m_configuration = configuration.Values.FirstOrDefault(t => t.Value?.GetType() == configType).Value;
            this.m_alternateOptions = configuration.Options[this.m_configurationName]() as Type[];
        }

        /// <summary>
        /// Gets a description of the feature
        /// </summary>
        public string Description => $"Install {this.m_configurationName}";

        /// <summary>
        /// Gets the feature on which this task is attached
        /// </summary>
        public IFeature Feature { get; }

        /// <summary>
        /// Gets the name of this task
        /// </summary>
        public string Name => $"Install {this.m_configurationName}";

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
            var typeConfiguration = appService.ServiceProviders.Find(o => this.m_alternateOptions.Contains(o.Type) && o.Type != this.m_cacheType);

            // Next we remove the existing configuration
            appService.ServiceProviders.RemoveAll(o => this.m_alternateOptions.Contains(o.Type));

            // Then we add our own service
            appService.ServiceProviders.Add(new TypeReferenceConfiguration(this.m_cacheType));

            // Next remove the old configuration section
            var configType = typeConfiguration?.Type.GetCustomAttribute<ServiceProviderAttribute>()?.Configuration;
            if (configType != null)
            {
                configuration.RemoveSection(configType);
            }
            // Then we add our own configuration
            if (this.m_configuration != null)
            {
                configuration.AddSection(this.m_configuration);
            }
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

}
