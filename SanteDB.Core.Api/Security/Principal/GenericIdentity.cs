using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Principal
{
    /// <summary>
    /// Represents a generic identity
    /// </summary>
    public class GenericIdentity : IIdentity
    {

        /// <summary>
        /// Represents a generic identity
        /// </summary>
        public GenericIdentity(String name)
        {
            this.Name = name;
        }

        /// <summary>
        /// The generic identity
        /// </summary>
        public GenericIdentity(String name, Boolean isAuthenticated, String authenticationType)
        {
            this.Name = name;
            this.IsAuthenticated = isAuthenticated;
            this.AuthenticationType = authenticationType;
        }

        /// <summary>
        /// Get the authentication type
        /// </summary>
        public string AuthenticationType { get; }

        /// <summary>
        /// Get whether is authenticated
        /// </summary>
        public bool IsAuthenticated { get; }

        /// <summary>
        /// Get name
        /// </summary>
        public string Name { get; }
    }
}
