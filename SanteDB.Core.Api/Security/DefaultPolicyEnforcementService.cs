﻿/*
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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using System;
using System.Security.Principal;

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
            {
                return PolicyGrantType.Deny;
            }
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