using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Principal
{
    /// <summary>
    /// Represents a generic principal
    /// </summary>
    public class GenericPrincipal : IPrincipal
    {

        private String[] m_roles;

        /// <summary>
        /// Creates a new generic principal
        /// </summary>
        public GenericPrincipal(IIdentity identity, String[] roles)
        {
            this.m_roles = roles;
            this.Identity = identity;
        }
        
        /// <summary>
        /// Get the identity
        /// </summary>
        public IIdentity Identity { get; }

        /// <summary>
        /// Determine role membership
        /// </summary>
        public bool IsInRole(string role)
        {
            return this.m_roles.Contains(role);
        }
    }
}
