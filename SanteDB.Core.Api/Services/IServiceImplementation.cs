using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a marker class for a service implementation
    /// </summary>
    public interface IServiceImplementation
    {

        /// <summary>
        /// Gets the service name
        /// </summary>
        String ServiceName { get; }
    }
}
