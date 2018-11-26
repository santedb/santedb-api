using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a decision on a single policy element
    /// </summary>
    public struct PolicyDecisionDetail
    {
        /// <summary>
        /// Creates a new policy decision outcome
        /// </summary>
        public PolicyDecisionDetail(String policyId, PolicyGrantType outcome)
        {
            this.PolicyId = policyId;
            this.Outcome = outcome;
        }

        /// <summary>
        /// Gets the policy identifier
        /// </summary>
        public String PolicyId { get; private set; }

        /// <summary>
        /// Gets the policy decision outcome
        /// </summary>
        public PolicyGrantType Outcome { get; private set; }
    }
}
