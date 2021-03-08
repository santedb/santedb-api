/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Security;
using SanteDB.Core.Auditing;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Interfaces;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

    /// <summary>
    /// Security utility
    /// </summary>
    public static class AuditUtil
    {

        private static Tracer traceSource = Tracer.GetTracer(typeof(AuditUtil));

        private static AuditAccountabilityConfigurationSection s_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<AuditAccountabilityConfigurationSection>();

        /// <summary>
        /// Audit that the audit log was used
        /// </summary>
        /// <param name="action">The action that occurred</param>
        /// <param name="outcome">The outcome of the action</param>
        /// <param name="query">The query which was being executed</param>
        /// <param name="auditIds">The identifiers of any objects disclosed</param>
        /// <param name="remoteAddress">The remote address</param>
        public static void AuditAuditLogUsed(ActionType action, OutcomeIndicator outcome, String query, params Guid[] auditIds)
        {
            traceSource.TraceInfo("Create AuditLogUsed audit");
            AuditData audit = new AuditData(DateTime.Now, action, outcome, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.AuditLogUsed));

            // User actors
            AddLocalDeviceActor(audit);
            AddUserActor(audit);
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
        /// Audit that a synchronization occurred
        /// </summary>
        public static void AuditSynchronization(AuditableObjectLifecycle lifecycle, String remoteTarget, OutcomeIndicator outcome, params IdentifiedData[] objects)
        {
            AuditCode eventTypeId = new AuditCode("Synchronization", "SecurityAuditCode");
            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, outcome, lifecycle == AuditableObjectLifecycle.Import ? EventIdentifierType.Import : EventIdentifierType.Export , eventTypeId);

            AddLocalDeviceActor(audit);
            if (lifecycle == AuditableObjectLifecycle.Export) // me to remote 
            {
                // I am the source
                audit.Actors.First().ActorRoleCode = new List<AuditCode>() { new AuditCode("110153", "DCM") { DisplayName = "Source" } };
                // Remote is the destination
                audit.Actors.Add(new AuditActorData()
                {
                    ActorRoleCode = new List<AuditCode>() { new AuditCode("110152", "DCM") { DisplayName = "Destination" } },
                    NetworkAccessPointType = NetworkAccessPointType.MachineName,
                    NetworkAccessPointId = remoteTarget
                });
            }
            else
            {
                // Remote is the destination
                audit.Actors.Add(new AuditActorData()
                {
                    ActorRoleCode = new List<AuditCode>() { new AuditCode("110153", "DCM") { DisplayName = "Source" } },
                    NetworkAccessPointType = NetworkAccessPointType.MachineName,
                    NetworkAccessPointId = remoteTarget
                });
            }

            if (objects.All(o => o is Bundle))
                objects = objects.OfType<Bundle>().SelectMany(o => o.Item).ToArray();
            audit.AuditableObjects = objects.OfType<IdentifiedData>().Select(o => CreateAuditableObject(o, lifecycle)).ToList();

            SendAudit(audit);
        }

        /// <summary>
        /// Audit an access control decision
        /// </summary>
        public static void AuditAccessControlDecision(IPrincipal principal, string policy, PolicyGrantType action)
        {
            traceSource.TraceInfo($"ACS: {principal} - {policy} - {action}");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, action == PolicyGrantType.Grant ? OutcomeIndicator.Success : action == PolicyGrantType.Elevate ? OutcomeIndicator.MinorFail : OutcomeIndicator.SeriousFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.AccessControlDecision));

            // User actors
            AddLocalDeviceActor(audit);
            AddUserActor(audit);
            // Audit policy
            audit.AuditableObjects = new List<AuditableObject>() {
                    new AuditableObject()
                    {
                        IDTypeCode = AuditableObjectIdType.Custom,
                        CustomIdTypeCode = new AuditCode("$Action", "SanteDBAction"),
                        ObjectId = Guid.Empty.ToString(),
                        Role = AuditableObjectRole.SecurityGranularityDefinition,
                        Type = AuditableObjectType.SystemObject,
                        ObjectData = new List<ObjectDataExtension>() { new ObjectDataExtension(policy, action.ToString()) }
                    }
                };
            AuditUtil.SendAudit(audit);
        }

        /// <summary>
        /// Audit masking of a particular object
        /// </summary>
        /// <param name="targetOfMasking">The object which was masked</param>
        /// <param name="wasRemoved">True if the object was removed instead of masked</param>
        public static void AuditMasking<TModel>(TModel targetOfMasking, PolicyDecision decision, bool wasRemoved)
            where TModel : IdentifiedData
        {
            AuditUtil.AuditDataAction(new AuditCode("SecurityAuditCode-Masking", "SecurityAuditCode") { DisplayName = "Mask Sensitive Data" }, ActionType.Execute, AuditableObjectLifecycle.Deidentification, EventIdentifierType.ApplicationActivity, OutcomeIndicator.Success, null, decision, targetOfMasking);

            // TODO: Implement this
        }

        /// <summary>
        /// Audit the creation of an object
        /// </summary>
        public static void AuditCreate<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
        {
            AuditUtil.AuditDataAction(new AuditCode("SecurityAuditCode-CreateInstances", "SecurityAuditDataEvent") { DisplayName = "Create New Record" }, ActionType.Create, AuditableObjectLifecycle.Creation, EventIdentifierType.Import, outcome, queryPerformed, null, resourceData);
        }

        /// <summary>
        /// Audit the update of an object
        /// </summary>
        public static void AuditUpdate<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
        {
            AuditUtil.AuditDataAction(new AuditCode("SecurityAuditCode-UpdateInstances", "SecurityAuditDataEvent") { DisplayName = "Update Existing Record" }, ActionType.Update, AuditableObjectLifecycle.Amendment, EventIdentifierType.Import, outcome, queryPerformed, null, resourceData);
        }

        /// <summary>
        /// Audit a deletion
        /// </summary>
        public static void AuditDelete<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
        {
            AuditUtil.AuditDataAction(new AuditCode("SecurityAuditCode-DeleteInstances", "SecurityAuditDataEvent") { DisplayName = "Delete Existing Record" }, ActionType.Delete, AuditableObjectLifecycle.LogicalDeletion, EventIdentifierType.Import, outcome, queryPerformed, null, resourceData);
        }

        /// <summary>
        /// Audit the update of an object
        /// </summary>
        public static void AuditQuery<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] results)
        {
            AuditUtil.AuditDataAction(CreateAuditActionCode(EventTypeCodes.Query), ActionType.Execute, AuditableObjectLifecycle.Disclosure, EventIdentifierType.Query, outcome, queryPerformed, null, results);
        }

        /// <summary>
        /// Audit the update of an object
        /// </summary>
        public static void AuditRead<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] results)
        {
            AuditUtil.AuditDataAction(CreateAuditActionCode(EventTypeCodes.Query), ActionType.Read, AuditableObjectLifecycle.Disclosure, EventIdentifierType.Query, outcome, queryPerformed, null, results);
        }

        /// <summary>
        /// Audit that security objects were created
        /// </summary>
        public static void AuditSecurityCreationAction(IEnumerable<object> objects, bool success, IEnumerable<string> changedProperties)
        {
            traceSource.TraceInfo("Create SecurityCreationAction audit");

            var audit = new AuditData(DateTime.Now, ActionType.Create, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityObjectChanged));
            AddLocalDeviceActor(audit);
            AddUserActor(audit);

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
        /// Audit data action
        /// </summary>
        public static void AuditDataAction<TData>(EventTypeCodes typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, String queryPerformed, params TData[] data)
        {
            AuditDataAction<TData>(CreateAuditActionCode(typeCode), action, lifecycle, eventType, outcome, queryPerformed, null, data);
        }

        /// <summary>
        /// Autility utility which can be used to send a data audit 
        /// </summary>
        public static void AuditDataAction<TData>(AuditCode typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, String queryPerformed, PolicyDecision grantInfo, params TData[] data)
        {

            traceSource.TraceInfo("Create AuditDataAction audit");

            AuditData audit = new AuditData(DateTime.Now, action, outcome, eventType, typeCode);

            AddLocalDeviceActor(audit);
            AddUserActor(audit);

            // Objects
            if(typeof(Person).IsAssignableFrom(typeof(TData)) ||
                typeof(Act).IsAssignableFrom(typeof(TData)))
                audit.AuditableObjects = data.OfType<TData>().SelectMany(o =>
                {
                    if (o is Bundle bundle)
                        return bundle.Item.Select(i => CreateAuditableObject(i, lifecycle));
                    else return new AuditableObject[] { CreateAuditableObject(o, lifecycle) };
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
            if (grantInfo != null)
                audit.AuditableObjects.Add(CreateAuditableObject(grantInfo, AuditableObjectLifecycle.Verification));
            SendAudit(audit);
        }

        /// <summary>
        /// Create an auditable object from the specified object
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="obj">The object to translate</param>
        private static AuditableObject CreateAuditableObject<TData>(TData obj, AuditableObjectLifecycle lifecycle)
        {

            var idTypeCode = AuditableObjectIdType.Custom;
            var roleCode = AuditableObjectRole.Resource;
            var objType = AuditableObjectType.Other;

            if (obj is Patient)
            {
                idTypeCode = AuditableObjectIdType.PatientNumber;
                roleCode = AuditableObjectRole.Patient;
                objType = AuditableObjectType.Person;
            }
            else if (obj is UserEntity || obj is Provider)
            {
                idTypeCode = AuditableObjectIdType.UserIdentifier;
                objType = AuditableObjectType.Person;
                roleCode = AuditableObjectRole.Provider;
            }
            else if (obj is Entity)
                idTypeCode = AuditableObjectIdType.EnrolleeNumber;
            else if (obj is Act)
            {
                idTypeCode = AuditableObjectIdType.EncounterNumber;
                roleCode = AuditableObjectRole.Report;
                if ((obj as Act)?.ReasonConceptKey == NullReasonKeys.Masked) // Masked 
                    lifecycle = AuditableObjectLifecycle.Deidentification;
            }
            else if (obj is SecurityUser)
            {
                idTypeCode = AuditableObjectIdType.UserIdentifier;
                roleCode = AuditableObjectRole.SecurityUser;
                objType = AuditableObjectType.SystemObject;
            }
            else if (obj is AuditData)
            {
                idTypeCode = AuditableObjectIdType.ReportNumber;
                roleCode = AuditableObjectRole.SecurityResource;
                objType = AuditableObjectType.SystemObject;
            }
            else if (obj is Guid)
            {
                idTypeCode = AuditableObjectIdType.Uri;
                roleCode = AuditableObjectRole.MasterFile;
                objType = AuditableObjectType.SystemObject;
            }
            else if(obj is PolicyDecision)
            {
                idTypeCode = AuditableObjectIdType.Uri;
                roleCode = AuditableObjectRole.SecurityGranularityDefinition;
                objType = AuditableObjectType.SystemObject;
            }

            var retVal= new AuditableObject()
            {
                IDTypeCode = idTypeCode,
                CustomIdTypeCode = idTypeCode == AuditableObjectIdType.Custom ? new AuditCode(obj.GetType().Name, obj.GetType().Namespace) : null,
                LifecycleType = lifecycle,
                ObjectId = (obj as IIdentifiedEntity)?.Key?.ToString() ?? (obj as AuditData)?.Key?.ToString() ?? (obj.GetType().GetRuntimeProperty("Id")?.GetValue(obj)?.ToString()) ?? obj.ToString(),
                Role = roleCode,
                Type = objType
            };

            if(obj is PolicyDecision pd)
            {
                retVal.ObjectData = pd.Details.Select(o => new ObjectDataExtension(o.PolicyId, new byte[] { (byte)o.Outcome })).ToList();
            }
            return retVal;
        }

        /// <summary>
        /// Audit that sensitve data was disclosed
        /// </summary>
        /// <param name="result"></param>
        public static void AuditSensitiveDisclosure(IdentifiedData result, PolicyDecision decision, bool disclosed)
        {
            traceSource.TraceInfo("Create AuditDataAction audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Read, disclosed ? OutcomeIndicator.Success : OutcomeIndicator.MinorFail, EventIdentifierType.SecurityAlert, new AuditCode("SecurityAuditEvent-DisclosureOfSensitiveInformation", "SecurityAuditDataEvent") {
                DisplayName = "Sensitive Data Was Disclosed to User"
            });

            AddLocalDeviceActor(audit);
            AddUserActor(audit);
            audit.AuditableObjects = new List<AuditableObject>()
            {
                CreateAuditableObject(result, AuditableObjectLifecycle.Disclosure),
                CreateAuditableObject(decision, AuditableObjectLifecycle.Verification)
            };
            

            SendAudit(audit);
        }

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        public static void AuditSecurityDeletionAction(IEnumerable<Object> objects, bool success, IEnumerable<string> changedProperties)
        {
            traceSource.TraceInfo("Create SecurityDeletionAction audit");

            var audit = new AuditData(DateTime.Now, ActionType.Delete, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityObjectChanged));
            AddLocalDeviceActor(audit);
            AddUserActor(audit);

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
        public static void AuditSecurityAttributeAction(IEnumerable<Object> objects, bool success, IEnumerable<string> changedProperties)
        {
            traceSource.TraceInfo("Create SecurityAttributeAction audit");

            var audit = new AuditData(DateTime.Now, ActionType.Update, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityAttributesChanged));
            AddLocalDeviceActor(audit);
            AddUserActor(audit);

            audit.AuditableObjects = objects.Select(obj => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Custom,
                CustomIdTypeCode = new AuditCode(obj.GetType().Name, "SanteDBTable"),
                ObjectId = ((obj as IIdentifiedEntity)?.Key ?? Guid.Empty).ToString(),
                LifecycleType = AuditableObjectLifecycle.Amendment,
                ObjectData = changedProperties.Where(o => !String.IsNullOrEmpty(o)).Select(
                    kv => new ObjectDataExtension(
                        kv.Contains("=") ? kv.Substring(0, kv.IndexOf("=")) : kv,
                        kv.Contains("=") ? Encoding.UTF8.GetBytes(kv.Substring(kv.IndexOf("=") + 1)) : new byte[0]
                    )
                ).ToList(),
                NameData = (obj as IdentifiedData)?.ToDisplay(),
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

            // If the current principal is SYSTEM then we don't need to send an audit
            Action<object> workitem = (o) =>
            {
                try
                {
                    dynamic metadata = o;
                    var rc = metadata.rc as RemoteEndpointInfo;
                    var principal = metadata.principal as IClaimsPrincipal;
                    traceSource.TraceInfo("Dispatching audit {0} - {1}", audit.ActionCode, audit.EventIdentifier);

                    // Get audit metadata
                    audit.AddMetadata(AuditMetadataKey.PID, Process.GetCurrentProcess().Id.ToString());
                    audit.AddMetadata(AuditMetadataKey.ProcessName, Process.GetCurrentProcess().ProcessName);
                    audit.AddMetadata(AuditMetadataKey.SessionId, principal?.FindFirst(SanteDBClaimTypes.SanteDBSessionIdClaim)?.Value);
                    audit.AddMetadata(AuditMetadataKey.CorrelationToken, rc?.CorrelationToken);
                    audit.AddMetadata(AuditMetadataKey.AuditSourceType, "4");
                    audit.AddMetadata(AuditMetadataKey.LocalEndpoint, rc?.OriginalRequestUrl);
                    audit.AddMetadata(AuditMetadataKey.RemoteHost, rc?.RemoteAddress);

                    // Filter apply?
                    var filters = s_configuration?.AuditFilters.Where(f =>
                    (!f.OutcomeSpecified ^ f.Outcome.HasFlag(audit.Outcome)) &&
                        (!f.ActionSpecified ^ f.Action.HasFlag(audit.ActionCode)) &&
                        (!f.EventSpecified ^ f.Event.HasFlag(audit.EventIdentifier)));

                    if (AuthenticationContext.Current.Principal == AuthenticationContext.AnonymousPrincipal)
                        AuthenticationContext.Current = new AuthenticationContext(AuthenticationContext.SystemPrincipal);
                    if (filters == null || filters.Count() == 0 || filters.Any(f => f.InsertLocal))
                        ApplicationServiceContext.Current.GetService<IRepositoryService<AuditData>>()?.Insert(audit); // insert into local AR 
                    if (filters == null || filters.Count() == 0 || filters.Any(f => f.SendRemote))
                        ApplicationServiceContext.Current.GetService<IAuditDispatchService>()?.SendAudit(audit);
                }
                catch (Exception e)
                {
                    traceSource.TraceError("Error dispatching / saving audit: {0}", e);
                }
            };

            // Action
            var parm = new { rc = RemoteEndpointUtil.Current.GetRemoteClient(), principal = AuthenticationContext.Current.Principal };
            if (ApplicationServiceContext.Current.IsRunning)
                ApplicationServiceContext.Current.GetService<IThreadPoolService>()?.QueueUserWorkItem(workitem, parm); // background
            else
                workitem(parm); // service is stopped

        }

        /// <summary>
        /// Add user actor
        /// </summary>
        public static void AddUserActor(AuditData audit, IPrincipal principal = null)
        {
            var configService = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>();

            // Use all remote endpoint providers to find the current request 
            principal = principal ?? AuthenticationContext.Current.Principal;
            // For the user
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = RemoteEndpointUtil.Current.GetRemoteClient()?.RemoteAddress,
                NetworkAccessPointType = NetworkAccessPointType.IPAddress,
                UserName = principal.Identity.Name,
                ActorRoleCode = new List<AuditCode>() {
                    new  AuditCode("110153", "DCM") { DisplayName = "Source" }
                },
                UserIsRequestor = true
            });
        }

        /// <summary>
        /// Add device actor
        /// </summary>
        public static void AddLocalDeviceActor(AuditData audit)
        {

            traceSource.TraceInfo("Adding local device actor to audit {0}", audit.EventIdentifier);
            // For the current device name
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = ApplicationServiceContext.Current.GetService<INetworkInformationService>()?.GetHostName(),
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                UserName = ApplicationServiceContext.Current.GetService<INetworkInformationService>()?.GetMachineName(),
                ActorRoleCode = new List<AuditCode>() {
                    new  AuditCode("110152", "DCM") { DisplayName = "Destination" }
                }
            });

        }

        /// <summary>
        /// Audit an override operation
        /// </summary>
        public static void AuditOverride(ISession session, IPrincipal principal, string purposeOfUse, string[] policies, bool success)
        {

            traceSource.TraceInfo("Create Override audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.EmergencyOverrideStarted, new AuditCode(purposeOfUse, SanteDBClaimTypes.PurposeOfUse));
            AddUserActor(audit, principal);
            AddLocalDeviceActor(audit);

            // Add policies which were overridden
            audit.AuditableObjects.AddRange(policies.Select(o => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                ObjectId = $"urn:oid:{o}",
                Type = AuditableObjectType.SystemObject,
                Role = AuditableObjectRole.SecurityGranularityDefinition
            }));

            if(session != null)
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    ObjectId = BitConverter.ToString(session.Id).Replace("-", ""),
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode("Session", "SanteDBTable"),
                    LifecycleType = AuditableObjectLifecycle.Creation
                });
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
            traceSource.TraceInfo("Create ApplicationStart audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.ApplicationActivity, CreateAuditActionCode(eventType));
            AddLocalDeviceActor(audit);
            SendAudit(audit);
        }


        /// <summary>
        /// Audit a login of a principal
        /// </summary>
        public static void AuditLogin(IPrincipal principal, String identityName, IIdentityProviderService identityProvider, bool successfulLogin = true)
        {

            traceSource.TraceInfo("Create Login audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, successfulLogin ? OutcomeIndicator.Success : OutcomeIndicator.SeriousFail, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.Login));
            AddLocalDeviceActor(audit);
            AddUserActor(audit , principal);
            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.UserIdentifier,
                ObjectId = identityName,
                LifecycleType = AuditableObjectLifecycle.NotSet,
                Role = AuditableObjectRole.SecurityUser,
                Type = AuditableObjectType.SystemObject
            });
            SendAudit(audit);
        }

        /// <summary>
        /// Audit a login of a principal
        /// </summary>
        public static void AuditLogout(IPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            traceSource.TraceInfo("Create Logout audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.Logout));
            AddLocalDeviceActor(audit);
            AddUserActor(audit, principal);

            SendAudit(audit);
        }

        /// <summary>
        /// Audit the use of a restricted function
        /// </summary>
        public static void AuditNetworkRequestFailure(Exception ex, Uri url, NameValueCollection requestHeaders, NameValueCollection responseHeaders)
        {
            AuditNetworkRequestFailure(ex, url, requestHeaders.AllKeys.ToDictionary(o => o, o => requestHeaders[o]), responseHeaders.AllKeys.ToDictionary(o => o, o => responseHeaders[o]));
        }

        /// <summary>
        /// Audit a network request failure
        /// </summary>
        public static void AuditNetworkRequestFailure(Exception ex, Uri url, IDictionary<String, String> requestHeaders, IDictionary<String, String> responseHeaders)
        { 
            traceSource.TraceInfo("Create Network Request Failure audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.MinorFail, EventIdentifierType.NetworkActivity, CreateAuditActionCode(EventTypeCodes.NetworkActivity));
            AddUserActor(audit);
            AddLocalDeviceActor(audit);
            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                LifecycleType = AuditableObjectLifecycle.Access,
                ObjectId = "urlQuery",
                QueryData = url.ToString(),
                Role = AuditableObjectRole.Resource,
                Type = AuditableObjectType.SystemObject
            });

            if (ex is PolicyViolationException)
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/policy/{(ex as PolicyViolationException).PolicyId}",
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    NameData = ex.ToString()
                });
            else
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/error/{ex.GetType().Name}",
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    NameData = ex.ToString()
                });


            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                Type = AuditableObjectType.Other,
                Role = AuditableObjectRole.RoutingCriteria,
                ObjectId = "HttpRequest",
                ObjectData = requestHeaders.Select(o => new ObjectDataExtension(o.Key, Encoding.UTF8.GetBytes(o.Value))).ToList()
            });
            audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                Type = AuditableObjectType.Other,
                Role = AuditableObjectRole.Report,
                ObjectId = "HttpResponse",
                ObjectData = responseHeaders.Select(o => new ObjectDataExtension(o.Key, Encoding.UTF8.GetBytes(o.Value))).ToList()
            });
            SendAudit(audit);
        }

        /// <summary>
        /// Audit that a session has begun
        /// </summary>
        public static void AuditSessionStart(ISession session, IPrincipal principal, bool success)
        {
            traceSource.TraceInfo("Create session audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.SessionStarted));
            AddLocalDeviceActor(audit);
            AddUserActor(audit, principal);

            var policies = session?.Claims.Where(o => o.Type == SanteDBClaimTypes.SanteDBScopeClaim).Select(o => o.Value);

            // Audit the actual session that is created
            var cprincipal = principal as IClaimsPrincipal;
            var deviceIdentity = cprincipal?.Identities.OfType<IDeviceIdentity>().FirstOrDefault();
            var applicationIdentity = cprincipal?.Identities.OfType<IApplicationIdentity>().FirstOrDefault();

            if (session != null)
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    ObjectId = BitConverter.ToString(session.Id).Replace("-", ""),
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode("Session", "SanteDBTable"),
                    LifecycleType = AuditableObjectLifecycle.Creation,
                    ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("method", principal.Identity?.AuthenticationType),
                        deviceIdentity != cprincipal.Identity && applicationIdentity != cprincipal.Identity ? new ObjectDataExtension("userIdentity", principal.Identity.Name) : null,
                        deviceIdentity != null ? new ObjectDataExtension("deviceIdentity", deviceIdentity?.Name) : null,
                        applicationIdentity != null ? new ObjectDataExtension("applicationIdentity", applicationIdentity?.Name) : null,
                        new ObjectDataExtension("scope", String.Join(" ", policies ?? new String[] { "*" }))
                    }.OfType<ObjectDataExtension>().ToList()
                });
            else
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    LifecycleType = AuditableObjectLifecycle.Creation,
                    ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("method", principal.Identity?.AuthenticationType),
                        deviceIdentity != cprincipal.Identity && applicationIdentity != cprincipal.Identity ? new ObjectDataExtension("userIdentity", principal.Identity.Name) : null,
                        deviceIdentity != null ? new ObjectDataExtension("deviceIdentity", deviceIdentity?.Name) : null,
                        applicationIdentity != null ? new ObjectDataExtension("applicationIdentity", applicationIdentity?.Name) : null
                    }.OfType<ObjectDataExtension>().ToList()
                });

            SendAudit(audit);
        }

        /// <summary>
        /// Audit that a session has begun
        /// </summary>
        public static void AuditSessionStop(ISession session, IPrincipal principal, bool success)
        {
            traceSource.TraceInfo("End session audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.SessionStopped));

            AddLocalDeviceActor(audit);
            AddUserActor(audit, principal);

            // Audit the actual session that is created
            var cprincipal = (principal ?? AuthenticationContext.Current.Principal) as IClaimsPrincipal;
            var deviceIdentity = cprincipal?.Identities.OfType<IDeviceIdentity>().FirstOrDefault();
            var applicationIdentity = cprincipal?.Identities.OfType<IApplicationIdentity>().FirstOrDefault();

            if (session != null)
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    ObjectId = BitConverter.ToString(session.Id).Replace("-", ""),
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode("Session", "SanteDBTable"),
                    LifecycleType = AuditableObjectLifecycle.Creation,
                    ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("method", principal.Identity?.AuthenticationType),
                        deviceIdentity != cprincipal.Identity && applicationIdentity != cprincipal.Identity ? new ObjectDataExtension("userIdentity", principal.Identity.Name) : null,
                        deviceIdentity != null ? new ObjectDataExtension("deviceIdentity", deviceIdentity?.Name) : null,
                        applicationIdentity != null ? new ObjectDataExtension("applicationIdentity", applicationIdentity?.Name) : null
                    }.OfType<ObjectDataExtension>().ToList()
                });
            else
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    LifecycleType = AuditableObjectLifecycle.Creation,
                    ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("method", principal.Identity?.AuthenticationType),
                        deviceIdentity != cprincipal.Identity && applicationIdentity != cprincipal.Identity ? new ObjectDataExtension("userIdentity", principal.Identity.Name) : null,
                        deviceIdentity != null ? new ObjectDataExtension("deviceIdentity", deviceIdentity?.Name) : null,
                        applicationIdentity != null ? new ObjectDataExtension("applicationIdentity", applicationIdentity?.Name) : null
                    }.OfType<ObjectDataExtension>().ToList()
                });

            SendAudit(audit);
        }

        /// <summary>
        /// Audit the export of data 
        /// </summary>
        public static void AuditDataExport(params object[] exportedData)
        {
            AuditCode eventTypeId = CreateAuditActionCode(EventTypeCodes.Export);
            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.SecurityAlert, eventTypeId);

            AddLocalDeviceActor(audit);
            AddUserActor(audit);

            audit.AuditableObjects = exportedData.Where(o=>o!=null).Select(o =>
            {
                var obj = CreateAuditableObject(o, AuditableObjectLifecycle.Export);
                return obj;
            }).ToList();

            SendAudit(audit);
        }

    }
}
