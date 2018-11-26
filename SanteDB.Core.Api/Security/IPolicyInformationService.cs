﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-6-28
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a contract for a policy information service
    /// </summary>
    public interface IPolicyInformationService
    {
        /// Get active policies for the specified securable type
        /// </summary>
        IEnumerable<IPolicyInstance> GetActivePolicies(object securable);

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
        void AddPolicies(Object securable, PolicyGrantType rule, params String[] policyOids);
    }


}
