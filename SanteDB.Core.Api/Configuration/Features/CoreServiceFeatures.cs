using SanteDB.Core.Protocol;
using SanteDB.Core.Services;
using SanteDB.Core.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Feature which deploys the core SanteDB services
    /// </summary>
    public class CoreServiceFeatures : IFeature
    {
        /// <summary>
        /// Get the name of the feature
        /// </summary>
        public string Name => "SanteDB Core API";

        /// <summary>
        /// Description
        /// </summary>
        public string Description => "Core features of the SanteDB API";

        /// <summary>
        /// The group of these features
        /// </summary>
        public string Group => "System";

        /// <summary>
        /// Gets the configuration option type
        /// </summary>
        public Type ConfigurationType => null;

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        public object Configuration { get; set; }

        /// <summary>
        /// Flags for the configuration feature
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.AutoSetup | FeatureFlags.NoRemove;

        /// <summary>
        /// Create the installation tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[]
            {
                new InstallCarePlannerServiceTask(this),
                new InstallPatchServiceTask(this)
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
        /// True if the options are configured
        /// </summary>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            switch (configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Count(
                s => s.Type == typeof(SimplePatchService) || s.Type == typeof(SimpleCarePlanService)
                ))
            {
                case 2:
                    return FeatureInstallState.Installed;
                case 1:
                    return FeatureInstallState.PartiallyInstalled;
                case 0:
                default:
                    return FeatureInstallState.NotInstalled;
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
            /// Gets the name of the task
            /// </summary>
            public string Name => "Setup Patch Service";

            /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Registers the simple patching service which allows for partial updates over REST";

            /// <summary>
            /// Gets the feature this is configuring
            /// </summary>
            public IFeature Feature { get; }

            /// <summary>
            /// Progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

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
            public bool VerifyState(SanteDBConfiguration configuration) => !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimplePatchService));
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
            /// Gets the name of the task
            /// </summary>
            public string Name => "Setup Care Planner";

            /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Registers the simple, built-in care planner service into the SanteDB context";

            /// <summary>
            /// Gets the feature this is configuring
            /// </summary>
            public IFeature Feature { get; }

            /// <summary>
            /// Progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

            /// <summary>
            /// Execute the configuration
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                if(!configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o=>o.Type == typeof(SimpleCarePlanService)))
                {
                    this.ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(0.0f, "Registering SimpleCarePlanService..."));
                    configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Add(new TypeReferenceConfiguration(typeof(SimpleCarePlanService)));
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Rollback the configuration
            /// </summary>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t=>t.Type == typeof(SimpleCarePlanService));
                return true;
            }

            /// <summary>
            /// Verify whether the task needs to be run
            /// </summary>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public bool VerifyState(SanteDBConfiguration configuration) => !configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleCarePlanService));
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
            /// Gets the name of the task
            /// </summary>
            public string Name => "Remove Patch Service";

            /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Removes the simple patching service which allows for partial updates over REST";

            /// <summary>
            /// Gets the feature this is configuring
            /// </summary>
            public IFeature Feature { get; }

            /// <summary>
            /// Progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

            /// <summary>
            /// Execute the configuration
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimplePatchService));
                return true;
            }

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
            public bool VerifyState(SanteDBConfiguration configuration) => configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimplePatchService));
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
            /// Gets the name of the task
            /// </summary>
            public string Name => "Remove Care Planner";

            /// <summary>
            /// Gets the description of the task
            /// </summary>
            public string Description => "Removes the simple care planning service from the SanteDB service";

            /// <summary>
            /// Gets the feature this is configuring
            /// </summary>
            public IFeature Feature { get; }

            /// <summary>
            /// Progress has changed
            /// </summary>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

            /// <summary>
            /// Execute the configuration
            /// </summary>
            public bool Execute(SanteDBConfiguration configuration)
            {
                configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.RemoveAll(t => t.Type == typeof(SimpleCarePlanService));
                return true;
            }

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
            public bool VerifyState(SanteDBConfiguration configuration) => configuration.GetSection<ApplicationServiceContextConfigurationSection>().ServiceProviders.Any(o => o.Type == typeof(SimpleCarePlanService));

        }
    }
}
