using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Policy decision
    /// </summary>
    public class PolicyDecision
    {

        /// <summary>
        /// Creates a new policy decision
        /// </summary>
        public PolicyDecision(Object securable, List<PolicyDecisionDetail> details)
        {
            this.Details = details;
            this.Securable = securable;

        }

        /// <summary>
        /// Details of the policy decision
        /// </summary>
        public IEnumerable<PolicyDecisionDetail> Details { get; private set; }


        /// <summary>
        /// The securable that this policy outcome is made against
        /// </summary>
        public Object Securable { get; private set; }

        /// <summary>
        /// Gets the outcome of the poilcy decision taking into account all triggered policies
        /// </summary>
        public PolicyGrantType Outcome
        {
            get
            {
                PolicyGrantType restrictive = PolicyGrantType.Grant;
                foreach (var dtl in this.Details)
                    if (dtl.Outcome < restrictive)
                        restrictive = dtl.Outcome;
                return restrictive;
            }
        }
    }

}
