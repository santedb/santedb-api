/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
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
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Core.Attributes;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Represents a diagnostics feature
    /// </summary>
    public class DiagnosticsFeature : IFeature
    {
        /// <summary>
        /// Gets the name of the feature
        /// </summary>
        public string Name => "Logging / Tracing";

        /// <summary>
        /// Get the description of the diagnostics feature
        /// </summary>
        public string Description => "Configures the diagnostics trace sources";

        /// <summary>
        /// Get the group for the diagnostics feature
        /// </summary>
        public string Group => FeatureGroup.Diagnostics;

        /// <summary>
        /// Configuration type
        /// </summary>
        public Type ConfigurationType => typeof(GenericFeatureConfiguration);

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        public object Configuration { get; set; }

        /// <summary>
        /// Get the flags
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.SystemFeature;

        /// <summary>
        /// Create the installation tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[] {
                new ConfigureDiagnosticsTask(this)
            };
        }

        /// <summary>
        /// Create uninstall tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Query the state of the diagnostics configuration
        /// </summary>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            // Configuration is not known?
            var config = configuration.GetSection<DiagnosticsConfigurationSection>();
            if (config == null)
                config = new DiagnosticsConfigurationSection();

            // Configuration for trace sources missing?
            GenericFeatureConfiguration configFeature = new GenericFeatureConfiguration();

            // Configuration features
            var asms = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes()
                .Select(t => t.GetTypeInfo().Assembly)
                .Distinct();
            foreach (var source in asms.SelectMany(a => a.GetCustomAttributes<PluginTraceSourceAttribute>()))
            {
                configFeature.Options.Add(source.TraceSourceName, () => Enum.GetValues(typeof(System.Diagnostics.Tracing.EventLevel)));
                var src = config.Sources.FirstOrDefault(
                    s => s.SourceName == source.TraceSourceName);
                if (src != null)
                    configFeature.Values.Add(source.TraceSourceName, src.Filter);
                else
                    configFeature.Values.Add(source.TraceSourceName, System.Diagnostics.Tracing.EventLevel.Warning);
            }

            configFeature.Categories.Add("Sources", configFeature.Options.Keys.ToArray());

            // Writers?
            var tw = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes()
                .Where(t => typeof(TraceWriter).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) && !t.GetTypeInfo().IsAbstract)
                .Distinct();

            configFeature.Options.Add("writer", () => tw);
            configFeature.Options.Add("filter", () => Enum.GetValues(typeof(System.Diagnostics.Tracing.EventLevel)));

            configFeature.Options.Add("initializationData", () => ConfigurationOptionType.FileName);
            configFeature.Categories.Add("Writers", new string[] { "writer", "initializationData", "filter" });


            configFeature.Values.Add("writer", config.TraceWriter.FirstOrDefault()?.TraceWriter?.GetType() ?? tw.FirstOrDefault());
            configFeature.Values.Add("initializationData", config.TraceWriter.FirstOrDefault()?.InitializationData ?? "santedb.log");
            configFeature.Values.Add("filter", config.Mode);
            this.Configuration = configFeature;
            return FeatureInstallState.Installed;
        }


        /// <summary>
        /// Configure the diagnostics services
        /// </summary>
        private class ConfigureDiagnosticsTask : IConfigurationTask
        {
            /// <summary>
            /// Backup of diagnostics
            /// </summary>
            private DiagnosticsConfigurationSection m_backup;

            /// <summary>
            /// Constructor for the diagnostics task
            /// </summary>
            public ConfigureDiagnosticsTask(DiagnosticsFeature feature)
            {
                this.Feature = feature;
            }

            /// <summary>
            /// Gets the name of the task
            /// </summary>
            public string Name => "Configure Diagnostics";

            /// <summary>
            /// Configures the diagnostics subsystem
            /// </summary>
            public string Description => "Configures the diagnostics subsystem";

            /// <summary>
            /// Gets the feature associated with this task
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

                this.m_backup = configuration.GetSection<DiagnosticsConfigurationSection>();

                configuration.RemoveSection<DiagnosticsConfigurationSection>();
                var featureConfig = this.Feature.Configuration as GenericFeatureConfiguration;
                var config = new DiagnosticsConfigurationSection();

                if (featureConfig == null) {
                    this.Feature.QueryState(configuration);
                    featureConfig = this.Feature.Configuration as GenericFeatureConfiguration;
                }

                // Configure writers
                config.TraceWriter.AddRange(this.m_backup.TraceWriter.Where(t => t.WriterName != "main"));
                config.TraceWriter.Add(new TraceWriterConfiguration()
                {
                    WriterName = "main",
                    InitializationData = featureConfig.Values["initializationData"] as String,
                    TraceWriterClassXml = (featureConfig.Values["writer"] as Type).AssemblyQualifiedName,
                    Filter = (System.Diagnostics.Tracing.EventLevel)featureConfig.Values["filter"]
                });
                config.Mode = System.Diagnostics.Tracing.EventLevel.LogAlways;

                // Configure sources
                foreach (var k in featureConfig.Categories["Sources"])
                {
                    config.Sources.Add(new TraceSourceConfiguration()
                    {
                        SourceName = k, 
                        Filter = (System.Diagnostics.Tracing.EventLevel)featureConfig.Values[k]
                    });
                }

                config.Sources.AddRange(this.m_backup.Sources.Where(s => !featureConfig.Categories["Sources"].Contains(s.SourceName)));
                configuration.AddSection(config);
                return true;

            }

            /// <summary>
            /// Rollback configuration
            /// </summary>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                if (this.m_backup != null)
                {
                    configuration.RemoveSection<DiagnosticsConfigurationSection>();
                    configuration.AddSection(this.m_backup);
                }
                return true;
            }

            /// <summary>
            /// Verify that configuration needsto occur
            /// </summary>
            public bool VerifyState(SanteDBConfiguration configuration)
            {
                return true;
            }
        }
    }
}
