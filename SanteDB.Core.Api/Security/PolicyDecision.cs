/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2021-2-9
 */
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

        /// <summary>
        /// Policy decision
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"PolicyDecision-{this.Securable}={this.Outcome}";
    }

}
