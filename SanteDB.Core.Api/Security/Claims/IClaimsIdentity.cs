using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Represents the claims identity
    /// </summary>
    public interface IClaimsIdentity : IIdentity
    {

        /// <summary>
        /// Gets the claims
        /// </summary>
        IEnumerable<IClaim> Claims { get; }
    }
}
