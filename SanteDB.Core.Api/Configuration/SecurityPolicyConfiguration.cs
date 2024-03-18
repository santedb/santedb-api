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
 * User: fyfej
 * Date: 2023-6-21
 */
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{

    /// <summary>
    /// Policy identification
    /// </summary>
    [XmlType(nameof(SecurityPolicyIdentification), Namespace = "http://santedb.org/configuration")]
    public enum SecurityPolicyIdentification
    {
        /// <summary>
        /// Maximum password age
        /// </summary>
        [XmlEnum("auth.pwd.maxAge")]
        MaxPasswordAge,
        /// <summary>
        /// When changing a password question must be different
        /// </summary>
        [XmlEnum("auth.pwd.history")]
        PasswordHistory,
        /// <summary>
        /// Maximum invalid logins
        /// </summary>
        [XmlEnum("auth.failLogin")]
        MaxInvalidLogins,
        /// <summary>
        /// Maximum challenge age
        /// </summary>
        [XmlEnum("auth.challenge.maxAge")]
        MaxChallengeAge,
        /// <summary>
        /// When changing a challenge question must be different
        /// </summary>
        [XmlEnum("auth.challenge.history")]
        ChallengeHistory,
        /// <summary>
        /// Maximum length of sesison
        /// </summary>
        [XmlEnum("auth.session.length")]
        SessionLength,
        /// <summary>
        /// Maximum time to refresh session
        /// </summary>
        [XmlEnum("auth.session.refresh")]
        RefreshLength,
        /// <summary>
        /// The place where authentication certificates should be stored
        /// </summary>
        [XmlEnum("auth.cert.location")]
        DefaultCertificateInstallLocation,
        /// <summary>
        /// How long the authentication cookie is valid for.
        /// </summary>
        [XmlEnum("auth.cookie.length")]
        AuthenticationCookieValidityLength,
        /// <summary>
        /// Length of local sessions on dCDR
        /// </summary>
        [XmlEnum("downstream.session.length")]
        DownstreamLocalSessionLength,
        /// <summary>
        /// True to allow dCDRs to create their own local accounts
        /// </summary>
        [XmlEnum("downstream.user.accounts")]
        AllowLocalDownstreamUserAccounts,
        /// <summary>
        /// Indicates that the local is permitted to cache successful login credentials
        /// </summary>
        [XmlEnum("local.user.cache")]
        AllowCachingOfUserCredentials,
        /// <summary>
        /// Indicates that only users from the <see cref="AssignedFacilityUuid"/> are permitted to login
        /// </summary>
        [XmlEnum("local.user.anyUserLogin")]
        AllowNonAssignedUsersToLogin,
        /// <summary>
        /// Indicates the owner facility 
        /// </summary>
        [XmlEnum("uuid.facility")]
        AssignedFacilityUuid,
        /// <summary>
        /// Indicates the owner user
        /// </summary>
        [XmlEnum("uuid.owner")]
        AssignedOwnerUuid,
        /// <summary>
        /// Length of time that security auidts should be retained
        /// </summary>
        [XmlEnum("audit.retention")]
        AuditRetentionTime,
        /// <summary>
        /// The device principal
        /// </summary>
        [XmlEnum("uuid.devicePrincipal")]
        AssignedDeviceSecurityId,
        /// <summary>
        /// The device entity identifier
        /// </summary>
        [XmlEnum("uuid.device")]
        AssignedDeviceEntityId,
        /// <summary>
        /// Abandon session on PWD change
        /// </summary>
        [XmlEnum("session.abandon.pwd")]
        AbandonSessionAfterPasswordReset,
        /// <summary>
        /// Users must have MFA to their registered e-mail address
        /// </summary>
        [XmlEnum("auth.mfa.required")]
        ForceMfa,
        /// <summary>
        /// Default MFA method for the system if the user has selected none
        /// </summary>
        [XmlEnum("auth.mfa.default")]
        DefaultMfaMethod,
        /// <summary>
        /// True if public backups are permitted
        /// </summary>
        [XmlEnum("backup.public")]
        AllowPublicBackups

    }

    /// <summary>
    /// Gets the policy value as a timespan
    /// </summary>
    public class PolicyValueTimeSpan
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public PolicyValueTimeSpan()
        {

        }

        /// <summary>
        /// Timepsan value
        /// </summary>
        public PolicyValueTimeSpan(TimeSpan ts)
        {
            this.Value = ts;
        }

        /// <summary>
        /// Policy value timespan
        /// </summary>
        public PolicyValueTimeSpan(int hours, int minutes, int seconds)
        {
            this.Value = new TimeSpan(hours, minutes, seconds);
        }

        /// <summary>
        /// Time to live for XML serialization
        /// </summary>
        [XmlText]
        public string ValueXml
        {
            get { return XmlConvert.ToString(this.Value); }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.Value = XmlConvert.ToTimeSpan(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the timespan
        /// </summary>
        [XmlIgnore]
        public TimeSpan Value { get; set; }

        /// <summary>
        /// Convert this wrapper to timespan
        /// </summary>
        public static explicit operator TimeSpan(PolicyValueTimeSpan instance)
        {
            return instance.Value;
        }

        /// <summary>
        /// Convert this wrapper to timespan
        /// </summary>
        public static explicit operator PolicyValueTimeSpan(TimeSpan instance)
        {
            return new PolicyValueTimeSpan() { Value = instance };
        }

    }

    /// <summary>
    /// Security policies
    /// </summary>
    [XmlType(nameof(SecurityPolicyConfiguration), Namespace = "http://santedb.org/configuration")]
    public class SecurityPolicyConfiguration
    {
        private object m_policyValue;

        /// <summary>
        /// Default ctor for serialization
        /// </summary>
        public SecurityPolicyConfiguration()
        {

        }

        /// <summary>
        /// Policy configuration ctor
        /// </summary>
        public SecurityPolicyConfiguration(SecurityPolicyIdentification policy, object value)
        {
            this.PolicyId = policy;
            this.PolicyValue = value;
            this.Enabled = true;
        }

        /// <summary>
        /// True if the policy is enabled
        /// </summary>
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// The policy type
        /// </summary>
        [XmlAttribute("policy")]
        public SecurityPolicyIdentification PolicyId { get; set; }

        /// <summary>
        /// The policy value
        /// </summary>
        [XmlElement("int", typeof(Int32))]
        [XmlElement("timespan", typeof(PolicyValueTimeSpan))]
        [XmlElement("date", typeof(DateTime))]
        [XmlElement("list", typeof(List<String>))]
        [XmlElement("string", typeof(String))]
        [XmlElement("guid", typeof(Guid))]
        [XmlElement("bool", typeof(Boolean))]
        [XmlElement("real", typeof(double))]
        public Object PolicyValue
        {
            get => this.m_policyValue;
            set
            {
                if (value is TimeSpan ts)
                {
                    this.m_policyValue = new PolicyValueTimeSpan(ts);
                }
                else
                {
                    this.m_policyValue = value;
                }
            }
        }

    }
}