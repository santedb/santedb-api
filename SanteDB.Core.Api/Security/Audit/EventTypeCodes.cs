using System;
using System.Collections.Generic;
using System.Text;
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
    }

#pragma warning restore CS1591
}
