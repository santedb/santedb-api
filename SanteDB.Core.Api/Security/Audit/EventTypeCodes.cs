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
using System.Xml.Serialization;

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// Event type codes
    /// </summary>
#pragma warning disable CS1591

    public enum EventTypeCodes
    {
        [XmlEnum("SecurityAuditCode-ApplicationActivity")]
        ApplicationActivity,

        [XmlEnum("SecurityAuditCode-AuditLogUsed")]
        AuditLogUsed,

        [XmlEnum("SecurityAuditCode-Export")]
        Export,

        [XmlEnum("SecurityAuditCode-Import")]
        Import,

        [XmlEnum("SecurityAuditCode-NetworkActivity")]
        NetworkActivity,

        [XmlEnum("SecurityAuditCode-OrderRecord")]
        OrderRecord,

        [XmlEnum("SecurityAuditCode-PatientRecord")]
        PatientRecord,

        [XmlEnum("SecurityAuditCode-ProcedureRecord")]
        ProcedureRecord,

        [XmlEnum("SecurityAuditCode-Query")]
        Query,

        [XmlEnum("SecurityAuditCode-SecurityAlert")]
        SecurityAlert,

        [XmlEnum("SecurityAuditCode-UserAuthentication")]
        UserAuthentication,

        [XmlEnum("SecurityAuditCode-ApplicationStart")]
        ApplicationStart,

        [XmlEnum("SecurityAuditCode-ApplicationStop")]
        ApplicationStop,

        [XmlEnum("SecurityAuditCode-Login")]
        Login,

        [XmlEnum("SecurityAuditCode-Logout")]
        Logout,

        [XmlEnum("SecurityAuditCode-Attach")]
        Attach,

        [XmlEnum("SecurityAuditCode-Detach")]
        Detach,

        [XmlEnum("SecurityAuditCode-NodeAuthentication")]
        NodeAuthentication,

        [XmlEnum("SecurityAuditCode-EmergencyOverrideStarted")]
        EmergencyOverrideStarted,

        [XmlEnum("SecurityAuditCode-Useofarestrictedfunction")]
        UseOfARestrictedFunction,

        [XmlEnum("SecurityAuditCode-Securityattributeschanged")]
        SecurityAttributesChanged,

        [XmlEnum("SecurityAuditCode-Securityroleschanged")]
        SecurityRolesChanged,

        [XmlEnum("SecurityAuditCode-SecurityObjectChanged")]
        SecurityObjectChanged,

        [XmlEnum("SecurityAuditCode-AuditLoggingStarted")]
        AuditLoggingStarted,

        [XmlEnum("SecurityAuditCode-AuditLoggingStopped")]
        AuditLoggingStopped,

        [XmlEnum("SecurityAuditCode-SessionStarted")]
        SessionStarted,

        [XmlEnum("SecurityAuditCode-SessionStopped")]
        SessionStopped,

        [XmlEnum("SecurityAuditCode-AccessControlDecision")]
        AccessControlDecision,

        [XmlEnum("SecurityAuditCode-SecondaryUseQuery")]
        SecondaryUseQuery,

        [XmlEnum("SecurityAuditCode-ConfigurationChanged")]
        ConfigurationChanged,

        [XmlEnum("SecurityAuditCode-SecurityConfigurationChanged")]
        SecurityConfigurationChanged,

    }

#pragma warning restore CS1591
}
