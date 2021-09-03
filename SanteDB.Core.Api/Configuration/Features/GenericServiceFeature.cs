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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using SanteDB.Core.Attributes;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Services;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents a feature which wraps a generic service
    /// </summary>
    public abstract class GenericServiceFeature<TService> : IFeature
        where TService : IServiceImplementation
    {
	    /// <summary>
        /// Create a generic service feature
        /// </summary>
        public GenericServiceFeature()
        {
            var instanceAtt = typeof(TService).GetCustomAttribute<ServiceProviderAttribute>();
            if (instanceAtt != null) {
                this.Name = instanceAtt?.Name;
                this.Description = instanceAtt?.Name;
              
            }
            else {
                this.Name = typeof(TService).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? typeof(TService).Name;
                this.Description = typeof(TService).GetCustomAttribute<DescriptionAttribute>()?.Description ?? typeof(TService).Name;
            }
            this.Group = typeof(TService).Assembly.GetCustomAttribute<PluginAttribute>()?.Group;
        }

	    /// <summary>
        /// Gets or sets the configuration for this feature
        /// </summary>
        public virtual object Configuration { 
            get; 
            set; 
        }

	    /// <summary>
        /// Gets the configuration type
        /// </summary>
        public abstract Type ConfigurationType { get; }

	    /// <summary>
        /// Create the installation tasks
        /// </summary>
        public virtual IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[] { new InstallTask(this) };
        }

	    /// <summary>
        /// Create uninstallation task
        /// </summary>
        public virtual IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            return new IConfigurationTask[] { new UninstallTask(this) };
        }

	    /// <summary>
        /// Gets the description of the service
        /// </summary>
        public virtual string Description { get; }

	    /// <summary>
        /// Get the flags for this feature
        /// </summary>
        public virtual FeatureFlags Flags => typeof(TService).Assembly.GetCustomAttribute<PluginAttribute>()?.EnableByDefault == true ? FeatureFlags.AutoSetup : FeatureFlags.None;

	    /// <summary>
        /// Gets the group name
        /// </summary>
        public virtual string Group { get; }

	    /// <summary>
        /// Gets the name of the service provider
        /// </summary>
        public virtual string Name { get; }

	    /// <summary>
        /// Returns true if the object is configured
        /// </summary>
        public virtual FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            var isServiceInstalled = configuration.GetSection<ApplicationServiceContextConfigurationSection>()?.ServiceProviders.Any(o=>o.Type == typeof(TService)) == true;
            // First, this configuration type is available
            if (this.ConfigurationType != null)
            {
                try
                {
                    var setConfiguration = configuration.GetSection(this.ConfigurationType);
                    if(setConfiguration != null) // Set the configuration from the file
                    {
                        this.Configuration = setConfiguration;
                    }
                    return isServiceInstalled && this.Configuration != null ? FeatureInstallState.Installed : isServiceInstalled || this.Configuration != null ? FeatureInstallState.PartiallyInstalled : FeatureInstallState.NotInstalled;

                }
                catch
                {
                    return isServiceInstalled ? FeatureInstallState.Installed : FeatureInstallState.NotInstalled;
                }
            }

            return isServiceInstalled ? FeatureInstallState.Installed : FeatureInstallState.NotInstalled;
        }

	    /// <summary>
        /// Installation task
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

	        /// <summary>
            /// Description
            /// </summary>
            public string Description => $"This task will register {this.Feature.Name} in the configuration file and service list";

	        /// <summary>
            /// Execute the specified configuration task
            /// </summary>
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

	        /// <summary>
            /// Gets the feature
            /// </summary>
            public IFeature Feature { get; }

	        /// <summary>
            /// Get the name of the task
            /// </summary>
            public string Name => $"Install {this.Feature.Name}";

	        /// <summary>
            /// Fired when the feature is installed
            /// </summary>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

	        /// <summary>
            /// Rollback the configuration
            /// </summary>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(o => o.Type == this.GetServiceType());
                return true;
            }

	        /// <summary>
            /// Verify state
            /// </summary>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == this.GetServiceType()) ||
                    configuration.GetSection(this.Feature.ConfigurationType) == null;
            }

	        /// <summary>
            /// Get the service type
            /// </summary>
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
        /// Installation task
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

	        /// <summary>
            /// Main install
            /// </summary>
            public string Id => $"Uninstall-{this.Feature.Name}";

	        /// <summary>
            /// Description
            /// </summary>
            public string Description => $"This task will remove {this.Feature.Name} from the service list and remove all configuration settings";

	        /// <summary>
            /// Execute the specified configuration task
            /// </summary>
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

	        /// <summary>
            /// Gets the feature
            /// </summary>
            public IFeature Feature { get; }

	        /// <summary>
            /// Get the name of the task
            /// </summary>
            public string Name => $"Uninstall {this.Feature.Name}";

	        /// <summary>
            /// Fired when the feature is installed
            /// </summary>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

	        /// <summary>
            /// Rollback the configuration
            /// </summary>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                return false;
            }

	        /// <summary>
            /// Verify state
            /// </summary>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == this.GetServiceType()) &&
                   configuration.GetSection(this.Feature.ConfigurationType) != null;
            }

	        /// <summary>
            /// Get the service type
            /// </summary>
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
