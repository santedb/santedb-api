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
 * Date: 2023-3-10
 */
using SanteDB.Core.Data;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// A <see cref="IFeature"/> which configures the appropriate persistence strategy (SIM, MDM, etc.)
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

        /// <inheritdoc/>
        public object Configuration
        {
            get => this.m_configuration;
            set => this.m_configuration = value as GenericFeatureConfiguration;
        }

        /// <inheritdoc/>
        public Type ConfigurationType => typeof(GenericFeatureConfiguration);

        /// <inheritdoc/>
        public string Description => "Sets a resource manager which controls the persisting and linking of objects. None is recommended unless you require central registry functions.";

        /// <inheritdoc/>
        public FeatureFlags Flags => FeatureFlags.None;

        /// <inheritdoc/>
        public string Group => FeatureGroup.Persistence;

        /// <inheritdoc/>
        public string Name => "Resource Management";

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            yield return new InstallPersistenceStrategyTask(this, this.m_configuration);
        }

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            yield return new UninstallPersistenceStrategyTask(this);
        }

        /// <inheritdoc/>
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
    /// Uninstall the persistence strategy from the current CDR
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

        /// <inheritdoc/>

        public string Description => "Removes Resource Persistence management strategy configuration, and the service";

        /// <inheritdoc/>
        public IFeature Feature { get; }

        /// <inheritdoc/>
        public string Name => "Remove Resource Manager";

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            var appSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            appSection.ServiceProviders.RemoveAll(o => typeof(IDataManagementPattern).IsAssignableFrom(o.Type));
            configuration.RemoveSection<ResourceManagementConfigurationSection>();
            return true;
        }

        /// <inheritdoc/>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            var appSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
            return appSection.ServiceProviders.Any(o => typeof(IDataManagementPattern).IsAssignableFrom(o.Type));
        }
    }

    /// <summary>
    /// Install the persistence strategy into the CDR
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

        /// <inheritdoc/>

        public string Description => $"Configures the resource management strategy to {this.m_resourceMergeConfiguration.Values[PersistenceStrategyFeature.RESOURCE_MANAGER_NAME]}";

        /// <inheritdoc/>
        public IFeature Feature { get; }

        /// <inheritdoc/>
        public string Name => $"Install Resource Manager";

        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            var appSection = configuration.GetSection<ApplicationServiceContextConfigurationSection>();

            appSection.ServiceProviders.RemoveAll(o => typeof(IDataManagementPattern).IsAssignableFrom(o.Type));
            appSection.ServiceProviders.Add(new TypeReferenceConfiguration(this.m_resourceMergeConfiguration.Values[PersistenceStrategyFeature.RESOURCE_MANAGER_NAME] as Type));
            configuration.RemoveSection<ResourceManagementConfigurationSection>();
            configuration.AddSection(this.m_resourceMergeConfiguration.Values[PersistenceStrategyFeature.RESOURCE_MERGE_CONFIG]);
            return true;
        }

        /// <inheritdoc/>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public bool VerifyState(SanteDBConfiguration configuration) => true;
    }
}