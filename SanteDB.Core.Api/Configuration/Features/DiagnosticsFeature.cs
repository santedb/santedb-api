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
        public string Name => "Diagnostics";

        /// <summary>
        /// Get the description of the diagnostics feature
        /// </summary>
        public string Description => "Configures the diagnostics trace sources";

        /// <summary>
        /// Get the group for the diagnostics feature
        /// </summary>
        public string Group => "System";

        /// <summary>
        /// Configuration type
        /// </summary>
        public Type ConfigurationType => typeof(DiagnosticsConfigurationSection);

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        public object Configuration { get; set; }

        /// <summary>
        /// Get the flags
        /// </summary>
        public FeatureFlags Flags => FeatureFlags.AutoSetup | FeatureFlags.AlwaysConfigure | FeatureFlags.NoRemove;

        /// <summary>
        /// Create the installation tasks
        /// </summary>
        public IEnumerable<IConfigurationTask> CreateInstallTasks()
        {
            return new IConfigurationTask[0];
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
            this.Configuration = config;

            // Configuration for trace sources missing?
            var asms = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes()
                .Select(t => t.GetTypeInfo().Assembly)
                .Distinct();
            foreach(var source in asms.SelectMany(a=>a.GetCustomAttributes<PluginTraceSourceAttribute>()))
            {
                if (!config.Sources.Any(s => s.SourceName == source.TraceSourceName))
                    config.Sources.Add(new TraceSourceConfiguration()
                    {
                        SourceName = source.TraceSourceName,
#if DEBUG
                        Filter = System.Diagnostics.Tracing.EventLevel.Verbose
#else
                        Filter = System.Diagnostics.Tracing.EventLevel.Error
#endif
                    });
            }

            // Writers?
            var tw = ApplicationServiceContext.Current.GetService<IServiceManager>().GetAllTypes()
                .Where(t => typeof(TraceWriter).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()) && !t.GetTypeInfo().IsAbstract)
                .Distinct();
            if (config.TraceWriter.Count == 0)
                config.TraceWriter.AddRange(tw.Select(o => new TraceWriterConfiguration()
                {
                    TraceWriterClassXml = o.AssemblyQualifiedName,
#if DEBUG
                    Filter = System.Diagnostics.Tracing.EventLevel.Verbose
#else
                        Filter = System.Diagnostics.Tracing.EventLevel.Error
#endif
                }));
            return FeatureInstallState.Installed;
        }
        
    }
}
