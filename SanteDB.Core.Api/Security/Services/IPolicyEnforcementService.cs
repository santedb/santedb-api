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
using SanteDB.Core.Exceptions;
using SanteDB.Core.Services;
using System;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a PEP that receives demands
    /// </summary>
    [System.ComponentModel.Description("Policy Enforcement Provider (PEP)")]
    public interface IPolicyEnforcementService : IServiceImplementation
    {

        /// <summary>
        /// Demand access to the policy for the current <see cref="AuthenticationContext.Current"/>
        /// </summary>
        /// <param name="policyId">The identifier OID of the policy for which permission is being demanded</param>
        /// <exception cref="PolicyViolationException">When <see cref="AuthenticationContext.Current"/> does not have permission to <paramref name="policyId"/></exception>
        void Demand(string policyId);

        /// <summary>
        /// Demand access to the policy on behalf of principal
        /// </summary>
        /// <param name="policyId">The identifier OID of the policy for which permission is being demanded</param>
        /// <param name="principal">The principal for which permission is being demanded</param>
        /// <exception cref="PolicyViolationException">When <paramref name="principal"/> does not have permission to <paramref name="policyId"/></exception>
        void Demand(String policyId, IPrincipal principal);

        /// <summary>
        /// Demand the specified policy and return the result
        /// </summary>
        /// <remarks>This method differs from <see cref="Demand(string, IPrincipal)"/> in that:
        /// <list type="bullet">
        ///     <item>It does not throw a <see cref="PolicyViolationException"/></item>
        ///     <item>It does not audit the access control decision</item>
        /// </list>
        /// Callers should use this to "test" if a principal has permission without generating an audit. If an audit
        /// is desired, or a decision to halt a process needed, use <see cref="Demand(string, IPrincipal)"/>
        /// </remarks>
        /// <param name="policyId">The policy OID for which the demand is occurring</param>
        /// <param name="principal">The principal for which the policy should be demanded</param>
        /// <returns>True if <paramref name="principal"/> demand for <paramref name="policyId"/> succeeded </returns>
        bool SoftDemand(String policyId, IPrincipal principal);
    }
}
