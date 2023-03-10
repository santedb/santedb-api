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
using SanteDB.Core.Protocol;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Feature which deploys the core SanteDB services
    /// </summary>
    public class CoreServiceFeatures : IFeature
    {
        /// <inheritdoc/>
        public object Configuration { get; set; }

        /// <inheritdoc/>
        public Type ConfigurationType => typeof(GenericFeatureConfiguration);

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[]
            {
                new InstallCarePlannerServiceTask(this),
                new InstallPatchServiceTask(this),
                new ConfigureServicesTask(this)
            };
        }

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            return new IConfigurationTask[]
            {
                new UninstallCarePlannerServiceTask(this),
                new UninstallPatchServiceTask(this)
            };
        }

        /// <inheritdoc/>
        public string Description => "Core services for this SanteDB API hosting environment. This configuration should only be used by advanced users.";

        /// <inheritdoc/>
        public FeatureFlags Flags => FeatureFlags.SystemFeature;

        /// <inheritdoc/>
        public string Group => FeatureGroup.System;

        /// <inheritdoc/>
        public string Name => "SanteDB Core API";

        /// <inheritdoc/>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            // Get the configuratoin
            switch (configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Count(
                s => s.Type == typeof(SimplePatchService) || s.Type == typeof(SimpleCarePlanService)
                ))
            {
                case 2:
                    var sp = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
                    var types = AppDomain.CurrentDomain.GetAllTypes();
                    var config = new GenericFeatureConfiguration();
                    // Map configuration over to the features section
                    foreach (var pvd in types.Where(t => t.IsInterface && typeof(IServiceImplementation).IsAssignableFrom(t)).ToArray())
                    {
                        if (pvd.Name == "IDaemonService")
                        {
                            var daemons = types.Where(t => pvd.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && !t.ContainsGenericParameters);
                            var daemonNames = daemons.Select(o => o.GetCustomAttribute<ServiceProviderAttribute>()?.Name ?? o.Name);
                            config.Categories.Add("Daemons", daemonNames.ToArray());
                            foreach (var itm in daemons)
                            {
                                config.Options.Add(itm.GetCustomAttribute<ServiceProviderAttribute>()?.Name ?? itm.Name, () => new String[] { "Active", "Disabled" });
                                config.Values.Add(itm.GetCustomAttribute<ServiceProviderAttribute>()?.Name ?? itm.Name, sp.Any(t => t.Type == itm) ? "Active" : "Disabled");
                            }
                            continue;
                        }
                        else
                        {
                            var optionName = pvd.GetCustomAttribute<DescriptionAttribute>()?.Description ?? pvd.FullName;
                            config.Options.Add(optionName, () => types.Where(t => !t.IsInterface && !t.IsAbstract && !t.ContainsGenericParameters && pvd.IsAssignableFrom(t)));
                            config.Values.Add(optionName, sp.FirstOrDefault(o => pvd.IsAssignableFrom(o.Type))?.Type);
                        }
                    }

                    var removeOptions = new List<string>();
                    foreach (var o in config.Options)
                    {
                        if ((o.Value() as IEnumerable)?.OfType<object>().Count() == 0)
                        {
                            removeOptions.Add(o.Key);
                        }
                    }

                    foreach (var itm in removeOptions)
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

        /// <inheritdoc/>
        public class ConfigureServicesTask : IConfigurationTask
        {
            // Backup
            private ApplicationServiceContextConfigurationSection m_backup;

            /// <inheritdoc/>
            public ConfigureServicesTask(CoreServiceFeatures feature)
            {
                this.Feature = feature;
            }

            /// <inheritdoc/>
            public string Description => "Registers the selected services";

            /// <inheritdoc/>
            public bool Execute(SanteDBConfiguration configuration)
            {
                this.m_backup = configuration.GetSection<ApplicationServiceContextConfigurationSection>();

                // Get the configuration
                var config = this.Feature.Configuration as GenericFeatureConfiguration;
                if (config != null)
                {
                    var sp = configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders;
                    var types = AppDomain.CurrentDomain.GetAllTypes();
                    var appConfig = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
                    // Map configuration over to the features section
                    foreach (var pvd in types.Where(t => t.IsInterface && typeof(IServiceImplementation).IsAssignableFrom(t)).ToArray())
                    {
                        object value = null;
                        if (config.Values.TryGetValue(pvd.Name, out value) &&
                            value != null &&
                            !sp.Any(t => value as Type == t.Type))
                        {
                            appConfig.ServiceProviders.Add(new TypeReferenceConfiguration(value as Type));
                        }
                    }

                    //// Remove any sp which aren't configured for any service impl
                    //sp.RemoveAll(r => !config.Values.Any(v => v.Value == r.Type) && !typeof(IDaemonService).IsAssignableFrom(r.Type) &&
                    //    typeof(IServiceImplementation).IsAssignableFrom(r.Type));
                }

                return true;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => "Save Service Configuration";

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                if (this.m_backup != null)
                {
                    configuration.RemoveSection<ApplicationServiceContextConfigurationSection>();
                    configuration.AddSection(this.m_backup);
                }
                return true;
            }

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            public string Description => "Registers the simple, built-in care planner service into the SanteDB context";

            /// <inheritdoc/>
            public bool Execute(SanteDBConfiguration configuration)
            {
                if (!configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleCarePlanService)))
                {
                    this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(0.0f, "Registering SimpleCarePlanService..."));
                    configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimpleCarePlanService)));
                    return true;
                }
                return false;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => "Setup Care Planner";

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimpleCarePlanService));
                return true;
            }

            /// <inheritdoc/>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleCarePlanService));
            }
        }

        /// <summary>
        /// Install patching service
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

            /// <inheritdoc/>
            public string Description => "Registers the simple patching service which allows for partial updates over REST";

            /// <inheritdoc/>
            public bool Execute(SanteDBConfiguration configuration)
            {
                if (!configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimplePatchService)))
                {
                    this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(0.0f, "Registering SimplePatchService..."));
                    configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimplePatchService)));
                    return true;
                }
                return false;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => "Setup Patch Service";

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimplePatchService));
                return true;
            }

            /// <inheritdoc/>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimplePatchService));
            }
        }

        /// <summary>
        /// Remove the care planner service
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

            /// <inheritdoc/>
            public string Description => "Removes the simple care planning service from the SanteDB service";

            /// <inheritdoc/>
            public bool Execute(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimpleCarePlanService));
                return true;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => "Remove Care Planner";

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimpleCarePlanService)));
                return true;
            }

            /// <inheritdoc/>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleCarePlanService));
            }
        }

        /// <summary>
        /// Remove the patching service
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

            /// <inheritdoc/>
            public string Description => "Removes the simple patching service which allows for partial updates over REST";

            /// <inheritdoc/>
            public bool Execute(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimplePatchService));
                return true;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => "Remove Patch Service";

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimplePatchService)));
                return true;
            }

            /// <inheritdoc/>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimplePatchService));
            }
        }
    }
}