/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using Newtonsoft.Json;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Security.Principal;
using System.Xml.Serialization;

// TODO: Move this to the core API to increase reuse
namespace SanteDB.Core.Security
{
    /// <summary>
    /// Local policy decision service
    /// </summary>
    [ServiceProvider("Default PDP Service")]
    public class DefaultPolicyDecisionService : IPolicyDecisionService
    {
        // Adhoc cache reference
        private IAdhocCacheService m_adhocCacheService;

        private IPasswordHashingService m_hasher;
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultPolicyDecisionService));

        /// <summary>
        /// Policy for the grant
        /// </summary>
        private class EffectivePolicy : IPolicy
        {
            /// <summary>
            /// Generic policy
            /// </summary>
            public EffectivePolicy()
            {
            }

            /// <summary>
            /// Constructs a simple policy
            /// </summary>
            public EffectivePolicy(Guid key, String oid, String name, bool canOverride)
            {
                this.Key = key;
                this.Oid = oid;
                this.Name = name;
                this.CanOverride = canOverride;
                this.IsActive = true;
            }

            /// <summary>
            /// Gets the key
            /// </summary>
            public Guid Key
            {
                get; set;
            }

            /// <summary>
            /// True if the policy can be overridden
            /// </summary>
            public bool CanOverride
            {
                get; set;
            }

            /// <summary>
            /// Returns true if the policy is active
            /// </summary>
            public bool IsActive
            {
                get; set;
            }

            /// <summary>
            /// Gets the name of the policy
            /// </summary>
            public string Name
            {
                get; set;
            }

            /// <summary>
            /// Gets the oid of the policy
            /// </summary>
            public string Oid
            {
                get; set;
            }
        }

        /// <summary>
        /// Represents an effective policy instance from this PDP
        /// </summary>
        private class EffectivePolicyInstance : IPolicyInstance
        {
            // Securable
            private object m_securable;

            /// <summary>
            /// Serialization ctor
            /// </summary>
            public EffectivePolicyInstance()
            {
            }

            /// <summary>
            /// Effective policy instance
            /// </summary>
            public EffectivePolicyInstance(IPolicy policy, PolicyGrantType rule, IPrincipal forPrincipal)
            {
                this.Policy = new EffectivePolicy(policy.Key, policy.Oid, policy.Name, policy.CanOverride);
                this.Rule = rule;
                this.m_securable = forPrincipal;
            }

            /// <summary>
            /// Gets or sets the policy
            /// </summary>
            public EffectivePolicy Policy { get; set; }

            /// <summary>
            /// Gets or sets the rule
            /// </summary>
            public PolicyGrantType Rule { get; set; }

            /// <summary>
            /// The policy instance
            /// </summary>
            [JsonIgnore, XmlIgnore]
            IPolicy IPolicyInstance.Policy => this.Policy;

            /// <summary>
            /// Represents the enforcement rule
            /// </summary>
            [JsonIgnore, XmlIgnore]
            PolicyGrantType IPolicyInstance.Rule => this.Rule;

            /// <summary>
            /// Represents the securable
            /// </summary>
            [JsonIgnore, XmlIgnore]
            object IPolicyInstance.Securable => this.m_securable;

            /// <summary>
            /// Get the policy string
            /// </summary>
            public override string ToString() => $"{this.Policy?.Oid} ({this.Policy?.Name}) => {this.Rule}";
        }

        /// <summary>
        /// Gets the service name
        /// </summary>
        public String ServiceName => "Default PDP Decision Service";

        /// <summary>
        /// Default policy decision service
        /// </summary>
        public DefaultPolicyDecisionService(IPasswordHashingService hashService, IAdhocCacheService adhocCache = null)
        {
            this.m_adhocCacheService = adhocCache;
            this.m_hasher = hashService;
        }

        /// <summary>
        /// This is not cached
        /// </summary>
        public void ClearCache(IPrincipal principal)
        {
            string cacheKey = this.ComputeCacheKey(principal);
            this.m_adhocCacheService?.Remove(cacheKey);
        }

        /// <summary>
        /// Get the effective policies (most restrictive of set)
        /// </summary>
        public IEnumerable<IPolicyInstance> GetEffectivePolicySet(IPrincipal principal)
        {
            string cacheKey = this.ComputeCacheKey(principal);
            var result = this.m_adhocCacheService?.Get<EffectivePolicyInstance[]>(cacheKey);

            if (result == null)
            {
                // First, just verbatim policy most restrictive
                var pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();
                if (pip == null)
                {
                    this.m_tracer.TraceWarning("No IPolicyInformationService is registered, default will be deny for all policies");
                    return new List<IPolicyInstance>();
                }

                var allPolicies = pip.GetPolicies();
                var activePoliciesForObject = pip.GetPolicies(principal).GroupBy(o => o.Policy.Oid).Select(o => o.OrderBy(p => p.Rule).FirstOrDefault());

                // Create an effective policy list
                result = allPolicies.Select(masterPolicy =>
                {
                    // Get all policies which are related to this policy
                    var activePolicy = activePoliciesForObject.Where(p => masterPolicy.Oid.StartsWith(p.Policy.Oid)).OrderByDescending(o => o.Policy.Oid.Length).FirstOrDefault(); // The most specific policy grant for this policy

                    // What is the most specific policy in this tree?
                    if (activePolicy == null)
                    {
                        return new EffectivePolicyInstance(masterPolicy, PolicyGrantType.Deny, principal);
                    }
                    else
                    {
                        return new EffectivePolicyInstance(masterPolicy, activePolicy.Rule, principal);
                    }
                }).ToArray();

                this.m_adhocCacheService?.Add(cacheKey, result, new TimeSpan(0, 60, 0));
            }
            return result;
        }

        /// <summary>
        /// Compute cache key
        /// </summary>
        private string ComputeCacheKey(IPrincipal principal)
        {
            if (principal is IClaimsPrincipal cp)
            {
                if (cp.TryGetClaimValue(SanteDBClaimTypes.SanteDBSessionIdClaim, out string sessionId))
                {
                    return $"pdp.{this.m_hasher.ComputeHash(sessionId)}";
                }
                else if (cp.TryGetClaimValue(SanteDBClaimTypes.NameIdentifier, out string nameId))
                {
                    return $"pdp.{this.m_hasher.ComputeHash(nameId)}";
                }
            }
            return $"pdp.{this.m_hasher.ComputeHash(principal.Identity.Name)}";
        }

        /// <summary>
        /// Get a policy decision
        /// </summary>
        public PolicyDecision GetPolicyDecision(IPrincipal principal, object securable)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            else if (securable == null)
            {
                throw new ArgumentNullException(nameof(securable));
            }
            // We need to get the active policies for this
            var pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();
            IEnumerable<IPolicyInstance> securablePolicies = pip.GetPolicies(securable), principalPolicies = null;

            if (principal is IClaimsPrincipal cp)
            {
                principalPolicies = cp.GetGrantedPolicies(pip);
            }
            if(principalPolicies?.Any() != true)
            {
                principalPolicies = this.GetEffectivePolicySet(principal);
            }

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
                {
                    rule = PolicyGrantType.Deny;
                }

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
            {
                throw new ArgumentNullException(nameof(principal));
            }
            else if (String.IsNullOrEmpty(policyId))
            {
                throw new ArgumentNullException(nameof(policyId));
            }

            // Get the user object from the principal
            var pip = ApplicationServiceContext.Current.GetService<IPolicyInformationService>();

            // Can we make this decision based on the claims?
            if (principal is IClaimsPrincipal claimsPrincipal && claimsPrincipal.HasClaim(c => c.Type == SanteDBClaimTypes.SanteDBGrantedPolicyClaim)) // must adhere to the token
            {
                if (claimsPrincipal.HasClaim(c => c.Type == SanteDBClaimTypes.SanteDBGrantedPolicyClaim && policyId == c.Value))
                {
                    return PolicyGrantType.Grant;
                }
                else
                {
                    // Can override?
                    var polInfo = pip?.GetPolicy(policyId);
                    if (polInfo?.CanOverride == true && pip?.GetPolicyInstance(principal, PermissionPolicyIdentifiers.OverridePolicyPermission).Rule != PolicyGrantType.Deny)
                    {
                        return PolicyGrantType.Elevate;
                    }
                    else
                    {
                        return PolicyGrantType.Deny;
                    }
                }
            }
            else
            {
                // Most restrictive
                IPolicyInstance policyInstance = this.GetEffectivePolicySet(principal).FirstOrDefault(o => policyId == o.Policy.Oid);
                var retVal = PolicyGrantType.Deny;

                if (policyInstance == null)
                {
                    retVal = PolicyGrantType.Deny;
                }
                else if (!policyInstance.Policy.CanOverride && policyInstance.Rule == PolicyGrantType.Elevate)
                {
                    retVal = PolicyGrantType.Deny;
                }
                else if (!policyInstance.Policy.IsActive)
                {
                    retVal = PolicyGrantType.Grant;
                }
                else if ((policyInstance.Policy as IHandledPolicy)?.Handler != null)
                {
                    var policy = policyInstance.Policy as IHandledPolicy;
                    if (policy != null)
                    {
                        retVal = policy.Handler.GetPolicyDecision(principal, policy, null).Outcome;
                    }
                }
                else
                {
                    retVal = policyInstance.Rule;
                }

                return retVal;
            }
        }

        /// <summary>
        /// Clear cache by principal name
        /// </summary>
        public void ClearCache(string principalName)
        {
            this.m_adhocCacheService?.Remove($"pdp.{this.m_hasher.ComputeHash(principalName)}");
        }
    }
}