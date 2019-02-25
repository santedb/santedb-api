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
        /// Get the SQL required to deploy the feature
        /// </summary>
        String GetDeploySql(String invariantName);

        /// <summary>
        /// Get SQL required to determine if feature is installed
        /// </summary>
        String GetCheckSql(String invariantName);
    }
}
