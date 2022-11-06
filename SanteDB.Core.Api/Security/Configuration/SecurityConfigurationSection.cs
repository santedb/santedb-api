﻿/*
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
using SanteDB.Core.Configuration;
using SanteDB.Core.Model.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        /// Devices and humans are exempt
        /// </summary>
        [XmlEnum("all")]
        AllExempt = DevicePrincipalsExempt | UserPrincipalsExempt
    }

    /// <summary>
    /// SanteDB Security configuration
    /// </summary>
    [XmlType(nameof(SecurityConfigurationSection), Namespace = "http://santedb.org/configuration")]
    public class SecurityConfigurationSection : IEncryptedConfigurationSection
    {
        /// <summary>
        /// Password complexity requirements disclosure
        /// </summary>
        public const string PasswordValidationDisclosureName = "sec.pwd";
        /// <summary>
        /// Local accounts allowed on device policy
        /// </summary>
        public const string LocalAccountAllowedDisclosureName = "sec.local";
        /// <summary>
        /// Session length policy
        /// </summary>
        public const string LocalSessionLengthDisclosureName = "sec.ses";

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
                this.SecurityPolicy.Add(new SecurityPolicyConfiguration(policyId, policyValue));
            }
            else
            {
                pol.PolicyValue = policyValue;
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
            yield return new AppSettingKeyValuePair(LocalAccountAllowedDisclosureName, this.GetSecurityPolicy(SecurityPolicyIdentification.AllowLocalDownstreamUserAccounts, false).ToString());
            yield return new AppSettingKeyValuePair(LocalSessionLengthDisclosureName, this.GetSecurityPolicy(SecurityPolicyIdentification.DownstreamLocalSessionLength, new TimeSpan(0, 30, 0).ToString()));
        }

    }
}