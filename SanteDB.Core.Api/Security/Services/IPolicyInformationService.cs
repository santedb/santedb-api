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
using SanteDB.Core.Model.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a contract for a policy information service
    /// </summary>
    [System.ComponentModel.Description("Policy Information Provider (PIP)")]
    public interface IPolicyInformationService : IServiceImplementation
    {

        /// <summary>
        /// Get all active policies for the specified securable type
        /// </summary>
        IEnumerable<IPolicyInstance> GetPolicies(object securable);

        /// <summary>
        /// Get all policies on the system
        /// </summary>
        IEnumerable<IPolicy> GetPolicies();

        /// <summary>
        /// Get a specific policy
        /// </summary>
        IPolicy GetPolicy(string policyOid);

        /// <summary>
        /// Adds the specified policies to the specified securable object
        /// </summary>
        /// <param name="securable">The object to which policies should be added</param>
        /// <param name="rule">The rule to be applied to the securable</param>
        /// <param name="policyOids">The oids of the policies to add</param>
        /// <param name="principal">The principal which is adding policies ot the <paramref name="securable"/></param>
        void AddPolicies(Object securable, PolicyGrantType rule, IPrincipal principal, params String[] policyOids);

        /// <summary>
        /// Gets the policy instance for the specified object
        /// </summary>
        IPolicyInstance GetPolicyInstance(object securable, string policyOid);

        /// <summary>
        /// Returns true if <paramref name="securable"/> has <paramref name="policyOid"/> assigned to it
        /// </summary>
        /// <param name="securable">The securable to check</param>
        /// <param name="policyOid">The policy OID to check</param>
        /// <returns>True if <paramref name="securable"/> has <paramref name="policyOid"/></returns>
        bool HasPolicy(object securable, string policyOid);

        /// <summary>
        /// Removes the specified policies from the user account
        /// </summary>
        void RemovePolicies(Object securable, IPrincipal principal, params string[] oid);
    }


}

