﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 *
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
 * User: JustinFyfe
 * Date: 2019-1-22
 */
using SanteDB.Core.Auditing;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
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
        ApplicationActtivity,
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

    }
    #pragma warning restore CS1591

    /// <summary>
    /// Security utility
    /// </summary>
    public static class AuditUtil
    {

        private static Tracer traceSource = Tracer.GetTracer(typeof(AuditUtil));

        /// <summary>
        /// Audit that the audit log was used
        /// </summary>
        /// <param name="action"></param>
        /// <param name="outcome"></param>
        /// <param name="query"></param>
        /// <param name="auditIds"></param>
        public static void AuditAuditLogUsed(ActionType action, OutcomeIndicator outcome, String query, String remoteAddress, params Guid[] auditIds)
        {
            traceSource.TraceVerbose("Create AuditLogUsed audit");
            AuditData audit = new AuditData(DateTime.Now, action, outcome, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.AuditLogUsed));

            // User actors
            AddLocalDeviceActor(audit);
            AddUserActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);
            // Add objects to which the thing was done
            audit.AuditableObjects = auditIds.Select(o => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.ReportNumber,
                LifecycleType = action == ActionType.Delete ? AuditableObjectLifecycle.PermanentErasure : AuditableObjectLifecycle.Disclosure,
                ObjectId = o.ToString(),
                Role = AuditableObjectRole.SecurityResource,
                Type = AuditableObjectType.SystemObject
            }).ToList();

            if (!String.IsNullOrEmpty(query))
            {
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.SearchCritereon,
                    LifecycleType = AuditableObjectLifecycle.Access,
                    QueryData = query,
                    Role = AuditableObjectRole.Query,
                    Type = AuditableObjectType.SystemObject
                });
            }

            SendAudit(audit);

        }

        /// <summary>
        /// Audit that security objects were created
        /// </summary>
        public static void AuditSecurityCreationAction(IEnumerable<object> objects, bool success, IEnumerable<string> changedProperties, String remoteAddress)
        {
            traceSource.TraceVerbose("Create SecurityCreationAction audit");

            var audit = new AuditData(DateTime.Now, ActionType.Create, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityObjectChanged));
            AddLocalDeviceActor(audit);
            AddUserActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);

            audit.AuditableObjects = objects.Select(obj => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Custom,
                CustomIdTypeCode = new AuditCode(obj.GetType().Name, "SanteDBTable"),
                ObjectId = ((obj as IIdentifiedEntity)?.Key ?? Guid.Empty).ToString(),
                LifecycleType = AuditableObjectLifecycle.Creation,
                Role = AuditableObjectRole.SecurityResource,
                Type = AuditableObjectType.SystemObject
            }).ToList();
            SendAudit(audit);
        }

        /// <summary>
        /// Autility utility which can be used to send a data audit 
        /// </summary>
        public static void AuditDataAction<TData>(EventTypeCodes typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, String queryPerformed, String remoteAddress, params TData[] data) 
        {

            traceSource.TraceVerbose("Create AuditDataAction audit");

            AuditCode eventTypeId = CreateAuditActionCode(typeCode);
            AuditData audit = new AuditData(DateTime.Now, action, outcome, eventType, eventTypeId);

            AddLocalDeviceActor(audit);
            AddUserActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);

            // Objects
            audit.AuditableObjects = data.Select(o =>
            {

                var idTypeCode = AuditableObjectIdType.Custom;
                var roleCode = AuditableObjectRole.Resource;
                var objType = AuditableObjectType.Other;

                if (o is Patient)
                {
                    idTypeCode = AuditableObjectIdType.PatientNumber;
                    roleCode = AuditableObjectRole.Patient;
                    objType = AuditableObjectType.Person;
                }
                else if (o is UserEntity || o is Provider)
                {
                    idTypeCode = AuditableObjectIdType.UserIdentifier;
                    objType = AuditableObjectType.Person;
                    roleCode = AuditableObjectRole.Provider;
                }
                else if (o is Entity)
                    idTypeCode = AuditableObjectIdType.EnrolleeNumber;
                else if (o is Act)
                {
                    idTypeCode = AuditableObjectIdType.EncounterNumber;
                    roleCode = AuditableObjectRole.Report;
                    if ((o as Act)?.ReasonConceptKey == NullReasonKeys.Masked) // Masked 
                        lifecycle = AuditableObjectLifecycle.Deidentification;
                }
                else if (o is SecurityUser)
                {
                    idTypeCode = AuditableObjectIdType.UserIdentifier;
                    roleCode = AuditableObjectRole.SecurityUser;
                    objType = AuditableObjectType.SystemObject;
                }
                else if(o is AuditData)
                {
                    idTypeCode = AuditableObjectIdType.ReportNumber;
                    roleCode = AuditableObjectRole.SecurityResource;
                    objType = AuditableObjectType.SystemObject;
                }

                return new AuditableObject()
                {
                    IDTypeCode = idTypeCode,
                    CustomIdTypeCode = idTypeCode == AuditableObjectIdType.Custom ? new AuditCode(o.GetType().Name, o.GetType().Namespace) : null,
                    LifecycleType = lifecycle,
                    ObjectId = (o as IIdentifiedEntity)?.Key?.ToString() ?? (o as AuditData)?.Key.ToString() ?? (o.GetType().GetRuntimeProperty("Id")?.GetValue(o)?.ToString()),
                    Role = roleCode,
                    Type = objType
                };
            }).ToList();

            // Query performed
            if (!String.IsNullOrEmpty(queryPerformed))
            {
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.SearchCritereon,
                    LifecycleType = AuditableObjectLifecycle.Access,
                    QueryData = queryPerformed,
                    Role = AuditableObjectRole.Query,
                    Type = AuditableObjectType.SystemObject
                });
            }

            SendAudit(audit);
        }

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        public static void AuditSecurityDeletionAction(IEnumerable<Object> objects, bool success, IEnumerable<string> changedProperties, String remoteAddress)
        {
            traceSource.TraceVerbose("Create SecurityDeletionAction audit");

            var audit = new AuditData(DateTime.Now, ActionType.Delete, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityObjectChanged));
            AddLocalDeviceActor(audit);
            AddUserActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);

            audit.AuditableObjects = objects.Select(obj => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Custom,
                CustomIdTypeCode = new AuditCode(obj.GetType().Name, "SanteDBTable"),
                ObjectId = ((obj as IIdentifiedEntity)?.Key ?? Guid.Empty).ToString(),
                LifecycleType = AuditableObjectLifecycle.LogicalDeletion,
                Role = AuditableObjectRole.SecurityResource,
                Type = AuditableObjectType.SystemObject
            }).ToList();
            SendAudit(audit);
        }

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        public static void AuditSecurityAttributeAction(IEnumerable<Object> objects, bool success, IEnumerable<string> changedProperties, String remoteAddress)
        {
            traceSource.TraceVerbose("Create SecurityAttributeAction audit");

            var audit = new AuditData(DateTime.Now, ActionType.Update, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityAttributesChanged));
            AddLocalDeviceActor(audit);
            AddUserActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);

            audit.AuditableObjects = objects.Select(obj => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Custom,
                CustomIdTypeCode = new AuditCode(obj.GetType().Name, "SanteDBTable"),
                ObjectId = ((obj as IIdentifiedEntity)?.Key ?? Guid.Empty).ToString(),
                LifecycleType = AuditableObjectLifecycle.Amendment,
                ObjectData = changedProperties.Where(o=>!String.IsNullOrEmpty(o)).Select(
                    kv => new ObjectDataExtension(
                        kv.Contains("=") ? kv.Substring(0, kv.IndexOf("=")) : kv,
                        kv.Contains("=") ? Encoding.UTF8.GetBytes(kv.Substring(kv.IndexOf("=") + 1)) : new byte[0]
                    )
                ).ToList(),
                Role = AuditableObjectRole.SecurityResource,
                Type = AuditableObjectType.SystemObject
            }).ToList();
            SendAudit(audit);
        }


        /// <summary>
        /// Send specified audit
        /// </summary>
        public static void SendAudit(AuditData audit)
        {
            traceSource.TraceInfo("Dispatching Audit - {0}", audit.Key);
            
            // If the current principal is SYSTEM then we don't need to send an audit
            ApplicationServiceContext.Current.GetService<IThreadPoolService>().QueueUserWorkItem(o =>
            {
                AuthenticationContext.Current = new AuthenticationContext(AuthenticationContext.SystemPrincipal);
                // Translate codes to DICOM
                if (audit.EventTypeCode != null)
                {
                    IConceptRepositoryService icpcr = ApplicationServiceContext.Current.GetService<IConceptRepositoryService>();
                    var concept = icpcr.GetConcept(audit.EventTypeCode.Code);
                    if (concept != null)
                    {
                        var refTerm = icpcr.GetConceptReferenceTerm(concept.Key.Value, "DCM");
                        if (refTerm != null)
                            audit.EventTypeCode = new AuditCode(refTerm.Mnemonic, "DCM") { DisplayName = refTerm.LoadCollection<ReferenceTermName>("DisplayNames")?.FirstOrDefault()?.Name };
                        else
                            audit.EventTypeCode.DisplayName = concept.LoadCollection<ConceptName>("ConceptNames").FirstOrDefault()?.Name;
                    }
                    traceSource.TraceVerbose("Mapped Audit Type Code - {0}-{1}-{2}", audit.EventTypeCode.CodeSystem, audit.EventTypeCode.Code, audit.EventTypeCode.DisplayName);

                }

                ApplicationServiceContext.Current.GetService<IAuditDispatchService>()?.SendAudit(audit);
                ApplicationServiceContext.Current.GetService<IAuditRepositoryService>()?.Insert(audit); // insert into local AR 
            });

        }

        /// <summary>
        /// Add user actor
        /// </summary>
        internal static void AddUserActor(AuditData audit)
        {
            var configService = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>();

            // For the user
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = ApplicationServiceContext.Current.GetService<INetworkInformationService>().GetHostName(),
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                UserName = AuthenticationContext.Current.Principal.Identity.Name,
                UserIsRequestor = true
            });
        }

        /// <summary>
        /// Add device actor
        /// </summary>
        public static void AddLocalDeviceActor(AuditData audit)
        {

            // For the current device name
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = ApplicationServiceContext.Current.GetService<INetworkInformationService>().GetHostName(),
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                UserName = ApplicationServiceContext.Current.GetService<INetworkInformationService>().GetMachineName(),
                ActorRoleCode = new List<AuditCode>() {
                    ApplicationServiceContext.Current.HostType == SanteDBHostType.Server ?
                    new  AuditCode("110152", "DCM") { DisplayName = "Destination" } :
                    new  AuditCode("110153", "DCM") { DisplayName = "Source" }
                }
            });

        }

        /// <summary>
        /// Add device actor
        /// </summary>
        internal static void AddRemoteDeviceActor(AuditData audit, String remoteAddress)
        {
            if (remoteAddress == null) return;

            // For the current device name
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = remoteAddress,
                NetworkAccessPointType = NetworkAccessPointType.IPAddress,
                ActorRoleCode = new List<AuditCode>() {
                    ApplicationServiceContext.Current.HostType == SanteDBHostType.Server ?
                    new  AuditCode("110153", "DCM") { DisplayName = "Source" } :
                    new  AuditCode("110152", "DCM") { DisplayName = "Destination" } 
                }
            });

        }

        /// <summary>
        /// Audit an override operation
        /// </summary>
        public static void AuditOverride(IPrincipal principal, string purposeOfUse, string[] policies, bool success, String remoteAddress)
        {

            traceSource.TraceVerbose("Create Override audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.EmergencyOverrideStarted, new AuditCode(purposeOfUse, SanteDBClaimTypes.XspaPurposeOfUseClaim));
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                NetworkAccessPointId = ApplicationServiceContext.Current.GetService<INetworkInformationService>().GetHostName(),
                UserName = principal?.Identity?.Name,
                UserIsRequestor = true,
                ActorRoleCode = ApplicationServiceContext.Current.GetService<IRoleProviderService>()?.GetAllRoles(principal.Identity.Name).Select(o =>
                    new AuditCode(o, null)
                ).ToList()
            });
            AddLocalDeviceActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);

            audit.AuditableObjects.AddRange(policies.Select(o => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                ObjectId = $"urn:oid:{o}",
                Type = AuditableObjectType.SystemObject,
                Role = AuditableObjectRole.SecurityGranularityDefinition
            }));

            SendAudit(audit);
        }

        /// <summary>
        /// Create audit action code
        /// </summary>
        public static AuditCode CreateAuditActionCode(EventTypeCodes typeCode)
        {
            var typeCodeWire = typeof(EventTypeCodes).GetRuntimeField(typeCode.ToString()).GetCustomAttribute<XmlEnumAttribute>();
            return new AuditCode(typeCodeWire.Name, "SecurityAuditCode");
        }

        /// <summary>
        /// Audit application start or stop
        /// </summary>
        public static void AuditApplicationStartStop(EventTypeCodes eventType)
        {
            traceSource.TraceVerbose("Create ApplicationStart audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.ApplicationActivity, CreateAuditActionCode(eventType));
            AddLocalDeviceActor(audit);
            SendAudit(audit);
        }


        /// <summary>
        /// Audit a login of a principal
        /// </summary>
        public static void AuditLogin(IPrincipal principal, String identityName, IIdentityProviderService identityProvider, String remoteAddress, bool successfulLogin = true)
        {

            traceSource.TraceVerbose("Create Login audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, successfulLogin ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.Login));
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                NetworkAccessPointId = ApplicationServiceContext.Current.GetService<INetworkInformationService>().GetHostName(),
                UserName = principal?.Identity?.Name ?? identityName,
                UserIsRequestor = true,
                ActorRoleCode = ApplicationServiceContext.Current.GetService<IRoleProviderService>()?.GetAllRoles(principal.Identity.Name).Select(o =>
                    new AuditCode(o, null)
                ).ToList()
            });
            AddLocalDeviceActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);

            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                NameData = identityProvider.GetType().AssemblyQualifiedName,
                ObjectId = $"http://santedb.org/auth/{identityProvider.GetType().FullName.Replace(".", "/")}",
                Type = AuditableObjectType.SystemObject,
                Role = AuditableObjectRole.Job
            });

            SendAudit(audit);
        }

        /// <summary>
        /// Audit a login of a principal
        /// </summary>
        public static void AuditLogout(IPrincipal principal, String remoteAddress)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            traceSource.TraceVerbose("Create Logout audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.Logout));
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = ApplicationServiceContext.Current.GetService<INetworkInformationService>().GetHostName(),
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                UserName = principal.Identity.Name,
                UserIsRequestor = true
            });
            AddLocalDeviceActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);

            SendAudit(audit);
        }

        /// <summary>
        /// Audit the use of a restricted function
        /// </summary>
        public static void AuditRestrictedFunction(Exception ex, Uri url, String remoteAddress, params string[] mitigations)
        {
            traceSource.TraceVerbose("Create RestrictedFunction audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, mitigations.Length > 0 ? OutcomeIndicator.MinorFail : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityAlert));
            AddUserActor(audit);
            AddLocalDeviceActor(audit);
            AddRemoteDeviceActor(audit, remoteAddress);
            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                LifecycleType = AuditableObjectLifecycle.Access,
                ObjectId = url.ToString(),
                Role = AuditableObjectRole.Resource,
                Type = AuditableObjectType.SystemObject
            });

            if(ex is PolicyViolationException)
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/policy/{(ex as PolicyViolationException).PolicyId}",
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    ObjectData = new List<ObjectDataExtension>(mitigations.Select(o=>new ObjectDataExtension("mitigation", Encoding.UTF8.GetBytes(o))))
                    {
                        new ObjectDataExtension("decision", new byte[] { (byte)(ex as PolicyViolationException).PolicyDecision }),
                        new ObjectDataExtension("policyId", Encoding.UTF8.GetBytes((ex as PolicyViolationException).PolicyId))
                    }
                });
            else
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/error/{ex.GetType().Name}",
                    Role= AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    ObjectData = new List<ObjectDataExtension>(mitigations.Select(o => new ObjectDataExtension("mitigation", Encoding.UTF8.GetBytes(o))))
                    {
                        new ObjectDataExtension("exception", Encoding.UTF8.GetBytes(ex.ToString()))
                    }
                });
            
            SendAudit(audit);
        }
    }
}
