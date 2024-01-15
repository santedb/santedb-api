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
 * Date: 2023-5-19
 */
using SanteDB.Core.Model.Audit;

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// Extended audit codes for use in audit messages
    /// </summary>
    public static class ExtendedAuditCodes
    {
#pragma warning disable CS1591
        public static readonly AuditCode ActorRoleDestination = new AuditCode("110152", "DCM") { DisplayName = "Destination" };
        public static readonly AuditCode ActorRoleSource = new AuditCode("110153", "DCM") { DisplayName = "Source" };
        public static readonly AuditCode ActorRoleApplication = new AuditCode("110150", "DCM") { DisplayName = "Application" };
        public static readonly AuditCode ActorRoleDevice = new AuditCode("DEV", "http://terminology.hl7.org/CodeSystem/v3-EntityClass") { DisplayName = "Device" };
        public static readonly AuditCode ActorRoleCustodian = new AuditCode("CST", "http://terminology.hl7.org/CodeSystem/v3-ParticipationType") { DisplayName = "Custodian" };
        public static readonly AuditCode ActorRoleHuman = new AuditCode("humanuser", "http://terminology.hl7.org/CodeSystem/extra-security-role-type") { DisplayName = "Human User" };

        public static readonly AuditCode EventTypeMasking = new AuditCode("SecurityAuditCode-Masking", "SecurityAuditCode") { DisplayName = "Mask Sensitive Data" };
        public static readonly AuditCode EventTypeCreate = new AuditCode("SecurityAuditCode-CreateInstances", "SecurityAuditCode") { DisplayName = "Create Data" };
        public static readonly AuditCode EventTypeUpdate = new AuditCode("SecurityAuditCode-UpdateInstances", "SecurityAuditCode") { DisplayName = "Update Data" };
        public static readonly AuditCode EventTypeDelete = new AuditCode("SecurityAuditCode-DeleteInstances", "SecurityAuditCode") { DisplayName = "Delete Data" };
        public static readonly AuditCode EventTypeSynchronization = new AuditCode("SecurityAuditCode-Synchronization", "SecurityAuditCode") { DisplayName = "Mask Sensitive Data" };
        public static readonly AuditCode EventTypeDataManagement = new AuditCode("SecurityAuditCode-DataManagement", "SecurityAuditCode") { DisplayName = "Data Management Activities" };
        public static readonly AuditCode EventTypeSecurityAlert = new AuditCode("110113", "DCM") { DisplayName = "Security Alert" };

        public static readonly AuditCode CustomIdTypeHttpHeaders = new AuditCode("HTTP-Headers", "SecurityAuditCodes");
        public static readonly AuditCode CustomIdTypeAuditObject = new AuditCode("SecurityAudit", "http://santedb.org/model");
        public static readonly AuditCode CustomIdTypePolicy = new AuditCode("SecurityPolicy", "http://santedb.org/model");
        public static readonly AuditCode CustomIdTypeProvider = new AuditCode("PVD", "http://terminology.hl7.org/CodeSystem/v3-EntityClass") { DisplayName = "Provider" };
        public static readonly AuditCode CustomIdTypeOrganization = new AuditCode("ORG", "http://terminology.hl7.org/CodeSystem/v3-EntityClass") { DisplayName = "Organization" };
        public static readonly AuditCode CustomIdTypePatient = new AuditCode("PAT", "http://terminology.hl7.org/CodeSystem/v3-EntityClass") { DisplayName = "Patient" };
        public static readonly AuditCode CustomIdTypeEntity = new AuditCode("ENT", "http://terminology.hl7.org/CodeSystem/v3-EntityClass") { DisplayName = "Entity" };
        public static readonly AuditCode CustomIdTypeAct = new AuditCode("ACT", "http://terminology.hl7.org/CodeSystem/v3-EntityClass") { DisplayName = "Act" };
        public static readonly AuditCode CustomIdTypePlace = new AuditCode("PLC", "http://terminology.hl7.org/CodeSystem/v3-EntityClass") { DisplayName = "Place" };
        public static readonly AuditCode CustomIdTypePolicyDecision = new AuditCode("PolicyDecision", "http://santedb.org/model");
        public static readonly AuditCode CustomIdTypeForeignFile = new AuditCode("ForeignDataFile", "http://santedb.org/model") { DisplayName = "External Data File" };
        public static readonly AuditCode CustomIdTypeSession = new AuditCode("SecuritySession", "http://santedb.org/model") { DisplayName = "Security Session" };
        public static readonly AuditCode DataQualityConfiguration = new AuditCode("DataQualityConfiguration", "http://santedb.org/configuration") { DisplayName = "Data Quality Configuration" };
        public static readonly AuditCode CustomIdTypeMaskedFields = new AuditCode("SecurityUsitCode-MaskedFields", "SecurityAuditCode") { DisplayName = "Masked Fields" };
#pragma warning restore CS1591

    }
}
