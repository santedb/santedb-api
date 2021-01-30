using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Privacy enforcement service
    /// </summary>
    public interface IPrivacyEnforcementService : IServiceImplementation
    {

        /// <summary>
        /// Apply all privacy policies
        /// </summary>
        IdentifiedData Apply(IdentifiedData data, IPrincipal principal);

    }
}
