using SanteDB.Core.Configuration;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration
{
    
    /// <summary>
    /// Represents a configuration task
    /// </summary>
    public interface IConfigurationTask : IReportProgressChanged
    {

        /// <summary>
        /// Get the name of the task
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Get description of the task
        /// </summary>
        String Description { get;  }

        /// <summary>
        /// Gets the feature that is being configured
        /// </summary>
        IFeature Feature { get; }

        /// <summary>
        /// Execute the configuration task
        /// </summary>
        bool Execute(SanteDBConfiguration configuration);

        /// <summary>
        /// Rollback changes in the specified configuration
        /// </summary>
        bool Rollback(SanteDBConfiguration configuration);

        /// <summary>
        /// Verify the task prior to running
        /// </summary>
        bool VerifyState(SanteDBConfiguration configuration);
    }

    /// <summary>
    /// Represents a configuration task which is described
    /// </summary>
    public interface IDescribedConfigurationTask : IConfigurationTask
    {

        /// <summary>
        /// Get information about the task
        /// </summary>
        Uri HelpUri { get; }

        /// <summary>
        /// Gets the additional information
        /// </summary>
        String AdditionalInformation { get; }
    }
}
