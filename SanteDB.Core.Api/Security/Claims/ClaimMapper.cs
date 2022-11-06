using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                .GroupBy(o=> o.ExternalTokenFormat)
                .ToDictionary(o => o.Key, o => o.ToArray());
        }

        /// <summary>
        /// Get the current singleton for the claim mapper
        /// </summary>
        public static ClaimMapper Current
        {
            get
            {
                if(m_instance == null)
                {
                    lock(m_lock) // Lock to prevent multiple instantiations
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
