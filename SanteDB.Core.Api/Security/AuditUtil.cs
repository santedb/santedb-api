/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: Justin Fyfe
 * Date: 2019-8-8
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
using SanteDB.Core.Security.Principal;
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
            traceSource.TraceVerbose("Create AuditLogUsed audit");
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
        public static void AuditMasking<TModel>(TModel targetOfMasking, bool wasRemoved)
            where TModel : IdentifiedData
        {
            AuditUtil.AuditDataAction(EventTypeCodes.ApplicationActivity, ActionType.Execute, AuditableObjectLifecycle.Deidentification, EventIdentifierType.SecurityAlert, OutcomeIndicator.Success, null, targetOfMasking);

            // TODO: Implement this
        }

        /// <summary>
        /// Audit the creation of an object
        /// </summary>
        public static void AuditCreate<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
        {
            AuditUtil.AuditDataAction(EventTypeCodes.Import, ActionType.Create, AuditableObjectLifecycle.Creation, EventIdentifierType.Import, outcome, queryPerformed, resourceData);
        }

        /// <summary>
        /// Audit the update of an object
        /// </summary>
        public static void AuditUpdate<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
        {
            AuditUtil.AuditDataAction(EventTypeCodes.Import, ActionType.Update, AuditableObjectLifecycle.Amendment, EventIdentifierType.Import, outcome, queryPerformed, resourceData);
        }

        /// <summary>
        /// Audit a deletion
        /// </summary>
        public static void AuditDelete<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
        {
            AuditUtil.AuditDataAction(EventTypeCodes.Import, ActionType.Delete, AuditableObjectLifecycle.LogicalDeletion, EventIdentifierType.Import, outcome, queryPerformed, resourceData);
        }

        /// <summary>
        /// Audit the update of an object
        /// </summary>
        public static void AuditQuery<TData>(OutcomeIndicator outcome, string queryPerformed, params TData[] results) 
        {
            AuditUtil.AuditDataAction(EventTypeCodes.Query, ActionType.Read, AuditableObjectLifecycle.Disclosure, EventIdentifierType.Query, outcome, queryPerformed, results);
        }

        /// <summary>
        /// Audit that security objects were created
        /// </summary>
        public static void AuditSecurityCreationAction(IEnumerable<object> objects, bool success, IEnumerable<string> changedProperties)
        {
            traceSource.TraceVerbose("Create SecurityCreationAction audit");

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
        /// Autility utility which can be used to send a data audit 
        /// </summary>
        public static void AuditDataAction<TData>(EventTypeCodes typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, String queryPerformed, params TData[] data) 
        {

            traceSource.TraceVerbose("Create AuditDataAction audit");

            AuditCode eventTypeId = CreateAuditActionCode(typeCode);
            AuditData audit = new AuditData(DateTime.Now, action, outcome, eventType, eventTypeId);

            AddLocalDeviceActor(audit);
            AddUserActor(audit);

            // Objects
            audit.AuditableObjects = data.Select(o =>
            {
                var obj = CreateAuditableObject(o, lifecycle);
                return obj;
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
            else if(obj is Guid)
            {
                idTypeCode = AuditableObjectIdType.Uri;
                roleCode = AuditableObjectRole.MasterFile;
                objType = AuditableObjectType.SystemObject;
            }

            return new AuditableObject()
            {
                IDTypeCode = idTypeCode,
                CustomIdTypeCode = idTypeCode == AuditableObjectIdType.Custom ? new AuditCode(obj.GetType().Name, obj.GetType().Namespace) : null,
                LifecycleType = lifecycle,
                ObjectId = (obj as IIdentifiedEntity)?.Key?.ToString() ?? (obj as AuditData)?.Key.ToString() ?? (obj.GetType().GetRuntimeProperty("Id")?.GetValue(obj)?.ToString()) ?? obj.ToString(),
                Role = roleCode,
                Type = objType
            };

        }

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        public static void AuditSecurityDeletionAction(IEnumerable<Object> objects, bool success, IEnumerable<string> changedProperties)
        {
            traceSource.TraceVerbose("Create SecurityDeletionAction audit");

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
            traceSource.TraceVerbose("Create SecurityAttributeAction audit");

            var audit = new AuditData(DateTime.Now, ActionType.Update, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.SecurityAlert, CreateAuditActionCode(EventTypeCodes.SecurityAttributesChanged));
            AddLocalDeviceActor(audit);
            AddUserActor(audit);

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

            // Get audit metadata
            foreach (var itm in ApplicationServiceContext.Current.GetService<IAuditMetadataProvider>()?.GetMetadata())
                if (itm.Value != null && !audit.Metadata.Any(o => o.Key == itm.Key))
                    audit.AddMetadata(itm.Key, itm.Value?.ToString());

            // If the current principal is SYSTEM then we don't need to send an audit
            Action<object> workitem = (o) =>
            {
                AuthenticationContext.Current = new AuthenticationContext(AuthenticationContext.SystemPrincipal);
                ApplicationServiceContext.Current.GetService<IAuditRepositoryService>()?.Insert(audit); // insert into local AR 
                ApplicationServiceContext.Current.GetService<IAuditDispatchService>()?.SendAudit(audit);

            };

            // Action
            if (ApplicationServiceContext.Current.IsRunning)
                ApplicationServiceContext.Current.GetService<IThreadPoolService>().QueueUserWorkItem(workitem); // background
            else
                workitem(null); // service is stopped

        }

        /// <summary>
        /// Add user actor
        /// </summary>
        public static void AddUserActor(AuditData audit, IPrincipal principal = null)
        {
            var configService = ApplicationServiceContext.Current.GetService<ISecurityRepositoryService>();
            
            principal = principal ?? AuthenticationContext.Current.Principal;
            // For the user
            audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = ApplicationServiceContext.Current.GetService<IRemoteEndpointResolver>()?.GetRemoteEndpoint(),
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
        public static void AuditOverride(IPrincipal principal, string purposeOfUse, string[] policies, bool success)
        {

            traceSource.TraceVerbose("Create Override audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.EmergencyOverrideStarted, new AuditCode(purposeOfUse, SanteDBClaimTypes.XspaPurposeOfUseClaim));
            AddUserActor(audit, principal);
            AddLocalDeviceActor(audit);

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
        public static void AuditLogin(IPrincipal principal, String identityName, IIdentityProviderService identityProvider, bool successfulLogin = true)
        {

            traceSource.TraceVerbose("Create Login audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, successfulLogin ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.Login));
            AddLocalDeviceActor(audit);
            AddUserActor(audit);
            SendAudit(audit);
        }

        /// <summary>
        /// Audit a login of a principal
        /// </summary>
        public static void AuditLogout(IPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            traceSource.TraceVerbose("Create Logout audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.Success, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.Logout));
            AddLocalDeviceActor(audit);
            AddUserActor(audit, principal);

            SendAudit(audit);
        }

        /// <summary>
        /// Audit the use of a restricted function
        /// </summary>
        public static void AuditNetworkRequestFailure(Exception ex, Uri url, IDictionary<String,String> requestHeaders, IDictionary<String, String> responseHeaders)
        {
            traceSource.TraceVerbose("Create RestrictedFunction audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, OutcomeIndicator.MinorFail, EventIdentifierType.NetworkEntry, CreateAuditActionCode(EventTypeCodes.NetworkActivity));
            AddUserActor(audit);
            AddLocalDeviceActor(audit);
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
                    NameData = ex.ToString()
                });
            else
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/error/{ex.GetType().Name}",
                    Role= AuditableObjectRole.SecurityResource,
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
            traceSource.TraceVerbose("Create session audit");

            AuditData audit = new AuditData(DateTime.Now, ActionType.Execute, success ? OutcomeIndicator.Success : OutcomeIndicator.EpicFail, EventIdentifierType.UserAuthentication, CreateAuditActionCode(EventTypeCodes.SessionStarted));
            AddLocalDeviceActor(audit);
            AddUserActor(audit, principal);

            // Audit the actual session that is created
            
            var cprincipal = principal as IClaimsPrincipal;
            var deviceIdentity = cprincipal?.Identities.OfType<IDeviceIdentity>().FirstOrDefault();
            var applicationIdentity = cprincipal?.Identities.OfType<IApplicationIdentity>().FirstOrDefault();

            if(session != null)
                audit.AuditableObjects.Add(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    ObjectId = BitConverter.ToString(session.Id).Replace("-",""),
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
        /// Audit that a session has begun
        /// </summary>
        public static void AuditSessionStop(ISession session, IPrincipal principal, bool success)
        {
            traceSource.TraceVerbose("End session audit");

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

            audit.AuditableObjects = exportedData.Select(o =>
            {
                var obj = CreateAuditableObject(o, AuditableObjectLifecycle.Export);
                return obj;
            }).ToList();

            SendAudit(audit);
        }

    }
}
