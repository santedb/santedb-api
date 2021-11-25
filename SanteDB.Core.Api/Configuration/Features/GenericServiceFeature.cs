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
 * Date: 2021-8-5
 */

using SanteDB.Core.Attributes;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// An implementation of <see cref="IFeature"/> which configures <typeparamref name="TService"/>
    /// </summary>
    /// <typeparam name="TService">The type of service which this generic service feature base class is configuring</typeparam>
    /// <example>
    /// <code language="cs" title="Expose MyService in Configuration Tool">
    /// <![CDATA[
    ///
    ///     public class MyFeature : GenericServiceFeature<MyService> {
    ///         // Override the group you want your feature to appear in
    ///         public override string Group => FeatureGroup.System;
    ///         // Override the type of configuration
    ///         public override Type ConfigurationType => typeof(FileSystemQueueConfigurationSection);
    ///     }
    ///
    /// ]]>
    /// </code>
    /// </example>
    public abstract class GenericServiceFeature<TService> : IFeature
        where TService : IServiceImplementation
    {
        /// <summary>
        /// Create a generic service feature
        /// </summary>
        public GenericServiceFeature()
        {
            var instanceAtt = typeof(TService).GetCustomAttribute<ServiceProviderAttribute>();
            if (instanceAtt != null)
            {
                this.Name = instanceAtt?.Name;
                this.Description = instanceAtt?.Name;
            }
            else
            {
                this.Name = typeof(TService).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? typeof(TService).Name;
                this.Description = typeof(TService).GetCustomAttribute<DescriptionAttribute>()?.Description ?? typeof(TService).Name;
            }
            this.Group = typeof(TService).Assembly.GetCustomAttribute<PluginAttribute>()?.Group;
        }

        /// <inheritdoc/>
        public virtual object Configuration
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public abstract Type ConfigurationType { get; }

        /// <inheritdoc/>
        public virtual IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[] { new InstallTask(this) };
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            return new IConfigurationTask[] { new UninstallTask(this) };
        }

        /// <inheritdoc/>
        public virtual string Description { get; }

        /// <inheritdoc/>
        public virtual FeatureFlags Flags => typeof(TService).Assembly.GetCustomAttribute<PluginAttribute>()?.EnableByDefault == true ? FeatureFlags.AutoSetup : FeatureFlags.None;

        /// <inheritdoc/>
        public virtual string Group { get; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <inheritdoc/>
        public virtual FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            var isServiceInstalled = configuration.GetSection<ApplicationServiceContextConfigurationSection>()?.ServiceProviders.Any(o => o.Type == typeof(TService)) == true;
            // First, this configuration type is available
            if (this.ConfigurationType != null)
            {
                try
                {
                    var setConfiguration = configuration.GetSection(this.ConfigurationType);
                    if (setConfiguration != null) // Set the configuration from the file
                    {
                        this.Configuration = setConfiguration;
                    }
                    else
                    {
                        this.Configuration = this.GetDefaultConfiguration();
                    }
                    return isServiceInstalled && setConfiguration != null ? FeatureInstallState.Installed : isServiceInstalled || this.Configuration != null ? FeatureInstallState.PartiallyInstalled : FeatureInstallState.NotInstalled;
                }
                catch
                {
                    return isServiceInstalled ? FeatureInstallState.Installed : FeatureInstallState.NotInstalled;
                }
            }

            return isServiceInstalled ? FeatureInstallState.Installed : FeatureInstallState.NotInstalled;
        }

        /// <summary>
        /// Returns the default configuration object for this service (so the configuration tool can expose the options to the user)
        /// </summary>
        protected abstract object GetDefaultConfiguration();

        /// <summary>
        /// Installation task for the generic feature
        /// </summary>
        public class InstallTask : IConfigurationTask
        {
            /// <summary>
            /// Get the installation task
            /// </summary>
            public InstallTask(IFeature feature)
            {
                this.Feature = feature;
            }

            /// <inheritdoc/>
            public string Description => $"This task will register {this.Feature.Name} in the configuration file and service list";

            /// <inheritdoc/>
            public bool Execute(SanteDBConfiguration configuration)
            {
                this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(0.0f, $"Installing Service {this.Feature.Name}..."));
                var serviceType = this.GetServiceType();
                // Look for service type in the services
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(o => o.Type == serviceType);
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(serviceType));

                this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(0.5f, $"Configuring Service {this.Feature.Name}..."));
                // Now configure the object
                configuration.Sections.RemoveAll(o => o.GetType() == this.Feature.ConfigurationType);
                if (this.Feature.Configuration != null)
                {
                    configuration.AddSection(this.Feature.Configuration);
                }

                this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(1.0f, null));
                return true;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => $"Install {this.Feature.Name}";

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(o => o.Type == this.GetServiceType());
                return true;
            }

            /// <inheritdoc/>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == this.GetServiceType()) ||
                    configuration.GetSection(this.Feature.ConfigurationType) == null;
            }

            /// <inheritdoc/>
            private Type GetServiceType()
            {
                var serviceType = this.Feature.GetType();
                while (!serviceType.IsGenericType)
                {
                    serviceType = serviceType.BaseType;
                }

                serviceType = serviceType.GenericTypeArguments[0];
                return serviceType;
            }
        }

        /// <summary>
        /// Un-install the generic feature task
        /// </summary>
        public class UninstallTask : IConfigurationTask
        {
            /// <summary>
            /// Get the installation task
            /// </summary>
            public UninstallTask(IFeature feature)
            {
                this.Feature = feature;
            }

            /// <inheritdoc/>
            public string Id => $"Uninstall-{this.Feature.Name}";

            /// <inheritdoc/>
            public string Description => $"This task will remove {this.Feature.Name} from the service list and remove all configuration settings";

            /// <inheritdoc/>
            public bool Execute(SanteDBConfiguration configuration)
            {
                var serviceType = this.GetServiceType();
                this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(0.0f, $"Removing Service {this.Feature.Name}..."));
                // Look for service type in the services
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(o => o.Type == serviceType);

                // Now configure the object
                this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(0.5f, $"Removing configuration for  {this.Feature.Name}..."));
                configuration.Sections.RemoveAll(o => o.GetType() == this.Feature.ConfigurationType);

                this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(1.0f, null));

                return true;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => $"Uninstall {this.Feature.Name}";

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                return false;
            }

            /// <inheritdoc/>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == this.GetServiceType()) &&
                   configuration.GetSection(this.Feature.ConfigurationType) != null;
            }

            /// <inheritdoc/>
            private Type GetServiceType()
            {
                var serviceType = this.Feature.GetType();
                while (!serviceType.IsGenericType)
                {
                    serviceType = serviceType.BaseType;
                }

                serviceType = serviceType.GenericTypeArguments[0];
                return serviceType;
            }
        }
    }
}