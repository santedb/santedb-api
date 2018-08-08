using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service 
    /// </summary>
    public interface IRecordMatchingConfigurationService
    {

        /// <summary>
        /// Get the specified named configuration
        /// </summary>
        IRecordMatchingConfiguration GetConfiguration(String name);

    }
}
