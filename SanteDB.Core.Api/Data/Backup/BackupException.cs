using System;
using System.Runtime.Serialization;

namespace SanteDB.Core.Data.Backup
{

   
    /// <summary>
    /// An exception that is thrown when creating or restoring backups
    /// </summary>
    public class BackupException : Exception
    {
        /// <inheritdoc/>
        public BackupException(string message) : base(message)
        {
        }

        /// <inheritdoc/>
        public BackupException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}