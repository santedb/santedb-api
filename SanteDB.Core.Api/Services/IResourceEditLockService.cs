using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Api.Services
{
    /// <summary>
    /// A resource locking service 
    /// </summary>
    public interface IResourceEditLockService
    {

        /// <summary>
        /// Try to get a lock on the resource for editing
        /// </summary>
        bool Lock<T>(Guid key);

        /// <summary>
        /// Release the lock on the specified key
        /// </summary>
        bool Unlock<T>(Guid key);
    }
}
