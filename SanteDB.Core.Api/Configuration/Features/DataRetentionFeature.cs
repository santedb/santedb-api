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
using SanteDB.Core.Jobs;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Data retention feature which enables the retention service
    /// </summary>
    /// <remarks>This feature sets up the <see cref="DataRetentionConfigurationSection"/> with the user's preferred retention rules, and
    /// then enables the <see cref="DataRetentionJob"/> in the host context.</remarks>
    /// <seealso cref="DataRetentionConfigurationSection"/>
    /// <seealso cref="DataRetentionJob"/>
    public class DataRetentionFeature : IFeature
    {
        // Configuration
        private GenericFeatureConfiguration m_configuration;

        /// <summary>
        /// Creates a new data retention feature
        /// </summary>
        public DataRetentionFeature()
        {
        }

        /// <inheritdoc/>
        public object Configuration { get; set; }

        /// <inheritdoc/>
        public Type ConfigurationType => typeof(GenericFeatureConfiguration);

        /// <inheritdoc/>
        public string Description => "Controls the frequency and actions for data retention on this SanteDB instance";

        /// <inheritdoc/>
        public FeatureFlags Flags => FeatureFlags.None;

        /// <inheritdoc/>
        public string Group => FeatureGroup.Persistence;

        /// <inheritdoc/>
        public string Name => "Data Retention Service";

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[]
            {
                new ConfigureRetentionJobTask(this, this.m_configuration),
                new ConfigureRetentionPoliciesTask(this, this.m_configuration.Values["Retention Policies"] as DataRetentionConfigurationSection)
            }.OfType<IConfigurationTask>();
        }

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            return new IConfigurationTask[] {
                new RemoveRetentionJobTask(this)
            };
        }

        /// <inheritdoc/>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            // First ensure the data retention service is present
            if (this.Configuration == null)
            {
                this.Configuration = this.m_configuration = new GenericFeatureConfiguration();

                this.m_configuration.Options.Add("Retention Policies", () => ConfigurationOptionType.Object);
                var retentionConfig = configuration.GetSection<DataRetentionConfigurationSection>();
                if (retentionConfig == null)
                {
                    this.m_configuration.Values.Add("Retention Policies", new DataRetentionConfigurationSection());
                }
                else
                {
                    this.m_configuration.Values.Add("Retention Policies", retentionConfig);
                }

                // Next the timer job
                var jobSection = configuration.GetSection<JobConfigurationSection>();
                var jobConfig = jobSection?.Jobs.Find(o => o.Type == typeof(DataRetentionJob));
                this.m_configuration.Options.Add("Job Status", () => new String[] { "Enabled", "Disabled" });
                this.m_configuration.Values.Add("Job Status", jobConfig == null ? "Disabled" : "Enabled");
                this.m_configuration.Options.Add("Schedule", () => ConfigurationOptionType.Object);
                this.m_configuration.Values.Add("Schedule", jobConfig?.Schedule?.FirstOrDefault() ?? new JobItemSchedule());

                return retentionConfig == null && jobConfig == null ? FeatureInstallState.NotInstalled :
                    retentionConfig != null || jobConfig != null || jobConfig.StartType == JobStartType.Never ? FeatureInstallState.PartiallyInstalled :
                    FeatureInstallState.Installed;
            }
            else
            {
                var jobSection = configuration.GetSection<JobConfigurationSection>();
                var jobConfig = jobSection?.Jobs.Find(o => o.Type == typeof(DataRetentionJob));
                var retentionConfig = configuration.GetSection<DataRetentionConfigurationSection>();

                this.m_configuration = this.Configuration as GenericFeatureConfiguration;
                return retentionConfig == null && jobConfig == null ? FeatureInstallState.NotInstalled :
                     retentionConfig != null || jobConfig != null || jobConfig.StartType == JobStartType.Never ? FeatureInstallState.PartiallyInstalled :
                     FeatureInstallState.Installed;
            }
        }
    }

    /// <summary>
    /// Remove the retention job
    /// </summary>
    internal class RemoveRetentionJobTask : IConfigurationTask
    {
        /// <summary>
        /// Remove the feature
        /// </summary>
        public RemoveRetentionJobTask(IFeature feature)
        {
            this.Feature = feature;
        }

        /// <inheritdoc/>
        public string Description => "Disables the retention policy job so that it does not run on its schedule. Note: You can still manually run the job";

        /// <inheritdoc/>
        public IFeature Feature { get; }

        /// <inheritdoc/>
        public string Name => "Disable Retention Job";

#pragma warning disable CS0067
        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore

        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            // Ensure the job section exists
            var jobSection = configuration.GetSection<JobConfigurationSection>();
            if (jobSection != null)
            {
                var job = jobSection.Jobs.FirstOrDefault(o => o.Type == typeof(DataRetentionJob));
                if (job != null)
                {
                    job.StartType = JobStartType.Never;
                }
            }
            return true;
        }

        /// <inheritdoc/>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            var jobSection = configuration.GetSection<JobConfigurationSection>();
            return jobSection != null && jobSection.Jobs.FirstOrDefault(o => o.Type == typeof(DataRetentionJob))?.StartType != JobStartType.Never;
        }
    }

    /// <summary>
    /// Configure retention policies in the primary configuration file
    /// </summary>
    internal class ConfigureRetentionPoliciesTask : IConfigurationTask
    {
        // The configuration
        private DataRetentionConfigurationSection m_configuration;

        /// <summary>
        /// Creates a new retention policy configuration task
        /// </summary>
        public ConfigureRetentionPoliciesTask(IFeature feature, DataRetentionConfigurationSection configuration)
        {
            this.m_configuration = configuration;
            this.Feature = feature;
        }

        /// <inheritdoc/>
        public string Description => "Configures the data retention policies so that the data retention jobs can operate properly";

        /// <inheritdoc/>
        public IFeature Feature { get; }

        /// <inheritdoc/>
        public string Name => "Configure Retention Policies";

#pragma warning disable CS0067
        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore

        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            configuration.RemoveSection<DataRetentionConfigurationSection>();
            configuration.AddSection(this.m_configuration);
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

    /// <summary>
    /// Configure the retention job in the job manager
    /// </summary>
    internal class ConfigureRetentionJobTask : IConfigurationTask
    {
        // Enabled
        private GenericFeatureConfiguration m_configuration;

        /// <summary>
        /// Creates a new retnetion policy configuration task
        /// </summary>
        public ConfigureRetentionJobTask(IFeature feature, GenericFeatureConfiguration configuration)
        {
            this.m_configuration = configuration;
            this.Feature = feature;
        }

        /// <inheritdoc/>
        public string Description => "Configures the retention job with the specified schedule in your configuration";

        /// <inheritdoc/>
        public IFeature Feature { get; }

#pragma warning disable CS0067
        /// <inheritdoc/>
        public string Name => "Configure Retention Job";


        /// <inheritdoc/>
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
#pragma warning restore
        /// <inheritdoc/>
        public bool Execute(SanteDBConfiguration configuration)
        {
            // Ensure the job manager is configured
            var appService = configuration.GetSection<ApplicationServiceContextConfigurationSection>()?.ServiceProviders;
            if (!appService.Any(o => typeof(IJobManagerService).IsAssignableFrom(o.Type)))
            {
                appService.Add(new TypeReferenceConfiguration(typeof(DefaultJobManagerService)));
            }

            // Ensure the job section exists
            var jobSection = configuration.GetSection<JobConfigurationSection>();
            if (jobSection == null)
            {
                jobSection = new JobConfigurationSection();
                configuration.AddSection(jobSection);
            }

            // Remove the job for data retention
            jobSection.Jobs.RemoveAll(o => o.Type == typeof(DataRetentionJob));
            jobSection.Jobs.Add(new JobItemConfiguration()
            {
                Type = typeof(DataRetentionJob),
                StartType = JobStartType.TimerOnly,
                Schedule = new List<JobItemSchedule>() { this.m_configuration.Values["Schedule"] as JobItemSchedule }
            });

            return true;
        }

        /// <inheritdoc/>
        public bool Rollback(SanteDBConfiguration configuration)
        {
            return true;
        }

        /// <inheritdoc/>
        public bool VerifyState(SanteDBConfiguration configuration)
        {
            return this.m_configuration.Values["Job Status"].Equals("Enabled");
        }
    }
}