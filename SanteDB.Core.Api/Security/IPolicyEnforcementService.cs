using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a PEP that receives demands
    /// </summary>
    public interface IPolicyEnforcementService : IServiceImplementation
    {

        /// <summary>
        /// Demand access to the policy
        /// </summary>
        void Demand(string policyId);

        /// <summary>
        /// Demand access to the policy on behalf of principal
        /// </summary>
        void Demand(String policyId, IPrincipal principal);
    }
}
