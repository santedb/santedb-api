using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// A service implementation that can request restarts of the application context
    /// (configuration changes, backups, etc.)
    /// </summary>
    public interface IRequestRestarts : IServiceImplementation
    {

        /// <summary>
        /// Fired when the backup service requires a restart
        /// </summary>
        event EventHandler RestartRequested;

    }
}
