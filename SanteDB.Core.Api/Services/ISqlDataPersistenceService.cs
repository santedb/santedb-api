using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a data persistence service where arbitrary SQL can be run
    /// </summary>
    public interface ISqlDataPersistenceService
    {

        /// <summary>
        /// Text that identifies the type of database system that is running
        /// </summary>
        string InvariantName { get; }

        /// <summary>
        /// Executes the arbitrary SQL
        /// </summary>
        void Execute(String sql);
    }
}
