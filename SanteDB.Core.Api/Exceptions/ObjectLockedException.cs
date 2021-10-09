using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// Indicates an object is already locked and the action cannot continue
    /// </summary>
    public class ObjectLockedException : Exception
    {
        /// <summary>
        /// Get the user which locked the object
        /// </summary>
        public String LockedUser { get; }

        /// <summary>
        /// Object is locked
        /// </summary>
        public ObjectLockedException() : base("Object locked")
        {
        }

        /// <summary>
        /// Object has been locked
        /// </summary>
        public ObjectLockedException(String lockUser) : base($"Object Locked by {lockUser}")
        {
            this.LockedUser = lockUser;
        }
    }
}