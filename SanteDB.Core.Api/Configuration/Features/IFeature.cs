using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration
{

    /// <summary>
    /// Feature installation state
    /// </summary>
    public enum FeatureInstallState
    {
        /// <summary>
        /// The feature is fully installed
        /// </summary>
        Installed,
        /// <summary>
        /// The feature is partially installed
        /// </summary>
        PartiallyInstalled,
        /// <summary>
        /// The feature is not installed
        /// </summary>
        NotInstalled
    }

    /// <summary>
    /// Identifies the flags for configuration
    /// </summary>
    [Flags]
    public enum FeatureFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The task should always be run
        /// </summary>
        AlwaysConfigure = 0x1,
        /// <summary>
        /// The task should be executed automatically if not already run
        /// </summary>
        AutoSetup = 0x2,
        /// <summary>
        /// The feature is a system feature and cannot be uninstalled.
        /// </summary>
        NoRemove = 0x4
    }

    /// <summary>
    /// Configuration options type
    /// </summary>
    public enum ConfigurationOptionType
    {
        /// <summary>
        /// Option is a string
        /// </summary>
        String,
        /// <summary>
        /// Option is a boolean
        /// </summary>
        Boolean,
        /// <summary>
        /// Option is a numeric
        /// </summary>
        Numeric,
        /// <summary>
        /// Option is a password
        /// </summary>
        Password,
        /// <summary>
        /// Option is a filename
        /// </summary>
        FileName,
        /// <summary>
        /// Database name
        /// </summary>
        DatabaseName
    }

    /// <summary>
    /// Representsa feature that can be configured which is not 
    /// </summary>
    public interface IFeature
    {

        /// <summary>
        /// Gets the name of the feature
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Get the description of the feature
        /// </summary>
        String Description { get; }

        /// <summary>
        /// Get the grouping in the configuration
        /// </summary>
        String Group { get; } 

        /// <summary>
        /// Gets the configuration type
        /// </summary>
        Type ConfigurationType { get; }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        Object Configuration { get; set; }

        /// <summary>
        /// Gets the flags for this feature
        /// </summary>
        FeatureFlags Flags { get; }

        /// <summary>
        /// Create the necessary tasks to configure the feature
        /// </summary>
        IEnumerable<IConfigurationTask> CreateInstallTasks();

        /// <summary>
        /// Create uninstallation tasks
        /// </summary>
        IEnumerable<IConfigurationTask> CreateUninstallTasks();

        /// <summary>
        /// Returns true if the configuration supplied is configured for this feature
        /// </summary>
        FeatureInstallState QueryState(SanteDBConfiguration configuration);
    }
}
