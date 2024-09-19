/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Security.Claims
{
    /// <summary>
    /// Utility for claim mapping
    /// </summary>
    public class ClaimMapper
    {

        private static ClaimMapper m_instance;
        private static object m_lock = new object();
        private readonly IDictionary<string, IClaimMapper[]> m_mappers;

        /// <summary>
        /// JWT tokens
        /// </summary>
        public static string ExternalTokenTypeJwt = "jwt";
        /// <summary>
        /// SAML token
        /// </summary>
        public static string ExternalTokenTypeSaml = "saml";

        /// <summary>
        /// Singleton constructor
        /// </summary>
        private ClaimMapper()
        {
            this.m_mappers = AppDomain.CurrentDomain.GetAllTypes().Where(t => typeof(IClaimMapper).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(t => Activator.CreateInstance(t) as IClaimMapper)
                .GroupBy(o => o.ExternalTokenFormat)
                .ToDictionary(o => o.Key, o => o.ToArray());
        }

        /// <summary>
        /// Get the current singleton for the claim mapper
        /// </summary>
        public static ClaimMapper Current
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_lock) // Lock to prevent multiple instantiations
                    {
                        if (m_instance == null)
                        {
                            m_instance = new ClaimMapper();
                        }
                    }
                }
                return m_instance;
            }
        }

        /// <summary>
        /// Try to get a claim mapper for the <paramref name="tokenFormat"/>
        /// </summary>
        /// <param name="tokenFormat">The token format to retrieve the claim mapper for</param>
        /// <param name="mapper">The mappers for the token type</param>
        /// <returns>True if the token format mapper exists</returns>
        public bool TryGetMapper(String tokenFormat, out IClaimMapper[] mapper) => this.m_mappers.TryGetValue(tokenFormat, out mapper);
    }
}
