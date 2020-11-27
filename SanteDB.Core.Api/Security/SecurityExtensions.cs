/*
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using System;

namespace SanteDB.Core.Api.Security
{
    /// <summary>
    /// Security extensions used for 
    /// </summary>
    public static class SecurityExtensions
    {
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
