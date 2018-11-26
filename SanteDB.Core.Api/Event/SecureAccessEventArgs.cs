using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Event
{

    /// <summary>
    /// Represents secure access 
    /// </summary>
    public abstract class SecureAccessEventArgs : EventArgs
    {

        /// <summary>
        /// Create new secure access event args
        /// </summary>
        public SecureAccessEventArgs(IPrincipal principal)
        {
            this.Principal = principal ?? AuthenticationContext.Current.Principal;
        }

        /// <summary>
        /// Gets the principal
        /// </summary>
        public IPrincipal Principal { get; }
    }
}
