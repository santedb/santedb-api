using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Diagnostics
{
    /// <summary>
    /// Represents a service which can be used to manage the log files for the application instance
    /// </summary>
    public interface ILogManagerService : IServiceImplementation
    {


        /// <summary>
        /// Gets all log files from the specified logging source
        /// </summary>
        IEnumerable<FileInfo> GetLogFiles();

        /// <summary>
        /// Get the log file given the specified log identifier
        /// </summary>
        FileInfo GetLogFile(String logId);

        /// <summary>
        /// Delete or remove a log file from the infrastructure
        /// </summary>
        void DeleteLogFile(String logId);

    }
}
