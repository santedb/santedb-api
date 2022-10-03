using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Initialization
{
    /// <summary>
    /// Represents a service that can install <see cref="Dataset"/> files into the 
    /// registered <see cref="IDataPersistenceService"/> instance
    /// </summary>
    public interface IDatasetInstallerService : IServiceImplementation
    {

        /// <summary>
        /// Install the specified dataset
        /// </summary>
        /// <param name="dataset">The dataset which should be installed</param>
        /// <returns>True if the dataset was installed</returns>
        bool Install(Dataset dataset);

        /// <summary>
        /// Get all installed dataset identifiers
        /// </summary>
        IEnumerable<String> GetInstalled();

        /// <summary>
        /// Get the date that the specified dataset was installed
        /// </summary>
        DateTimeOffset? GetInstallDate(String dataSetId);

    }
}
