﻿/*
 * Portions Copyright 2019-2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE)
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
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Core.Api.Security;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Principal;

// TODO: Move this to the core API to increase reuse
namespace SanteDB.Core.Security
{
    /// <summary>
    /// Local policy decision service
    /// </summary>
    [ServiceProvider("Default PDP Service")]
    public class DefaultPolicyDecisionService : IPolicyDecisionService
    {

        /// <summary>
        /// Represents an effective policy instance from this PDP
        /// </summary>
        private class EffectivePolicyInstance : IPolicyInstance
        {


            /// <summary>
            /// Effective policy instance
            /// </summary>
            public EffectivePolicyInstance(IPolicy policy, PolicyGrantType rule, IPrincipal forPrincipal)
            {
                this.Policy = policy;
                this.Rule = rule;
                this.Securable = forPrincipal;
            }

            /// <summary>
            /// The policy instance
            /// </summary>
            public IPolicy Policy { get; }

            /// <summary>
            /// Represents the enforcement rule
            /// </summary>
            public PolicyGrantType Rule { get; }

            /// <summary>
            /// Represents the securable
            /// </summary>
            public object Securable { get; }
        }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Default PDP Decision Service";

        /// <summary>
        /// This is not cached
        /// </summary>
        /// <param name="principal"></param>
        public void ClearCache(IPrincipal principal)
        {
        }

        /// <summary>
        /// Get the effective policies (most restrictive of set)
        /// </summary>
        public IEnumerable<IPolicyInstance> GetEffectivePolicySet(IPrincipal principal)
        {
            // First, just verbatim policy most restrictive
            var pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();

            var allPolicies = pip.GetPolicies();
            var activePoliciesForObject = pip.GetPolicies(principal).GroupBy(o => o.Policy.Oid).Select(o => o.OrderBy(p => p.Rule).FirstOrDefault());

            // Create an effective policy list
            return allPolicies.Select(masterPolicy =>
            {
                // Get all policies which are related to this policy 
                var activePolicy = activePoliciesForObject.Where(p => masterPolicy.Oid.StartsWith(p.Policy.Oid)).OrderByDescending(o => o.Policy.Oid.Length).FirstOrDefault(); // The most specific policy grant for this policy

                // What is the most specific policy in this tree?
                if (activePolicy == null)
                    return new EffectivePolicyInstance(masterPolicy, PolicyGrantType.Deny, principal);
                else
                    return new EffectivePolicyInstance(masterPolicy, activePolicy.Rule, principal);
            });
        }

        /// <summary>
        /// Get a policy decision 
        /// </summary>
        public PolicyDecision GetPolicyDecision(IPrincipal principal, object securable)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            else if (securable == null)
                throw new ArgumentNullException(nameof(securable));
            // We need to get the active policies for this
            var pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();
            IEnumerable<IPolicyInstance> securablePolicies = pip.GetPolicies(securable), principalPolicies = null;

            if (principal is IClaimsPrincipal cp)
                principalPolicies = cp.GetGrantedPolicies(pip);
            else
                principalPolicies = this.GetEffectivePolicySet(principal);

            List<PolicyDecisionDetail> details = new List<PolicyDecisionDetail>();
            var retVal = new PolicyDecision(securable, details);

            foreach (var pol in securablePolicies)
            {
                // Get most restrictive from principal
                var rule = principalPolicies.FirstOrDefault(p => p.Policy.Oid == pol.Policy.Oid)?.Rule ?? PolicyGrantType.Deny;

                // Rule for elevate can only be made when the policy allows for it & the principal is allowed
                if (rule == PolicyGrantType.Elevate &&
                    (!pol.Policy.CanOverride ||
                    principalPolicies.Any(o => o.Policy.Oid == PermissionPolicyIdentifiers.ElevateClinicalData && o.Rule == PolicyGrantType.Grant)))
                    rule = PolicyGrantType.Deny;

                details.Add(new PolicyDecisionDetail(pol.Policy.Oid, rule));
            }

            return retVal;
        }

        /// <summary>
        /// Get a policy outcome
        /// </summary>
        public PolicyGrantType GetPolicyOutcome(IPrincipal principal, string policyId)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));
            else if (String.IsNullOrEmpty(policyId))
                throw new ArgumentNullException(nameof(policyId));

            // Get the user object from the principal
            var pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();

            // Can we make this decision based on the claims? 
            if (principal is IClaimsPrincipal claimsPrincipal && claimsPrincipal.HasClaim(c => c.Type == SanteDBClaimTypes.SanteDBGrantedPolicyClaim)) // must adhere to the token
            {
                if (claimsPrincipal.HasClaim(c => c.Type == SanteDBClaimTypes.SanteDBGrantedPolicyClaim && policyId == c.Value))
                    return PolicyGrantType.Grant;
                else
                {
                    // Can override?
                    var polInfo = pip.GetPolicy(policyId);
                    if (polInfo.CanOverride && pip.GetPolicyInstance(pip, PermissionPolicyIdentifiers.OverridePolicyPermission).Rule != PolicyGrantType.Deny)
                        return PolicyGrantType.Elevate;
                    else return PolicyGrantType.Deny;
                }
            }
            else
            {
                
                // Most restrictive
                IPolicyInstance policyInstance = this.GetEffectivePolicySet(principal).FirstOrDefault(o => policyId == o.Policy.Oid);
                var retVal = PolicyGrantType.Deny;

                if (policyInstance == null)
                    retVal = PolicyGrantType.Deny;
                else if (!policyInstance.Policy.CanOverride && policyInstance.Rule == PolicyGrantType.Elevate)
                    retVal = PolicyGrantType.Deny;
                else if (!policyInstance.Policy.IsActive)
                    retVal = PolicyGrantType.Grant;
                else if ((policyInstance.Policy as IHandledPolicy)?.Handler != null)
                {
                    var policy = policyInstance.Policy as IHandledPolicy;
                    if (policy != null)
                        retVal = policy.Handler.GetPolicyDecision(principal, policy, null).Outcome;

                }
                else
                    retVal = policyInstance.Rule;
                return retVal;

            }
        }
    }
}
