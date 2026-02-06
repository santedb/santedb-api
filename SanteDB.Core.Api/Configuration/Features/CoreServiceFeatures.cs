/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Cdss;
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
        public Type ConfigurationType => typeof(ApplicationServiceContextConfigurationSection);

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
                s => s.Type == typeof(SimplePatchService) || s.Type == typeof(SimpleDecisionSupportService)
                ))
            {
                case 2:
                    var cs = configuration.GetSection<ApplicationServiceContextConfigurationSection>();
                    this.Configuration = new ApplicationServiceContextConfigurationSection()
                    {
                        AllowUnsignedAssemblies = cs.AllowUnsignedAssemblies,
                        AppSettings = new List<AppSettingKeyValuePair>(cs.AppSettings),
                        InstanceName = cs.InstanceName,
                        ServiceProviders = new List<TypeReferenceConfiguration>(cs.ServiceProviders),
                        ThreadPoolSize = cs.ThreadPoolSize
                    };
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
                var config = this.Feature.Configuration as ApplicationServiceContextConfigurationSection;
                if (config != null)
                {
                    config.ServiceProviders = config.ServiceProviders.OrderBy(r => this.m_backup.ServiceProviders.Any(b=>b.TypeXml == r.TypeXml) ? this.m_backup.ServiceProviders.FindIndex(b => r.TypeXml == b.TypeXml) : Int32.MaxValue).ToList();
                    //config.AddServices(this.m_backup.ServiceProviders.Where(d => !typeof(IServiceImplementation).IsAssignableFrom(d.Type)));
                    this.m_backup.ServiceProviders = config.ServiceProviders;
                    this.m_backup.InstanceName = config.InstanceName;
                    this.m_backup.AppSettings = config.AppSettings;
                    this.m_backup.AllowUnsignedAssemblies = config.AllowUnsignedAssemblies;

                }

                return true;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => "Save Service Configuration";
#pragma warning disable CS0067

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore

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
                if (!configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleDecisionSupportService)))
                {
                    this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(nameof(InstallCarePlannerServiceTask), 0.0f, "Registering SimpleCarePlanService..."));
                    configuration.GetSection<ApplicationServiceContextConfigurationSection>().AddService(new TypeReferenceConfiguration(typeof(SimpleDecisionSupportService)));
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
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimpleDecisionSupportService));
                return true;
            }

            /// <inheritdoc/>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleDecisionSupportService));
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
                    this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(nameof(InstallPatchServiceTask), 0.0f, "Registering SimplePatchService..."));
                    configuration.GetSection<ApplicationServiceContextConfigurationSection>().AddService(new TypeReferenceConfiguration(typeof(SimplePatchService)));
                    this.ProgressChanged?.Invoke(this, new Services.ProgressChangedEventArgs(nameof(InstallPatchServiceTask), 1.0f, null));
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
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimpleDecisionSupportService));
                return true;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => "Remove Care Planner";
#pragma warning disable CS0067

            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().AddService(new TypeReferenceConfiguration(typeof(SimpleDecisionSupportService)));
                return true;
            }

            /// <inheritdoc/>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleDecisionSupportService));
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

#pragma warning disable CS0067
            /// <inheritdoc/>
            public event EventHandler<Services.ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().AddService(new TypeReferenceConfiguration(typeof(SimplePatchService)));
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