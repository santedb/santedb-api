/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Configuration;
using SanteDB.Core.Interop;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Security.Configuration
{

    /// <summary>
    /// Identifies the policy enforcement exception
    /// </summary>
    [XmlType(nameof(PolicyEnforcementExemptionPolicy), Namespace = "http://santedb.org/configuration")]
    public enum PolicyEnforcementExemptionPolicy
    {
        /// <summary>
        /// No exemptions
        /// </summary>
        [XmlEnum("none")]
        NoExemptions = 0,
        /// <summary>
        /// Devices exempt
        /// </summary>
        [XmlEnum("devices")]
        DevicePrincipalsExempt = 0x1,
        /// <summary>
        /// Users exempt
        /// </summary>
        [XmlEnum("humans")]
        UserPrincipalsExempt = 0x2,
        /// <summary>
        /// Users exempt
        /// </summary>
        [XmlEnum("applications")]
        ApplicationPrincipalsExempt = 0x4,
        /// <summary>
        /// All non-human
        /// </summary>
        [XmlEnum("non-human")] 
        ApplicationsOrDevicesExempt = DevicePrincipalsExempt | ApplicationPrincipalsExempt,
        /// <summary>
        /// Devices and humans are exempt
        /// </summary>
        [XmlEnum("all")]
        AllExempt = DevicePrincipalsExempt | UserPrincipalsExempt | ApplicationPrincipalsExempt
    }

    /// <summary>
    /// SanteDB Security configuration
    /// </summary>
    [XmlType(nameof(SecurityConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class SecurityConfigurationSection : IEncryptedConfigurationSection, IDisclosedConfigurationSection
    {
        public static readonly SecurityPolicyIdentification[] PUBLIC_POLICIES = new SecurityPolicyIdentification[]
        {
            SecurityPolicyIdentification.AbandonSessionAfterLockout,
            SecurityPolicyIdentification.AbandonSessionAfterPasswordReset,
            SecurityPolicyIdentification.AllowCachingOfUserCredentials,
            SecurityPolicyIdentification.AllowLocalDownstreamUserAccounts,
            SecurityPolicyIdentification.AllowNonAssignedUsersToLogin,
            SecurityPolicyIdentification.AllowPublicBackups,
            SecurityPolicyIdentification.AuditRetentionTime,
            SecurityPolicyIdentification.AuthenticationCookieValidityLength,
            SecurityPolicyIdentification.ChallengeHistory,
            SecurityPolicyIdentification.DownstreamLocalSessionLength,
            SecurityPolicyIdentification.ElevatedSessionLength,
            SecurityPolicyIdentification.MaxInvalidLogins,
            SecurityPolicyIdentification.MaxPasswordAge,
            SecurityPolicyIdentification.PasswordHistory,
            SecurityPolicyIdentification.RefreshLength,
            SecurityPolicyIdentification.RequireMfa,
            SecurityPolicyIdentification.RequireRsaCerts,
            SecurityPolicyIdentification.SessionLength
        };


        /// <summary>
        /// Password complexity requirements disclosure
        /// </summary>
        public const string PasswordValidationDisclosureName = "sec.pwd";

        /// <summary>
        /// Security configuration section
        /// </summary>
        public SecurityConfigurationSection()
        {
            this.Signatures = new List<SecuritySignatureConfiguration>();
            this.PasswordRegex = RegexPasswordValidator.DefaultPasswordPattern;
            this.TrustedCertificates = new ObservableCollection<string>();
            this.SecurityPolicy = new List<SecurityPolicyConfiguration>();
        }


        /// <summary>
        /// Password regex
        /// </summary>
        [XmlAttribute("passwordRegex")]
        [DisplayName("Password Regex")]
        [Description("Identifies the password regular expression")]
        public string PasswordRegex { get; set; }

        /// <summary>
        /// Policy enforcement policy
        /// </summary>
        [XmlAttribute("pepExemptionPolicy")]
        [DisplayName("PEP Exemption Policy")]
        [Description("Identifies the policy enforcement exception." +
            "When set, certain types of security principals will not be subject to PEP rules." +
            "DevicePrincipalsExempt indicates that userless principals should not be subject to PEP enforcement" +
            "UserPrincipalsExempt indicates that user principals should be should not be subject to PEP enforcement")]
        public PolicyEnforcementExemptionPolicy PepExemptionPolicy { get; set; }

        /// <summary>
        /// Signature configuration
        /// </summary>
        [XmlArray("signingKeys"), XmlArrayItem("add")]
        [Description("Describes the algorithm and key for signing data originating from this server")]
        [DisplayName("Data Signatures")]
        public List<SecuritySignatureConfiguration> Signatures { get; set; }

        /// <summary>
        /// Trusted publishers
        /// </summary>
        [XmlArray("trustedCertificates"), XmlArrayItem("add")]
        [DisplayName("Trusted Certificates")]
        [Description("Individual X.509 certificate thumbprints to trust")]
        public ObservableCollection<string> TrustedCertificates { get; set; }

        /// <summary>
        /// Set the specified policy
        /// </summary>
        public void SetPolicy(SecurityPolicyIdentification policyId, object policyValue)
        {
            var pol = this.SecurityPolicy.Find(o => o.PolicyId == policyId);
            if (pol == null)
            {
                this.SecurityPolicy.Add(new SecurityPolicyConfiguration(policyId, policyValue) {  Enabled = true });
            }
            else
            {
                pol.PolicyValue = policyValue;
                pol.Enabled = true;
            }
        }

        /// <summary>
        /// Gets or sets the security policy configuration
        /// </summary>
        [XmlArray("securityPolicy"), XmlArrayItem("add")]
        [DisplayName("Security policy configuration")]
        [Description("Policy configuration")]
        public List<SecurityPolicyConfiguration> SecurityPolicy { get; set; }

        /// <summary>
        /// Context key
        /// </summary>
        [XmlElement("context"), Browsable(false)]
        public byte[] ContextKey { get; set; }

        /// <summary>
        /// Gets the enabled security policy
        /// </summary>
        /// <param name="id">The identifier of the policy</param>
        /// <param name="defaultValue">The default value of the policy</param>
        /// <returns>The policy configuration</returns>
        public T GetSecurityPolicy<T>(SecurityPolicyIdentification id, T defaultValue = default(T))
        {
            var pol = this.SecurityPolicy?.Find(o => o.Enabled && o.PolicyId == id);
            if (pol == null)
            {
                return defaultValue;
            }
            else if (MapUtil.TryConvert(pol.PolicyValue, typeof(T), out object retVal))
            {
                return (T)retVal;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Turn this configuration section into something suitable for sharing
        /// </summary>
        public IEnumerable<AppSettingKeyValuePair> ForDisclosure()
        {
            yield return new AppSettingKeyValuePair(PasswordValidationDisclosureName, this.PasswordRegex);

            foreach(var pol in this.SecurityPolicy.Where(p => PUBLIC_POLICIES.Contains(p.PolicyId) && p.Enabled))
            {
                var serializationName = typeof(SecurityPolicyIdentification).GetField(pol.PolicyId.ToString()).GetCustomAttribute<XmlEnumAttribute>()?.Name;
                yield return new AppSettingKeyValuePair(serializationName, pol.PolicyValue.ToString());
            }

        }

        /// <inheritdoc/>
        public void Injest(IEnumerable<AppSettingKeyValuePair> settings)
        {
            this.PasswordRegex = settings.FirstOrDefault(o => o.Key == SecurityConfigurationSection.PasswordValidationDisclosureName)?.Value ??
                    this.PasswordRegex;

            foreach(var pol in PUBLIC_POLICIES)
            {
                var serializationName = typeof(SecurityPolicyIdentification).GetField(pol.ToString()).GetCustomAttribute<XmlEnumAttribute>()?.Name;
                var setting = settings.FirstOrDefault(o => o.Key == serializationName)?.Value;
                if(String.IsNullOrEmpty(setting))
                {
                    continue;
                }

                if(Int32.TryParse(setting, out var i32))
                {
                    this.SetPolicy(pol, i32);
                }
                else if(DateTime.TryParse(setting, out var dt))
                {
                    this.SetPolicy(pol, dt);
                }
                else if(TimeSpan.TryParse(setting, out var ts))
                {
                    this.SetPolicy(pol, ts);
                }
                else if(Guid.TryParse(setting, out var uuid))
                {
                    this.SetPolicy(pol, uuid);
                }
                else if(double.TryParse(setting, out var db))
                {
                    this.SetPolicy(pol, db);
                }
                else if(Boolean.TryParse(setting, out var bl))
                {
                    this.SetPolicy(pol, bl);
                }
                else
                {
                    this.SetPolicy(pol, setting);
                }
            }
        }

    }
}