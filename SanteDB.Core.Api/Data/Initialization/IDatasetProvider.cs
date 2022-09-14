using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Data.Initialization
{
    /// <summary>
    /// Represents a class which can provide <see cref="Dataset"/> instances for installation
    /// </summary>
    public interface IDatasetProvider 
    {

        /// <summary>
        /// Get datasets from the dataset initialization provider
        /// </summary>
        /// <returns></returns>
        IEnumerable<Dataset> GetDatasets();

    }
}
