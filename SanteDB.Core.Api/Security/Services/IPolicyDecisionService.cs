/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a policy decision service
    /// </summary>
    [System.ComponentModel.Description("Policy Decision Provider (PDP)")]
    public interface IPolicyDecisionService : IServiceImplementation
    {

        /// <summary>
        /// Get all active policies for the specified securable type
        /// </summary>
        IEnumerable<IPolicyInstance> GetEffectivePolicySet(IPrincipal securable);

        /// <summary>
        /// Make a simple policy decision for a specific securable
        /// </summary>
        PolicyDecision GetPolicyDecision(IPrincipal principal, Object securable);

        /// <summary>
        /// Get a policy decision outcome (i.e. make a policy decision)
        /// </summary>
        PolicyGrantType GetPolicyOutcome(IPrincipal principal, string policyId);

        /// <summary>
        /// Clear the policy cache for the specified principal
        /// </summary>
        void ClearCache(IPrincipal principal);

        /// <summary>
        /// Clear the policy cache for the specified principal
        /// </summary>
        void ClearCache(String principalName);

    }
}

