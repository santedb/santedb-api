using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Policy enforcement service
    /// </summary>
    public class DefaultPolicyEnforcementService : IPolicyEnforcementService
    {
        // Default policy decision service
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultPolicyDecisionService));

        // PDP Service
        private IPolicyDecisionService m_pdpService;

        /// <summary>
        /// Policy decision service
        /// </summary>
        public DefaultPolicyEnforcementService(IPolicyDecisionService pdpService)
        {
            this.m_pdpService = pdpService;
        }

        /// <summary>
        /// Default policy enforcement
        /// </summary>
        public string ServiceName => "Default Policy Enforcement Service";

        /// <summary>
        /// Perform a soft demand
        /// </summary>
        private PolicyGrantType GetGrant(IPrincipal principal, String policyId)
        {
            var action = PolicyGrantType.Deny;

            // Non system principals must be authenticated
            if (!principal.Identity.IsAuthenticated &&
                principal != AuthenticationContext.SystemPrincipal)
                return PolicyGrantType.Deny;
            else
            {
                action = this.m_pdpService.GetPolicyOutcome(principal, policyId);
            }

            this.m_tracer.TraceVerbose("Policy Enforce: {0}({1}) = {2}", principal?.Identity?.Name, policyId, action);

            return action;
        }

        /// <summary>
        /// Demand the policy
        /// </summary>
        public void Demand(string policyId)
        {
            this.Demand(policyId, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Demand policy enforcement
        /// </summary>
        public void Demand(string policyId, IPrincipal principal)
        {
            var result = this.GetGrant(principal, policyId);
            AuditUtil.AuditAccessControlDecision(principal, policyId, result);
            if (result != PolicyGrantType.Grant)
            {
                throw new PolicyViolationException(principal, policyId, result);
            }
        }

        /// <summary>
        /// Soft demand
        /// </summary>
        public bool SoftDemand(string policyId, IPrincipal principal)
        {
            return this.GetGrant(principal, policyId) == PolicyGrantType.Grant;
        }
    }
}