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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;

namespace SanteDB.Core.Configuration.Features
{
    /// <summary>
    /// Configuration <see cref="IFeature"/> which controls the <see cref="DiagnosticsConfigurationSection"/> settings
    /// </summary>
    /// <seealso cref="DiagnosticsConfigurationSection"/>
    public class DiagnosticsFeature : IFeature
    {
        /// <inheritdoc/>
        public object Configuration { get; set; }

        /// <inheritdoc/>
        public Type ConfigurationType => typeof(GenericFeatureConfiguration);

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[] {
                new ConfigureDiagnosticsTask(this)
            };
        }

        /// <inheritdoc/>
        public IEnumerable<IConfigurationTask> CreateUninstallTasks()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public string Description => "Configures the diagnostics trace sources";

        /// <inheritdoc/>
        public FeatureFlags Flags => FeatureFlags.SystemFeature;

        /// <inheritdoc/>
        public string Group => FeatureGroup.Diagnostics;

        /// <inheritdoc/>
        public string Name => "Logging / Tracing";

        /// <inheritdoc/>
        public FeatureInstallState QueryState(SanteDBConfiguration configuration)
        {
            // Configuration is not known?
            var config = configuration.GetSection<DiagnosticsConfigurationSection>();
            if (config == null)
            {
                config = new DiagnosticsConfigurationSection();
            }

            // Configuration for trace sources missing?
            var configFeature = new GenericFeatureConfiguration();

            // Configuration features
            var asms = AppDomain.CurrentDomain.GetAllTypes()
                .Select(t => t.Assembly)
                .Distinct();
            foreach (var source in asms.SelectMany(a => a.GetCustomAttributes<PluginTraceSourceAttribute>()).Select(o => o.TraceSourceName).Distinct())
            {
                configFeature.Options.Add(source, () => Enum.GetValues(typeof(EventLevel)));
                var src = config.Sources.FirstOrDefault(
                    s => s.SourceName == source);
                if (configFeature.Values.ContainsKey(source))
                {
                    continue;
                }
                if (src != null)
                {
                    configFeature.Values.Add(source, src.Filter);
                }
                else
                {
                    configFeature.Values.Add(source, EventLevel.Warning);
                }
            }

            configFeature.Categories.Add("Sources", configFeature.Options.Keys.ToArray());

            // Writers?
            var tw = AppDomain.CurrentDomain.GetAllTypes()
                .Where(t => typeof(TraceWriter).IsAssignableFrom(t) && !t.IsAbstract)
                .Distinct();

            configFeature.Options.Add("writer", () => tw);
            configFeature.Options.Add("filter", () => Enum.GetValues(typeof(EventLevel)));

            configFeature.Options.Add("initializationData", () => ConfigurationOptionType.FileName);
            configFeature.Categories.Add("Writers", new[] { "writer", "initializationData", "filter" });
            configFeature.Values.Add("writer", config.TraceWriter.FirstOrDefault()?.TraceWriter ?? tw.FirstOrDefault());
            configFeature.Values.Add("initializationData", config.TraceWriter.FirstOrDefault()?.InitializationData ?? "santedb.log");
            configFeature.Values.Add("filter", config.Mode);
            this.Configuration = configFeature;
            return FeatureInstallState.Installed;
        }

        /// <summary>
        /// Configure the diagnostics services in the configuration file
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

            /// <inheritdoc/>
            public string Description => "Configures the diagnostics subsystem";

            /// <inheritdoc/>
            public bool Execute(SanteDBConfiguration configuration)
            {
                this.m_backup = configuration.GetSection<DiagnosticsConfigurationSection>();

                configuration.RemoveSection<DiagnosticsConfigurationSection>();
                var featureConfig = this.Feature.Configuration as GenericFeatureConfiguration;
                var config = new DiagnosticsConfigurationSection();

                if (featureConfig == null)
                {
                    this.Feature.QueryState(configuration);
                    featureConfig = this.Feature.Configuration as GenericFeatureConfiguration;
                }

                // Configure writers
                config.TraceWriter.AddRange(this.m_backup.TraceWriter.Where(t => t.WriterName != "main"));
                config.TraceWriter.Add(new TraceWriterConfiguration
                {
                    WriterName = "main",
                    InitializationData = featureConfig.Values["initializationData"] as string,
                    TraceWriterClassXml = (featureConfig.Values["writer"] as Type).AssemblyQualifiedName,
                    Filter = (EventLevel)featureConfig.Values["filter"]
                });
                config.Mode = EventLevel.LogAlways;

                // Configure sources
                foreach (var k in featureConfig.Categories["Sources"])
                {
                    config.Sources.Add(new TraceSourceConfiguration
                    {
                        SourceName = k,
                        Filter = (EventLevel)featureConfig.Values[k]
                    });
                }

                config.Sources.AddRange(this.m_backup.Sources.Where(s => !featureConfig.Categories["Sources"].Contains(s.SourceName)));
                configuration.AddSection(config);
                return true;
            }

            /// <inheritdoc/>
            public IFeature Feature { get; }

            /// <inheritdoc/>
            public string Name => "Configure Diagnostics";

            /// <inheritdoc/>
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

            /// <inheritdoc/>
            public bool Rollback(SanteDBConfiguration configuration)
            {
                if (this.m_backup != null)
                {
                    configuration.RemoveSection<DiagnosticsConfigurationSection>();
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
    }
}