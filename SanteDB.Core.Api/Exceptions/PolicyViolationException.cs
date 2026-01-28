/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Linq;
using System.Security;
using System.Security.Principal;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// Represents a policy violation
    /// </summary>
    public class PolicyViolationException : SecurityException
    {

        // Policy name
        private string m_policyName;
        private string m_detail;

        /// <summary>
        /// Create a new <see cref="PolicyViolationException"/> with additional details
        /// </summary>
        /// <param name="principal">The principal</param>
        /// <param name="policyId">The policy identifier</param>
        /// <param name="outcome">The outcome of the policy violation</param>
        /// <param name="additionalDetail">Additional details </param>
        public PolicyViolationException(IPrincipal principal, string policyId, PolicyGrantType outcome, String additionalDetail) : this(principal, policyId, outcome)
        {
            this.Data.Add("detail", additionalDetail);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyViolationException"/> class.
        /// </summary>
        /// <param name="policyId">Policy identifier.</param>
        /// <param name="outcome">Outcome.</param>
        /// <param name="principal">The principal that the action was attempted as</param>
        public PolicyViolationException(IPrincipal principal, string policyId, PolicyGrantType outcome)
        {
            this.PolicyId = policyId;
            this.PolicyDecision = principal.Identity.Name == "ANONYMOUS" ? PolicyGrantType.Elevate : outcome;
            this.Principal = principal;
            try
            {
                this.m_policyName = ApplicationServiceContext.Current.GetService<IPolicyInformationService>()?.GetPolicy(policyId)?.Name;
            }
            catch { }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyViolationException"/> class.
        /// </summary>
        /// <param name="policy">Policy identifier.</param>
        /// <param name="outcome">Outcome.</param>
        /// <param name="principal">The principal that the action was attempted as</param>
        public PolicyViolationException(IPrincipal principal, IPolicy policy, PolicyGrantType outcome)
        {
            this.PolicyId = policy.Oid;
            this.PolicyDecision = principal.Identity.Name == "ANONYMOUS" ? PolicyGrantType.Elevate : outcome;
            this.Principal = principal;
            this.m_policyName = policy.Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyViolationException"/> class.
        /// </summary>
        /// <param name="principal">The principal which attempted the action</param>
        /// <param name="decision">The decision of the policy which caused the exception</param>
        public PolicyViolationException(IPrincipal principal, PolicyDecision decision)
        {
            var policy = decision.Details.First(p => p.Outcome == decision.Outcome);
            this.PolicyId = policy.Policy.Oid;
            this.m_policyName = policy.Policy.Name;
            this.Detail = decision;
            this.Principal = principal;
            this.PolicyDecision = principal.Identity.Name == "ANONYMOUS" ? PolicyGrantType.Elevate : decision.Outcome;
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string (").</returns>
        /// <filterpriority>1</filterpriority>
        /// <value>The message.</value>
        public override string Message
        {
            get
            {

                return String.Format("Policy {3} ({0}) was violated by '{1}' with outcome '{2}'", this.PolicyId, this.Principal?.Identity?.Name ?? "UNKNOWN", this.PolicyDecision, this.m_policyName);
            }
        }

        /// <summary>
        /// Gets the principal that violated the policy
        /// </summary>
        public IPrincipal Principal { get; private set; }

        /// <summary>
        /// Gets the policy that was violated
        /// </summary>
        /// <value>The policy.</value>
        public IPolicy Policy { get; private set; }
        /// <summary>
        /// Gets the policy decision.
        /// </summary>
        /// <value>The policy decision.</value>
        public PolicyGrantType PolicyDecision { get; private set; }
        /// <summary>
        /// Gets the policy identifier.
        /// </summary>
        /// <value>The policy identifier.</value>
        public string PolicyId { get; private set; }

        /// <summary>
        /// Policy name
        /// </summary>
        public string PolicyName => this.m_policyName;

        /// <summary>
        /// The details of the violation
        /// </summary>
        public PolicyDecision Detail { get; private set; }
    }
}

