/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Api.Security
{
    /// <summary>
    /// Security extensions used for 
    /// </summary>
    public static class SecurityExtensions
    {

        /// <summary>
        /// Represents a generic policy instance from a claims principal
        /// </summary>
        private class ClaimsPolicyInstance : IPolicyInstance
        {

            /// <summary>
            /// Create a new claims policy instance from claim
            /// </summary>
            public ClaimsPolicyInstance(IClaimsPrincipal securable, IPolicy policy)
            {
                this.Policy = policy;
                this.Securable = securable;
            }

            /// <summary>
            /// Gets the policy
            /// </summary>
            public IPolicy Policy { get; }

            /// <summary>
            /// Gets the rule
            /// </summary>
            public PolicyGrantType Rule => PolicyGrantType.Grant; // Only granted claims are in a claims principal

            /// <summary>
            /// The securable
            /// </summary>
            public object Securable { get; }
        }

        /// <summary>
        /// Convert an IPolicy to a policy instance
        /// </summary>
        public static SecurityPolicyInstance ToPolicyInstance(this IPolicyInstance me)
        {
            return new SecurityPolicyInstance(
                new SecurityPolicy()
                {
                    CanOverride = me.Policy.CanOverride,
                    Oid = me.Policy.Oid,
                    Name = me.Policy.Name
                },
                (PolicyGrantType)(int)me.Rule
            );
        }

        /// <summary>
        /// Gets the granted policies from the specified claims principal
        /// </summary>
        public static IEnumerable<IPolicyInstance> GetGrantedPolicies(this IClaimsPrincipal me, IPolicyInformationService pip)
        {
            return me.Claims.Where(o => o.Type == SanteDBClaimTypes.SanteDBGrantedPolicyClaim).Select(o => new ClaimsPolicyInstance(me, pip.GetPolicy(o.Value)));
        }

        /// <summary>
        /// As date time
        /// </summary>
        public static DateTime AsDateTime(this IClaim me)
        {
            DateTime value = DateTime.MinValue;
            if(!DateTime.TryParse(me.Value, out value))
            {
                int offset = 0;
                if (Int32.TryParse(me.Value, out offset))
                    value = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(offset).ToLocalTime();
                else
                    throw new ArgumentOutOfRangeException(nameof(IClaim.Value));
            }
            return value;
        }

        /// <summary>
        /// To Epoch time
        /// </summary>
        public static Int32 ToUnixEpoch(this DateTime me)
        {
            return (Int32)me.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
    }
}
