/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Protocol;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Feature which deploys the core SanteDB services
    /// </summary>
    public class CoreServiceFeatures : IFeature
    {
	    /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        public object Configuration { get; set; }

	    /// <summary>
        /// Gets the configuration option type
        /// </summary>
        public Type ConfigurationType => typeof(GenericFeatureConfiguration);

	    /// <summary>
        /// Create the installation tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[]
            {
                new InstallCarePlannerServiceTask(this),
                new InstallPatchServiceTask(this),
                new ConfigureServicesTask(this)
            };
        }

	    /// <summary>
        /// Create uninstall tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            return new IConfigurationTask[]
            {
                new UninstallCarePlannerServiceTask(this),
                new UninstallPatchServiceTask(this)
            };
        }

	    /// <summary>
        /// Description
        /// </summary>
        public string Description => "Core services for this SanteDB API hosting environment. This configuration should only be used by advanced users.";

	    /// <summary>
        /// Flags for the configuration feature
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.SystemFeature;

	    /// <summary>
        /// The group of these features
        /// </summary>
        public string Group => FeatureGroup.System;

	    /// <summary>
        /// Get the name of the feature
        /// </summary>
        public string Name => "SanteDB Core API";

	    /// <summary>
        /// True if the options are configured
        /// </summary>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {

            // Get the configuratoin
            switch (configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Count(
                s => s.Type == typeof(SimplePatchService) || s.Type == typeof(SimpleCarePlanService)
                ))
            {
                case 2:
                    var sp = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
                    var types = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes();
                    var config = new GenericFeatureConfiguration();

                    // Map configuration over to the features section
                    foreach (var pvd in types.Where(t =>t.GetTypeInfo().IsInterface && typeof(IServiceImplementation).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())).ToArray())
                    {
                        if (pvd.Name == "IDaemonService")
                        {
	                        continue;
                        }

                        config.Options.Add(pvd.Name, () => types.Where(t => !t.GetTypeInfo().IsInterface && !t.GetTypeInfo().IsAbstract && !t.GetTypeInfo().ContainsGenericParameters && pvd.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())));
                        config.Values.Add(pvd.Name, sp.FirstOrDefault(o => pvd.GetTypeInfo().IsAssignableFrom(o.Type.GetTypeInfo()))?.Type);
                    }

                    var removeOptions = new List<string>();
                    foreach (var o in config.Options)
                    {
	                    if ((o.Value() as IEnumerable).OfType<object>().Count() == 0)
	                    {
		                    removeOptions.Add(o.Key);
	                    }
                    }

                    foreach(var itm in removeOptions)
                    {
                        config.Options.Remove(itm);
                        config.Values.Remove(itm);
                    }

                    if (this.Configuration == null)
                    {
	                    this.Configuration = config;
                    }

                    return FeatureInstallState.Installed;
                case 1:
                    return FeatureInstallState.PartiallyInstalled;
                case 0:
                default:
                    return FeatureInstallState.NotInstalled;
            }
        }


	    /// <summary>
        /// Configure services task
        /// </summary>
        public class ConfigureServicesTask : IConfigurationTask
        {
	        // Backup
	        private ApplicationServiceContextConfigurationSection m_backup;

	        /// <summary>
            /// Configure services task
            /// </summary>
            public ConfigureServicesTask(CoreServiceFeatures feature)
            {
                this.Feature = feature;
            }

	        /// <summary>
            /// Description
            /// </summary>
            public string Description => "Registers the selected services";

	        /// <summary>
            /// Execute the service
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                this.m_backup = configuration.GetSection<ApplicationServiceContextConfigurationSection>();

                // Get the configuration
                var config = this.Feature.Configuration as GenericFeatureConfiguration;
                if (config != null)
                {
                    var sp = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
                    var types = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes();
                    var appConfig = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
                    // Map configuration over to the features section
                    foreach (var pvd in types.Where(t => t.GetTypeInfo().IsInterface && typeof(IServiceImplementation).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo())).ToArray())
                    {

                        object value = null;
                        if (config.Values.TryGetValue(pvd.Name, out value) &&
                            value != null &&
                            !sp.Any(t => value as Type == t.Type))
                        {
	                        appConfig.ServiceProviders.Add(new TypeReferenceConfiguration(value as Type));
                        }
                    }

                    // Remove any sp which aren't configured for any service impl
                    sp.RemoveAll(r => !config.Values.Any(v => v.Value == r.Type) && !typeof(IDaemonService).GetTypeInfo().IsAssignableFrom(r.Type.GetTypeInfo()) &&
                        typeof(IServiceImplementation).GetTypeInfo().IsAssignableFrom(r.Type.GetTypeInfo()));
                }

                return true;
            }

	        /// <summary>
            /// Gets the feature
            /// </summary>
            public IFeature Feature { get; }

	        /// <summary>
            /// Get the name
            /// </summary>
            public string Name => "Save Service Configuration";

	        /// <summary>
            /// Fired when the progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

	        /// <summary>
            /// Perform a rollback
            /// </summary>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                if (this.m_backup != null)
                {
                    configuration.RemoveSection<ApplicationServiceContextConfigurationSection>();
                    configuration.AddSection(this.m_backup);
                }
                return true;
            }

	        /// <summary>
            /// Verify the state
            /// </summary>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return true;
            }
        }

	    /// <summary>
        /// Install care planner service
        /// </summary>
        public class InstallCarePlannerServiceTask : IConfigurationTask
        {
	        /// <summary>
            /// Creates a new care planner installation task
            /// </summary>
            public InstallCarePlannerServiceTask(IFeature feature)
            {
                this.Feature = feature;
            }

	        /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Registers the simple, built-in care planner service into the SanteDB context";

	        /// <summary>
            /// Execute the configuration
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                if (!configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleCarePlanService)))
                {
                    this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0.0f, "Registering SimpleCarePlanService..."));
                    configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimpleCarePlanService)));
                    return true;
                }
                return false;
            }

	        /// <summary>
            /// Gets the feature this is configuring
            /// </summary>
            public IFeature Feature { get; }

	        /// <summary>
            /// Gets the name of the task
            /// </summary>
            public string Name => "Setup Care Planner";

	        /// <summary>
            /// Progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

	        /// <summary>
            /// Rollback the configuration
            /// </summary>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimpleCarePlanService));
                return true;
            }

	        /// <summary>
            /// Verify whether the task needs to be run
            /// </summary>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public bool VerifyState(SanteDBConfiguration configuration)
	        {
		        return !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleCarePlanService));
	        }
        }

	    /// <summary>
        /// Install care planner service
        /// </summary>
        public class InstallPatchServiceTask : IConfigurationTask
        {
	        /// <summary>
            /// Creates a new care planner installation task
            /// </summary>
            public InstallPatchServiceTask(IFeature feature)
            {
                this.Feature = feature;
            }

	        /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Registers the simple patching service which allows for partial updates over REST";

	        /// <summary>
            /// Execute the configuration
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                if (!configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimplePatchService)))
                {
                    this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0.0f, "Registering SimplePatchService..."));
                    configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimplePatchService)));
                    return true;
                }
                return false;
            }

	        /// <summary>
            /// Gets the feature this is configuring
            /// </summary>
            public IFeature Feature { get; }

	        /// <summary>
            /// Gets the name of the task
            /// </summary>
            public string Name => "Setup Patch Service";

	        /// <summary>
            /// Progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

	        /// <summary>
            /// Rollback the configuration
            /// </summary>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimplePatchService));
                return true;
            }

	        /// <summary>
            /// Verify whether the task needs to be run
            /// </summary>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public bool VerifyState(SanteDBConfiguration configuration)
	        {
		        return !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimplePatchService));
	        }
        }

	    /// <summary>
        /// Install care planner service
        /// </summary>
        public class UninstallCarePlannerServiceTask : IConfigurationTask
        {
	        /// <summary>
            /// Creates a new care planner installation task
            /// </summary>
            public UninstallCarePlannerServiceTask(IFeature feature)
            {
                this.Feature = feature;
            }

	        /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Removes the simple care planning service from the SanteDB service";

	        /// <summary>
            /// Execute the configuration
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimpleCarePlanService));
                return true;
            }

	        /// <summary>
            /// Gets the feature this is configuring
            /// </summary>
            public IFeature Feature { get; }

	        /// <summary>
            /// Gets the name of the task
            /// </summary>
            public string Name => "Remove Care Planner";

	        /// <summary>
            /// Progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

	        /// <summary>
            /// Rollback the configuration
            /// </summary>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimpleCarePlanService)));
                return true;
            }

	        /// <summary>
            /// Verify whether the task needs to be run
            /// </summary>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public bool VerifyState(SanteDBConfiguration configuration)
	        {
		        return configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleCarePlanService));
	        }
        }

	    /// <summary>
        /// Install care planner service
        /// </summary>
        public class UninstallPatchServiceTask : IConfigurationTask
        {
	        /// <summary>
            /// Creates a new care planner installation task
            /// </summary>
            public UninstallPatchServiceTask(IFeature feature)
            {
                this.Feature = feature;
            }

	        /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Removes the simple patching service which allows for partial updates over REST";

	        /// <summary>
            /// Execute the configuration
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimplePatchService));
                return true;
            }

	        /// <summary>
            /// Gets the feature this is configuring
            /// </summary>
            public IFeature Feature { get; }

	        /// <summary>
            /// Gets the name of the task
            /// </summary>
            public string Name => "Remove Patch Service";

	        /// <summary>
            /// Progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

	        /// <summary>
            /// Rollback the configuration
            /// </summary>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimplePatchService)));
                return true;
            }

	        /// <summary>
            /// Verify whether the task needs to be run
            /// </summary>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public bool VerifyState(SanteDBConfiguration configuration)
	        {
		        return configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimplePatchService));
	        }
        }
    }
}
