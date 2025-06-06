﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using static System.Collections.Specialized.BitVector32;

#pragma warning disable CS0612
namespace SanteDB.Core.Security
{
    /// <summary>
    /// Policy enforcement service
    /// </summary>
    public class DefaultPolicyEnforcementService : IPolicyEnforcementService
    {
        // Default policy decision service
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(DefaultPolicyEnforcementService));

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

            if (principal != AuthenticationContext.SystemPrincipal)
            {
                ApplicationServiceContext.Current.GetAuditService().Audit().ForAccessControlDecision(principal, policyId, result).Send();
            }

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

        /// <inheritdoc/>
        public void DemandAny(params string[] policyIds)
        {
            var principal = AuthenticationContext.Current.Principal;
            var grants = policyIds.ToDictionary(o => o, o => this.GetGrant(principal, o));
            var result = grants.Values.Max();

            if (principal != AuthenticationContext.SystemPrincipal)
            {
                ApplicationServiceContext.Current.GetAuditService().Audit().WithAction(ActionType.Execute)
                    .WithOutcome(result == PolicyGrantType.Grant ? OutcomeIndicator.Success : result == PolicyGrantType.Elevate ? OutcomeIndicator.MinorFail : OutcomeIndicator.SeriousFail)
                    .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                    .WithEventType(EventTypeCodes.AccessControlDecision)
                    .WithLocalSource()
                    .WithPrincipal(principal)
                    .WithAuditableObjects(grants.Select(g=>new AuditableObject()
                    {
                        IDTypeCode = AuditableObjectIdType.Custom,
                        CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypePolicy,
                        ObjectId = g.Key,
                        Role = AuditableObjectRole.SecurityGranularityDefinition,
                        Type = AuditableObjectType.SystemObject,
                        ObjectData = new List<ObjectDataExtension>() { new ObjectDataExtension("G", g.Value.ToString()) }
                    }));
            }

            if (result != PolicyGrantType.Grant)
            {
                throw new PolicyViolationException(principal, String.Join("|", policyIds), result);
            }
        }

        /// <inheritdoc/>
        public void DemandAll(params string[] policyIds)
        {
            var principal = AuthenticationContext.Current.Principal;
            var grants = policyIds.ToDictionary(o => o, o => this.GetGrant(principal, o));
            var result = grants.Values.Min();

            if (principal != AuthenticationContext.SystemPrincipal)
            {
                ApplicationServiceContext.Current.GetAuditService().Audit().WithAction(ActionType.Execute)
                    .WithOutcome(result == PolicyGrantType.Grant ? OutcomeIndicator.Success : result == PolicyGrantType.Elevate ? OutcomeIndicator.MinorFail : OutcomeIndicator.SeriousFail)
                    .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                    .WithEventType(EventTypeCodes.AccessControlDecision)
                    .WithLocalSource()
                    .WithPrincipal(principal)
                    .WithAuditableObjects(grants.Select(g => new AuditableObject()
                    {
                        IDTypeCode = AuditableObjectIdType.Custom,
                        CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypePolicy,
                        ObjectId = g.Key,
                        Role = AuditableObjectRole.SecurityGranularityDefinition,
                        Type = AuditableObjectType.SystemObject,
                        ObjectData = new List<ObjectDataExtension>() { new ObjectDataExtension("G", g.Value.ToString()) }
                    }));
            }

            if (result != PolicyGrantType.Grant)
            {
                throw new PolicyViolationException(principal, String.Join("&", policyIds), result);
            }
        }
    }
}
#pragma warning restore