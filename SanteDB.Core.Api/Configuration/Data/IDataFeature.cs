using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Configuration.Data
{

    /// <summary>
    /// Represents a particular feature for deployment
    /// </summary>
    public interface IDataFeature
    {
        /// <summary>
        /// Get the name of the data feature
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the description of the feature
        /// </summary>
        String Description { get; }

        /// <summary>
        /// Gets the database provider for which the feature is intended
        /// </summary>
        String InvariantName { get; }

        /// <summary>
        /// Get the SQL required to deploy the feature
        /// </summary>
        String GetDeploySql();

        /// <summary>
        /// Get SQL required to determine if feature is installed
        /// </summary>
        String GetCheckSql();
    }
}
