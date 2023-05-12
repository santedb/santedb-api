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
 * Date: 2023-3-10
 */
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Security extensions used for
    /// </summary>
    public static class SecurityExtensions
    {
        // Trust Certificates
        private static X509Certificate2Collection s_trustCertificates;

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
        //public static DateTime AsDateTime(this IClaim me)
        //{
        //    if (!DateTime.TryParse(me.Value, out var value))
        //    {
        //        if (Int32.TryParse(me.Value, out int offset))
        //        {
        //            value = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(offset).ToLocalTime();
        //        }
        //        else
        //        {
        //            throw new ArgumentOutOfRangeException(nameof(IClaim.Value));
        //        }
        //    }
        //    return value;
        //}

        /// <summary>
        /// Get all internal certificates
        /// </summary>
        private static IEnumerable<X509Certificate2> GetInternalCertificates()
        {
            // Chain isn't valid but we should allow SSI code signing cert if it appears in the chain
            foreach (var crt in typeof(SecurityExtensions).Assembly.GetManifestResourceNames().Where(o => o.StartsWith(typeof(SecurityExtensions).Namespace)))
            {
                using (var x509Stream = typeof(SecurityExtensions).Assembly.GetManifestResourceStream(crt))
                {
                    byte[] certData = new byte[x509Stream.Length];
                    x509Stream.Read(certData, 0, (int)x509Stream.Length);
                    yield return new X509Certificate2(certData);
                }
            }
        }

        /// <summary>
        /// Install certificates for chain - this happens in the local user context
        /// </summary>
        public static void InstallCertsForChain()
        {
            try
            {
                using (var store = new X509Store(StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    // Chain isn't valid but we should allow SSI code signing cert if it appears in the chain
                    foreach (var crt in typeof(SecurityExtensions).Assembly.GetManifestResourceNames().Where(o => o.StartsWith(typeof(SecurityExtensions).Namespace)))
                    {
                        using (var x509Stream = typeof(SecurityExtensions).Assembly.GetManifestResourceStream(crt))
                        {
                            byte[] certData = new byte[x509Stream.Length];
                            x509Stream.Read(certData, 0, (int)x509Stream.Length);
                            var certTrust = new X509Certificate2(certData);

                            if (store.Certificates.Find(X509FindType.FindBySubjectName, certTrust.Subject, false).Count == 0)
                            {
                                var storeName = StoreName.My;

                                if (crt.StartsWith($"{typeof(SecurityExtensions).Namespace}.Certs.Trust"))
                                {
                                    storeName = StoreName.AuthRoot;
                                }
                                else if (crt.StartsWith($"{typeof(SecurityExtensions).Namespace}.Certs.Inter"))
                                {
                                    storeName = StoreName.CertificateAuthority;
                                }

                                X509CertificateUtils.GetPlatformServiceOrDefault().TryInstallCertificate(certTrust, storeName);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new SecurityException("Unable to install SanteDB's trusted certificates", e);
            }
        }

        /// <summary>
        /// Returns true if the certificate is not trusted by the operating system but is trusted by an internal store
        /// </summary>
        public static bool HasTrustedRootCert(this X509Chain chain)
        {
            if (s_trustCertificates == null)
            {
                s_trustCertificates = new X509Certificate2Collection();
                foreach (var crt in typeof(SecurityExtensions).Assembly.GetManifestResourceNames().Where(o => o.StartsWith($"{typeof(SecurityExtensions).Namespace}.Certs.Trust")))
                {
                    using (var x509Stream = typeof(SecurityExtensions).Assembly.GetManifestResourceStream(crt))
                    {
                        byte[] certData = new byte[x509Stream.Length];
                        x509Stream.Read(certData, 0, (int)x509Stream.Length);
                        var certTrust = new X509Certificate2(certData);
                        s_trustCertificates.Add(certTrust);
                    }
                }
            }
            return s_trustCertificates.Find(X509FindType.FindByThumbprint, chain.ChainElements[chain.ChainElements.Count - 1].Certificate.Thumbprint, false).Count > 0;
        }

        /// <summary>
        /// Returns true if the certificate <paramref name="me"/> is trusted by a SanteSuite certificate
        /// </summary>
        public static bool IsTrustedIntern(this X509Certificate2 me, X509Certificate2Collection extraCerts, out IEnumerable<X509ChainStatus> chainStatus)
        {
            var chain = X509Chain.Create();
            chain.ChainPolicy = new X509ChainPolicy()
            {
                RevocationMode = X509RevocationMode.NoCheck
            };

            if (extraCerts != null)
            {
                chain.ChainPolicy.ExtraStore.AddRange(extraCerts);
            }
            chain.ChainPolicy.ExtraStore.AddRange(GetInternalCertificates().ToArray());

            var retVal = chain.Build(me);
            chainStatus = chain.ChainStatus;

            if (!retVal)
            {
                using (var trustedPublisherStore = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine))
                {
                    trustedPublisherStore.Open(OpenFlags.ReadOnly);
                    retVal = trustedPublisherStore.Certificates.Find(X509FindType.FindBySubjectName, me.Subject, false).Count > 0;
                }
            }

            // Check for my own certificates
            if (!retVal)
            {
                using (var myUserStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    myUserStore.Open(OpenFlags.ReadOnly);
                    retVal = myUserStore.Certificates.Find(X509FindType.FindByThumbprint, me.Thumbprint, false).Count > 0;
                }
            }
            return retVal || HasTrustedRootCert(chain);
        }

        /// <summary>
        /// Gets the first claim value out of an <see cref="IClaimsIdentity"/> searching in the order of claim types provided in the <paramref name="claimTypes"/> parameters.
        /// </summary>
        /// <param name="identity">The identity to search</param>
        /// <param name="claimTypes">the claim types to search</param>
        /// <returns>A value for the first instance of the first claim type found or null.</returns>
        public static string GetFirstClaimValue(this IClaimsIdentity identity, params string[] claimTypes)
        {
            if (null == identity || null == claimTypes || claimTypes.Length == 0)
            {
                return null;
            }

            IClaim claim = null;
            foreach (var claimtype in claimTypes)
            {
                claim = identity.FindFirst(claimtype);

                if (null != claim)
                {
                    return claim.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first value of a matching claim type from an <see cref="IClaimsPrincipal"/> searching in the order specified in the parameters.
        /// </summary>
        /// <param name="principal">The claims principal to search.</param>
        /// <param name="claimTypes">The ordered set of claim types to search.</param>
        /// <returns>A value for the first instance of the first claim type found or null.</returns>
        public static string GetFirstClaimValue(this IClaimsPrincipal principal, params string[] claimTypes)
        {
            if (null == principal || null == claimTypes || claimTypes.Length == 0)
            {
                return null;
            }

            IClaim claim = null;
            foreach (var claimtype in claimTypes)
            {
                claim = principal.FindFirst(claimtype);

                if (null != claim)
                {
                    return claim.Value;
                }
            }

            return null;
        }
    }
}