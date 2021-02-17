using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a simple policy implemtnation
    /// </summary>
    public class GenericPolicy : IPolicy
    {
        /// <summary>
        /// Constructs a simple policy 
        /// </summary>
        public GenericPolicy(Guid key, String oid, String name, bool canOverride)
        {
            this.Key = key;
            this.Oid = oid;
            this.Name = name;
            this.CanOverride = canOverride;
            this.IsActive = true;
        }

        /// <summary>
        /// Gets the key
        /// </summary>
        public Guid Key
        {
            get; private set;
        }

        /// <summary>
        /// True if the policy can be overridden
        /// </summary>
        public bool CanOverride
        {
            get; private set;
        }

        /// <summary>
        /// Returns true if the policy is active
        /// </summary>
        public bool IsActive
        {
            get; private set;
        }

        /// <summary>
        /// Gets the name of the policy
        /// </summary>
        public string Name
        {
            get; private set;
        }

        /// <summary>
        /// Gets the oid of the policy
        /// </summary>
        public string Oid
        {
            get; private set;
        }
    }
}
