/*
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
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Configuration;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Queue;
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
    /// Extension methods for the audit builder to construct an audit event.
    /// </summary>
    public static class AuditExtensions
    {

        /// <summary>
        /// Append the outcome indicator to the audit
        /// </summary>
        public static IAuditBuilder WithOutcome(this IAuditBuilder me, OutcomeIndicator outcome)
        {
            me.Audit.Outcome = outcome;
            return me;
        }

        public static IAuditBuilder WithOutcome(this IAuditBuilder builder, bool condition, OutcomeIndicator trueOutcome = OutcomeIndicator.Success, OutcomeIndicator falseOutcome = OutcomeIndicator.MinorFail)
            => WithOutcome(builder, condition ? trueOutcome : falseOutcome);

        /// <summary>
        /// With event type
        /// </summary>
        public static IAuditBuilder WithEventType(this IAuditBuilder me, String eventTypeCode, String eventTypeCodeSystem = "http://santedb.org/conceptset/SecurityAuditCode", string displayName = null)
        {
            me.Audit.EventTypeCode = new AuditCode(eventTypeCode, eventTypeCodeSystem);
            if (null != displayName)
            {
                me.Audit.EventTypeCode.DisplayName = displayName;
            }
            return me;
        }

        /// <summary>
        /// With an enum set event type
        /// </summary>
        public static IAuditBuilder WithEventType(this IAuditBuilder me, EventTypeCodes typeCode, string displayName = null)
        {
            var typeCodeWire = typeof(EventTypeCodes).GetRuntimeField(typeCode.ToString()).GetCustomAttribute<XmlEnumAttribute>();
            me.Audit.EventTypeCode = new AuditCode(typeCodeWire.Name, "http://santedb.org/conceptset/SecurityAuditCode");
            if (null != displayName)
            {
                me.Audit.EventTypeCode.DisplayName = displayName;
            }
            return me;
        }

        /// <summary>
        /// With a defined event type 
        /// </summary>
        /// <param name="me"></param>
        /// <param name="eventTypeCode"></param>
        /// <returns></returns>
        public static IAuditBuilder WithEventType(this IAuditBuilder me, AuditCode eventTypeCode)
        {
            me.Audit.EventTypeCode = eventTypeCode;
            return me;
        }

        /// <summary>
        /// With the specified action code
        /// </summary>
        public static IAuditBuilder WithAction(this IAuditBuilder me, ActionType action)
        {
            me.Audit.ActionCode = action;
            return me;
        }

        /// <summary>
        /// Add timestamp
        /// </summary>
        public static IAuditBuilder WithTimestamp(this IAuditBuilder me, DateTimeOffset? timestamp = null)
        {
            me.Audit.Timestamp = timestamp ?? DateTimeOffset.Now;
            return me;
        }

        /// <summary>
        /// Event identiifer set
        /// </summary>
        public static IAuditBuilder WithEventIdentifier(this IAuditBuilder me, EventIdentifierType identifier)
        {
            me.Audit.EventIdentifier = identifier;
            return me;
        }

        /// <summary>
        /// Add a query performed data element
        /// </summary>
        public static IAuditBuilder WithQueryPerformed(this IAuditBuilder me, String queryPerformed)
        {
            me.Audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.SearchCritereon,
                LifecycleType = AuditableObjectLifecycle.Access,
                QueryData = queryPerformed,
                Role = AuditableObjectRole.Query,
                Type = AuditableObjectType.SystemObject
            });
            return me;
        }

        /// <summary>
        /// Add policy authorization to the audit
        /// </summary>
        public static IAuditBuilder WithPolicyAuthorization(this IAuditBuilder me, PolicyDecision policy)
        {
            me.Audit.AuditableObjects.AddRange(policy.Details.Select(o => new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                Role = AuditableObjectRole.SecurityGranularityDefinition,
                Type = AuditableObjectType.SystemObject,
                LifecycleType = AuditableObjectLifecycle.Verification,
                ObjectId = o.PolicyId
            }));
            return me;
        }

        /// <summary>
        /// Add local device information
        /// </summary>
        public static IAuditBuilder WithLocalDevice(this IAuditBuilder me)
        {
            if (null == me.Audit.Actors)
            {
                me.Audit.Actors = new List<AuditActorData>();
            }

            me.Audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = ApplicationServiceContext.Current.GetService<INetworkInformationService>()?.GetHostName(),
                NetworkAccessPointType = NetworkAccessPointType.MachineName,
                UserName = ApplicationServiceContext.Current.GetService<INetworkInformationService>()?.GetMachineName(),
                ActorRoleCode = new List<AuditCode>() {
                    new  AuditCode("110152", "DCM") { DisplayName = "Destination" }
                }
            });
            return me;
        }

        /// <summary>
        /// Append user information
        /// </summary>
        public static IAuditBuilder WithUser(this IAuditBuilder me, IPrincipal principal = null)
        {
            // Use all remote endpoint providers to find the current request
            principal = principal ?? AuthenticationContext.Current.Principal;

            if (null == me.Audit.Actors)
            {
                me.Audit.Actors = new List<AuditActorData>();
            }

            if (principal is IClaimsPrincipal cp)
            {
                foreach (var identity in cp.Identities)
                {
                    if (identity is IDeviceIdentity && identity is IClaimsIdentity did)
                    {
                        me.Audit.Actors.Add(new AuditActorData()
                        {
                            NetworkAccessPointId = RemoteEndpointUtil.Current.GetRemoteClient()?.RemoteAddress,
                            NetworkAccessPointType = NetworkAccessPointType.IPAddress,
                            UserName = did.Name,
                            ActorRoleCode = new List<AuditCode>()
                            {
                                new AuditCode("110153", "DCM") { DisplayName = "Source" }
                            },
                            AlternativeUserId = did.FindFirst(SanteDBClaimTypes.Sid)?.Value
                        });
                    }
                    else if (identity is IApplicationIdentity && identity is IClaimsIdentity aid)
                    {
                        me.Audit.Actors.Add(new AuditActorData()
                        {
                            NetworkAccessPointId = RemoteEndpointUtil.Current.GetRemoteClient()?.RemoteAddress,
                            NetworkAccessPointType = NetworkAccessPointType.IPAddress,
                            UserName = aid.Name,
                            ActorRoleCode = new List<AuditCode>()
                            {
                                new AuditCode("110150", "DCM") { DisplayName = "Application" }
                            },
                            AlternativeUserId = aid.FindFirst(SanteDBClaimTypes.Sid)?.Value
                        });
                    }
                    else if (identity is IClaimsIdentity uid)
                    {
                        me.Audit.Actors.Add(new AuditActorData()
                        {
                            UserName = uid.Name,
                            UserIsRequestor = true,
                            ActorRoleCode = new List<AuditCode>()
                            {
                                new AuditCode("humanuser", "http://terminology.hl7.org/CodeSystem/extra-security-role-type") { DisplayName = "Human User" }
                            },
                            AlternativeUserId = uid.FindFirst(SanteDBClaimTypes.Sid)?.Value
                        });
                    }
                }
            }
            else
            {
                var actor = new AuditActorData()
                {
                    NetworkAccessPointId = RemoteEndpointUtil.Current.GetRemoteClient()?.RemoteAddress,
                    NetworkAccessPointType = NetworkAccessPointType.IPAddress,
                    UserName = principal.Identity.Name
                };

                if (principal.Identity is IApplicationIdentity || principal.Identity is IDeviceIdentity)
                {
                    actor.ActorRoleCode.Add(new AuditCode("110153", "DCM") { DisplayName = "Source" });
                }
                else
                {
                    actor.UserIsRequestor = true;
                    actor.ActorRoleCode.Add(new AuditCode("humanuser", "http://terminology.hl7.org/CodeSystem/extra-security-role-type"));
                }
                me.Audit.Actors.Add(actor);
            }
            return me;
        }

        /// <summary>
        /// With object of patient
        /// </summary>
        public static IAuditBuilder WithPatient(this IAuditBuilder me, Patient patient, AuditableObjectLifecycle lifecycle)
        {
            me.Audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.PatientNumber,
                Role = AuditableObjectRole.Patient,
                Type = AuditableObjectType.Person,
                LifecycleType = lifecycle,
                ObjectId = $"urn:uuid:{patient.Key}",
                NameData = patient.ToString()
            });
            return me;
        }

        /// <summary>
        /// Add patient object to the audit
        /// </summary>
        public static IAuditBuilder WithPerson(this IAuditBuilder me, Person person, AuditableObjectLifecycle lifecycle)
        {
            me.Audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                Role = AuditableObjectRole.Resource,
                Type = AuditableObjectType.Person,
                LifecycleType = lifecycle,
                ObjectId = $"urn:uuid:{person.Key}",
                NameData = person.ToString()
            });
            return me;
        }

        /// <summary>
        /// Add provider object to the audit
        /// </summary>
        public static IAuditBuilder WithProvider(this IAuditBuilder me, Provider provider, AuditableObjectLifecycle lifecycle)
        {
            me.Audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                Role = AuditableObjectRole.Provider,
                Type = AuditableObjectType.Person,
                LifecycleType = lifecycle,
                ObjectId = $"urn:uuid:{provider.Key}",
                NameData = provider.ToString()
            });
            return me;
        }

        /// <summary>
        /// Add act object to the audit
        /// </summary>
        public static IAuditBuilder WithAct(this IAuditBuilder me, Act act, AuditableObjectLifecycle lifecycle)
        {
            me.Audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                Role = AuditableObjectRole.Resource,
                Type = AuditableObjectType.Other,
                LifecycleType = lifecycle,
                ObjectId = $"urn:uuid:{act.Key}",
                NameData = act.ToString()
            });
            return me;
        }

        /// <summary>
        /// Add HTTP information
        /// </summary>
        public static IAuditBuilder WithHttpInformation(this IAuditBuilder me, HttpListenerRequest request)
        {
            me.Audit.AuditableObjects.Add(new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Custom,
                CustomIdTypeCode = new AuditCode("SecurityAuditCodes", "HTTP-Headers"),
                Role = AuditableObjectRole.Query,
                Type = AuditableObjectType.SystemObject,
                QueryData = request.Url.ToString(),
                ObjectData = request.Headers.AllKeys.Where(o => o.Equals("accept", StringComparison.OrdinalIgnoreCase)).Select(
                    h => new ObjectDataExtension(h, Encoding.UTF8.GetBytes(request.Headers[h]))
                    ).ToList()
            });
            return me;
        }

        /// <summary>
        /// With a system object
        /// </summary>
        public static IAuditBuilder WithSystemObjects(this IAuditBuilder me, AuditableObjectRole role, AuditableObjectLifecycle lifecycle, params Uri[] objectIds)
        {
            if (null == me.Audit.AuditableObjects)
            {
                me.Audit.AuditableObjects = new List<AuditableObject>();
            }

            me.Audit.AuditableObjects.AddRange(objectIds.Select(o => new AuditableObject()
            {
                ObjectId = o.ToString(),
                IDTypeCode = AuditableObjectIdType.Uri,
                Type = AuditableObjectType.SystemObject,
                Role = role,
                LifecycleType = lifecycle
            }));
            return me;
        }

        /// <summary>
        /// Adds the specified Auditable Objects
        /// </summary>
        /// <param name="me"></param>
        /// <param name="auditableObjects"></param>
        /// <returns></returns>
        public static IAuditBuilder WithAuditableObjects(this IAuditBuilder me, IEnumerable<AuditableObject> auditableObjects)
        {
            if (null != auditableObjects)
            {
                if (null == me.Audit.AuditableObjects)
                {
                    me.Audit.AuditableObjects = new List<AuditableObject>();
                }

                me.Audit.AuditableObjects.AddRange(auditableObjects.Where(ao => null != ao && me.Audit.AuditableObjects.Contains(ao) != true));
            }
            return me;
        }

        /// <inheritdoc cref="WithAuditableObjects(IAuditBuilder, IEnumerable{AuditableObject})"/>
        public static IAuditBuilder WithAuditableObjects(this IAuditBuilder me, params AuditableObject[] auditableObjects)
            => WithAuditableObjects(me, (IEnumerable<AuditableObject>)auditableObjects);



        /// <summary>
        /// Audit that the audit log was used
        /// </summary>
        /// <param name="action">The action that occurred</param>
        /// <param name="outcome">The outcome of the action</param>
        /// <param name="query">The query which was being executed</param>
        /// <param name="auditIds">The identifiers of any objects disclosed</param>
        public static IAuditBuilder ForAuditLogUsed(this IAuditBuilder me, ActionType action, OutcomeIndicator outcome, string query, params Guid[] auditIds)
            => me
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.AuditLogUsed)
                .WithAction(action)
                .WithOutcome(outcome)
                .WithLocalDevice()
                .WithUser()
                .WithAuditableObjects(auditIds.Select(o => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    LifecycleType = action == ActionType.Delete ? AuditableObjectLifecycle.PermanentErasure : AuditableObjectLifecycle.Disclosure,
                    ObjectId = o.ToString(),
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    CustomIdTypeCode = new AuditCode("SecurityAudit", "http://santedb.org/model")
                }))
                .WithAuditableObjects(!string.IsNullOrEmpty(query) ? new AuditableObject
                {
                    IDTypeCode = AuditableObjectIdType.SearchCritereon,
                    LifecycleType = AuditableObjectLifecycle.Access,
                    QueryData = query,
                    Role = AuditableObjectRole.Query,
                    Type = AuditableObjectType.SystemObject
                } : null);


        /// <summary>
        /// Audit that a synchronization occurred
        /// </summary>
        public static IAuditBuilder ForSynchronization(this IAuditBuilder me, AuditableObjectLifecycle lifecycle, string remoteTarget, OutcomeIndicator outcome, params IdentifiedData[] objects)
        {
            me
                .WithAction(ActionType.Execute)
                .WithOutcome(outcome)
                .WithEventIdentifier(lifecycle == AuditableObjectLifecycle.Import ? EventIdentifierType.Import : EventIdentifierType.Export)
                .WithEventType("Synchronization")
                .WithLocalDevice();

            if (lifecycle == AuditableObjectLifecycle.Export)
            {
                // I am the source
                me.Audit.Actors.First().ActorRoleCode = new List<AuditCode>() { new AuditCode("110153", "DCM") { DisplayName = "Source" } };
                // Remote is the destination
                me.Audit.Actors.Add(new AuditActorData()
                {
                    ActorRoleCode = new List<AuditCode>() { new AuditCode("110152", "DCM") { DisplayName = "Destination" } },
                    NetworkAccessPointType = NetworkAccessPointType.MachineName,
                    NetworkAccessPointId = remoteTarget
                });
            }
            else
            {
                // Remote is the destination
                me.Audit.Actors.Add(new AuditActorData()
                {
                    ActorRoleCode = new List<AuditCode>() { new AuditCode("110153", "DCM") { DisplayName = "Source" } },
                    NetworkAccessPointType = NetworkAccessPointType.MachineName,
                    NetworkAccessPointId = remoteTarget
                });
            }

            if (objects.All(o => o is Bundle))
            {
                objects = objects.OfType<Bundle>().SelectMany(o => o.Item).ToArray();
            }

            me.WithAuditableObjects(objects.OfType<IdentifiedData>().Select(o => AuditUtil.CreateAuditableObject(o, lifecycle)));

            return me;
        }

        /// <summary>
        /// Audit an access control decision
        /// </summary>
        public static IAuditBuilder ForAccessControlDecision(this IAuditBuilder me, IPrincipal principal, string policy, PolicyGrantType action)
        {
            return me
                .WithAction(ActionType.Execute)
                .WithOutcome(action == PolicyGrantType.Grant ? OutcomeIndicator.Success : action == PolicyGrantType.Elevate ? OutcomeIndicator.MinorFail : OutcomeIndicator.SeriousFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.AccessControlDecision)
                .WithLocalDevice()
                .WithUser(principal) //TODO: Check with Justin on this
                .WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode("SecurityPolicy", "http://santedb.org/model"),
                    ObjectId = policy,
                    Role = AuditableObjectRole.SecurityGranularityDefinition,
                    Type = AuditableObjectType.SystemObject,
                    ObjectData = new List<ObjectDataExtension>() { new ObjectDataExtension(policy, action.ToString()) }
                });


        }

        /// <summary>
        /// Autility utility which can be used to send a data audit
        /// </summary>
        public static IAuditBuilder ForEventDataAction<TData>(this IAuditBuilder builder, AuditCode typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, string queryPerformed, PolicyDecision grantInfo, params TData[] data)
        {
            return builder
                .WithAction(action)
                .WithOutcome(outcome)
                .WithEventIdentifier(eventType)
                .WithEventType(typeCode)
                .WithLocalDevice()
                .WithUser()
                .WithAuditableObjects(data?.OfType<TData>()?.SelectMany(o =>
                {
                    if (o is Bundle bundle)
                        return bundle.Item.Select(i => i.ToAuditableObject(lifecycle));
                    else return new AuditableObject[] { o.ToAuditableObject(lifecycle) };
                }))
                .WithAuditableObjects(
                    !string.IsNullOrEmpty(queryPerformed) ? new AuditableObject
                    {
                        IDTypeCode = AuditableObjectIdType.SearchCritereon,
                        LifecycleType = AuditableObjectLifecycle.Access,
                        QueryData = queryPerformed,
                        Role = AuditableObjectRole.Query,
                        Type = AuditableObjectType.SystemObject
                    } : null,
                    null != grantInfo ? grantInfo.ToAuditableObject(AuditableObjectLifecycle.Verification) : null);
        }

        /// <summary>
        /// Audit data action
        /// </summary>
        public static IAuditBuilder ForEventDataAction<TData>(this IAuditBuilder builder, EventTypeCodes typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, String queryPerformed, params TData[] data)
            => builder.ForEventDataAction(AuditUtil.CreateAuditActionCode(typeCode), action, lifecycle, eventType, outcome, queryPerformed, null, data);


        private static AuditableObject ToAuditableObject<TData>(this TData obj, AuditableObjectLifecycle? lifecycle = null)
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
            else if (obj is AuditEventData)
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
            else if (obj is PolicyDecision)
            {
                idTypeCode = AuditableObjectIdType.Uri;
                roleCode = AuditableObjectRole.SecurityGranularityDefinition;
                objType = AuditableObjectType.SystemObject;
            }

            var retVal = new AuditableObject()
            {
                IDTypeCode = idTypeCode,
                CustomIdTypeCode = idTypeCode == AuditableObjectIdType.Custom ? new AuditCode(obj.GetType().Name, $"http://santedb.org/model") : null,
                LifecycleType = lifecycle,
                ObjectId = (obj as IIdentifiedData)?.Key?.ToString() ?? (obj as AuditEventData)?.Key?.ToString() ?? (obj.GetType().GetRuntimeProperty("Id")?.GetValue(obj)?.ToString()) ?? obj.ToString(),
                Role = roleCode,
                Type = objType,
                NameData = obj.ToString()
            };

            if (obj is PolicyDecision pd)
            {
                retVal.ObjectData = pd.Details.Select(o => new ObjectDataExtension(o.PolicyId, new byte[] { (byte)o.Outcome })).ToList();
            }
            return retVal;
        }

        /// <summary>
        /// Audit that sensitve data was disclosed
        /// </summary>
        /// <param name="builder">The audit builder</param>
        /// <param name="result">The result record which was disclosed</param>
        /// <param name="decision">The policy decision which resulted in the disclosure</param>
        /// <param name="disclosed">True if the record was actually disclosed (false if the audit is merely the access is being audited)</param>
        /// <param name="properties">The properties which were disclosed</param>
        public static IAuditBuilder ForSensitiveDisclosure(this IAuditBuilder builder, IdentifiedData result, PolicyDecision decision, bool disclosed, params string[] properties)
            => builder
                .WithAction(ActionType.Read)
                .WithOutcome(disclosed ? OutcomeIndicator.Success : OutcomeIndicator.MinorFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType("SecurityAuditEvent-DisclosureOfSensitiveInformation", "SecurityAuditEventDataEvent", displayName: "Sensitive Data Was Disclosed to User")
                .WithLocalDevice()
                .WithUser()
                .WithAuditableObjects(
                    result.ToAuditableObject(AuditableObjectLifecycle.Disclosure),
                    decision.ToAuditableObject(AuditableObjectLifecycle.Verification),
                    new AuditableObject
                    {
                        IDTypeCode = AuditableObjectIdType.Custom,
                        CustomIdTypeCode = new AuditCode("SecurityAudit-MaskedFields", "SecurityAuditCodes"),
                        LifecycleType = AuditableObjectLifecycle.Access,
                        NameData = String.Join(";", properties),
                        Type = AuditableObjectType.SystemObject
                    }
                );

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        public static IAuditBuilder ForSecurityDeletionAction(this IAuditBuilder builder, IEnumerable<Object> objects, bool success, IEnumerable<string> changedProperties)
            => builder
                .WithAction(ActionType.Delete)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.SecurityObjectChanged)
                .WithLocalDevice()
                .WithUser()
                .WithAuditableObjects(objects.Select(obj => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode(obj.GetType().Name, "http://santedb.org/model"),
                    ObjectId = ((obj as IIdentifiedData)?.Key ?? Guid.Empty).ToString(),
                    LifecycleType = AuditableObjectLifecycle.LogicalDeletion,
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject
                }));


        /// <summary>
        /// Audit application start
        /// </summary>
        public static IAuditBuilder ForApplicationStart(this IAuditBuilder builder)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.Success)
                .WithEventIdentifier(EventIdentifierType.ApplicationActivity)
                .WithEventType(EventTypeCodes.ApplicationStart)
                .WithLocalDevice()
            ;

        /// <summary>
        /// Audit application stop
        /// </summary>
        public static IAuditBuilder ForApplicationStop(this IAuditBuilder builder)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.Success)
                .WithEventIdentifier(EventIdentifierType.ApplicationActivity)
                .WithEventType(EventTypeCodes.ApplicationStop)
                .WithLocalDevice()
            ;

        /// <summary>
        /// Audit a login of a user principal
        /// </summary>
        public static IAuditBuilder ForUserLogin(this IAuditBuilder builder, IPrincipal principal, bool successfulLogin = true)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(successfulLogin, falseOutcome: OutcomeIndicator.SeriousFail)
                .WithEventIdentifier(EventIdentifierType.UserAuthentication)
                .WithEventType(EventTypeCodes.Login)
                .WithLocalDevice()
                .WithUser(principal)
                .WithAuditableObjects(new AuditableObject
                {
                    IDTypeCode = AuditableObjectIdType.UserIdentifier,
                    ObjectId = principal.Identity.Name,
                    LifecycleType = AuditableObjectLifecycle.NotSet,
                    Role = AuditableObjectRole.SecurityUser,
                    Type = AuditableObjectType.SystemObject
                });

        /// <summary>
        /// Audit a logout of a user principal
        /// </summary>
        public static IAuditBuilder ForUserLogout(this IAuditBuilder builder, IPrincipal principal)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.Success)
                .WithEventIdentifier(EventIdentifierType.UserAuthentication)
                .WithEventType(EventTypeCodes.Logout)
                .WithLocalDevice()
                .WithUser(principal);

        /// <summary>
        /// Audit a network request failure
        /// </summary>
        public static IAuditBuilder ForNetworkRequestFailure(this IAuditBuilder builder, Exception ex, Uri url, NameValueCollection requestHeaders, NameValueCollection responseHeaders)
            => ForNetworkRequestFailure(builder, ex, url, requestHeaders.AllKeys.ToDictionary(o => o, o => requestHeaders[o]), responseHeaders?.AllKeys.ToDictionary(o => o, o => responseHeaders[o]));

        /// <summary>
        /// Audit a network request failure
        /// </summary>
        public static IAuditBuilder ForNetworkRequestFailure(this IAuditBuilder builder, Exception ex, Uri url, IDictionary<String, String> requestHeaders, IDictionary<String, String> responseHeaders)
        {
            builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.MinorFail)
                .WithEventIdentifier(EventIdentifierType.NetworkActivity)
                .WithEventType(EventTypeCodes.NetworkActivity)
                .WithLocalDevice()
                .WithUser()
                .WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Access,
                    ObjectId = "urlQuery",
                    QueryData = url.ToString(),
                    Role = AuditableObjectRole.Resource,
                    Type = AuditableObjectType.SystemObject
                });

            // Get root cause
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            if (ex is PolicyViolationException polvex)
                builder.WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/policy/{polvex.PolicyId}",
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    NameData = ex.Message
                });
            else
                builder.WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/error/{ex.GetType().Name}",
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    NameData = ex.Message
                });

            if (requestHeaders != null)
            {
                builder.WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    Type = AuditableObjectType.Other,
                    Role = AuditableObjectRole.RoutingCriteria,
                    ObjectId = "HttpRequest",
                    ObjectData = requestHeaders.Select(o => new ObjectDataExtension(o.Key, o.Value)).ToList()
                });
            }
            if (responseHeaders != null)
            {
                builder.WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    Type = AuditableObjectType.Other,
                    Role = AuditableObjectRole.Report,
                    ObjectId = "HttpResponse",
                    ObjectData = responseHeaders.Select(o => new ObjectDataExtension(o.Key, o.Value)).ToList()
                });
            }

            return builder;
        }

        /// <summary>
        /// Audit that a session has begun
        /// </summary>
        public static IAuditBuilder ForSessionStart(this IAuditBuilder builder, ISession session, IPrincipal principal, bool success)
        {
            builder.WithAction(ActionType.Execute)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.UserAuthentication)
                .WithEventType(EventTypeCodes.SessionStarted)
                .WithLocalDevice()
                .WithUser(principal);

            var policies = session?.Claims.Where(o => o.Type == SanteDBClaimTypes.SanteDBScopeClaim).Select(o => o.Value);

            // Audit the actual session that is created
            var cprincipal = principal as IClaimsPrincipal;
            var deviceIdentity = cprincipal?.Identities.OfType<IDeviceIdentity>().FirstOrDefault();
            var applicationIdentity = cprincipal?.Identities.OfType<IApplicationIdentity>().FirstOrDefault();

            if (session != null)
            {
                builder.WithAuditableObjects(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    ObjectId = BitConverter.ToString(session.Id).Replace("-", ""),
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode("SecuritySession", "http://santedb.org/model"),
                    LifecycleType = AuditableObjectLifecycle.Creation,
                    ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("method", principal.Identity?.AuthenticationType),
                        deviceIdentity != cprincipal.Identity && applicationIdentity != cprincipal.Identity ? new ObjectDataExtension("userIdentity", principal.Identity.Name) : null,
                        deviceIdentity != null ? new ObjectDataExtension("deviceIdentity", deviceIdentity?.Name) : null,
                        applicationIdentity != null ? new ObjectDataExtension("applicationIdentity", applicationIdentity?.Name) : null,
                        new ObjectDataExtension("scope", String.Join("; ", policies ?? new String[] { "*" }))
                    }.OfType<ObjectDataExtension>().ToList()
                });
            }
            else
            {
                builder.WithAuditableObjects(new AuditableObject()
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
            }

            return builder;
        }

        /// <summary>
        /// Audit that a session has begun
        /// </summary>
        public static IAuditBuilder ForSessionStop(this IAuditBuilder builder, ISession session, IPrincipal principal, bool success)
        {
            builder.WithAction(ActionType.Execute)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.UserAuthentication)
                .WithEventType(EventTypeCodes.SessionStopped)
                .WithLocalDevice()
                .WithUser(principal);


            // Audit the actual session that is created
            var cprincipal = (principal ?? AuthenticationContext.Current.Principal) as IClaimsPrincipal;
            var deviceIdentity = cprincipal?.Identities.OfType<IDeviceIdentity>().FirstOrDefault();
            var applicationIdentity = cprincipal?.Identities.OfType<IApplicationIdentity>().FirstOrDefault();

            if (session != null)
            {
                builder.WithAuditableObjects(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    ObjectId = BitConverter.ToString(session.Id).Replace("-", ""),
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode("SecuritySession", "http://santedb.org/model"),
                    LifecycleType = AuditableObjectLifecycle.PermanentErasure,
                    ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("method", principal.Identity?.AuthenticationType),
                        deviceIdentity != cprincipal.Identity && applicationIdentity != cprincipal.Identity ? new ObjectDataExtension("userIdentity", principal.Identity.Name) : null,
                        deviceIdentity != null ? new ObjectDataExtension("deviceIdentity", deviceIdentity?.Name) : null,
                        applicationIdentity != null ? new ObjectDataExtension("applicationIdentity", applicationIdentity?.Name) : null
                    }.OfType<ObjectDataExtension>().ToList()
                });
            }
            else
            {
                builder.WithAuditableObjects(new AuditableObject()
                {
                    Role = AuditableObjectRole.SecurityResource,
                    LifecycleType = AuditableObjectLifecycle.PermanentErasure,
                    ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("method", principal.Identity?.AuthenticationType),
                        deviceIdentity != cprincipal.Identity && applicationIdentity != cprincipal.Identity ? new ObjectDataExtension("userIdentity", principal.Identity.Name) : null,
                        deviceIdentity != null ? new ObjectDataExtension("deviceIdentity", deviceIdentity?.Name) : null,
                        applicationIdentity != null ? new ObjectDataExtension("applicationIdentity", applicationIdentity?.Name) : null
                    }.OfType<ObjectDataExtension>().ToList()
                });
            }

            return builder;
        }

        /// <summary>
        /// Audit the export of data
        /// </summary>
        public static void ForDataExport(this IAuditBuilder builder, params object[] exportedData)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.Success)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.Export)
                .WithLocalDevice()
                .WithUser()
                .WithAuditableObjects(exportedData.Where(o => o != null).Select(o => o.ToAuditableObject(AuditableObjectLifecycle.Export)));


        /// <summary>
        /// Audit masking of a particular object
        /// </summary>
        /// <param name="targetOfMasking">The object which was masked</param>
        /// <param name="wasRemoved">True if the object was removed instead of masked</param>
        /// <param name="maskedObject">The object that was masked</param>
        /// <param name="decision">The decision which caused the masking to occur</param>
        public static IAuditBuilder ForMasking<TModel>(this IAuditBuilder builder, TModel targetOfMasking, PolicyDecision decision, bool wasRemoved, IdentifiedData maskedObject)
            => ForEventDataAction(
                builder,
                new AuditCode("SecurityAuditCode-Masking", "SecurityAuditCode") { DisplayName = "Mask Sensitive Data" },
                ActionType.Execute,
                AuditableObjectLifecycle.Deidentification,
                EventIdentifierType.ApplicationActivity,
                OutcomeIndicator.Success,
                null,
                decision,
                targetOfMasking);

        /// <summary>
        /// Audit the creation of an object
        /// </summary>
        public static IAuditBuilder ForCreate<TData>(this IAuditBuilder builder, OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
            => ForEventDataAction(
                builder,
                new AuditCode("SecurityAuditCode-CreateInstances", "SecurityAuditEventDataEvent") { DisplayName = "Create New Record" },
                ActionType.Create,
                AuditableObjectLifecycle.Creation,
                EventIdentifierType.Import,
                outcome,
                queryPerformed,
                null,
                resourceData);


        /// <summary>
        /// Audit the update of an object
        /// </summary>
        public static IAuditBuilder ForUpdate<TData>(this IAuditBuilder builder, OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
            => ForEventDataAction(
                builder,
                new AuditCode("SecurityAuditCode-UpdateInstances", "SecurityAuditEventDataEvent") { DisplayName = "Update Existing Record" },
                ActionType.Update,
                AuditableObjectLifecycle.Amendment,
                EventIdentifierType.Import,
                outcome,
                queryPerformed,
                null,
                resourceData);


        /// <summary>
        /// Audit a deletion
        /// </summary>
        public static IAuditBuilder ForDelete<TData>(this IAuditBuilder builder, OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
            => ForEventDataAction(
                builder,
                new AuditCode("SecurityAuditCode-DeleteInstances", "SecurityAuditEventDataEvent") { DisplayName = "Delete Existing Record" },
                ActionType.Delete,
                AuditableObjectLifecycle.LogicalDeletion,
                EventIdentifierType.Import,
                outcome,
                queryPerformed,
                null,
                resourceData);


        /// <summary>
        /// Audit the update of an object
        /// </summary>
        public static IAuditBuilder ForQuery<TData>(this IAuditBuilder builder, OutcomeIndicator outcome, string queryPerformed, params TData[] results)
            => ForEventDataAction(
                builder,
                EventTypeCodes.Query,
                ActionType.Execute,
                AuditableObjectLifecycle.Disclosure,
                EventIdentifierType.Query,
                outcome,
                queryPerformed,
                null,
                results);


        /// <summary>
        /// Audit the update of an object
        /// </summary>
        public static IAuditBuilder ForRead<TData>(this IAuditBuilder builder, OutcomeIndicator outcome, string queryPerformed, params TData[] results)
            => ForEventDataAction(
                builder,
                EventTypeCodes.Query,
                ActionType.Read,
                AuditableObjectLifecycle.Disclosure,
                EventIdentifierType.Query,
                outcome,
                queryPerformed,
                null,
                results);


        /// <summary>
        /// Audit that security objects were created
        /// </summary>
        public static IAuditBuilder ForSecurityCreationAction(this IAuditBuilder builder, IEnumerable<object> objects, bool success, IEnumerable<string> changedProperties)
            => builder
                .WithAction(ActionType.Create)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.SecurityObjectChanged)
                .WithLocalDevice()
                .WithUser()
                .WithAuditableObjects(objects.Select(obj => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode(obj.GetType().Name, "http://santedb.org/model"),
                    ObjectId = ((obj as IIdentifiedData)?.Key ?? Guid.Empty).ToString(),
                    LifecycleType = AuditableObjectLifecycle.Creation,
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject
                }));

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        public static IAuditBuilder ForSecurityAttributeAction(this IAuditBuilder builder, IEnumerable<object> objects, bool success, params string[] changedProperties)
            => builder
                .WithAction(ActionType.Update)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.SecurityAttributesChanged)
                .WithLocalDevice()
                .WithUser()
                .WithAuditableObjects(objects.Select(obj => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode(obj.GetType().Name, "http://santedb.org/model"),
                    ObjectId = ((obj as IIdentifiedData)?.Key ?? Guid.Empty).ToString(),
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
                }));

        

        /// <summary>
        /// Audit an override operation
        /// </summary>
        public static IAuditBuilder ForOverride(this IAuditBuilder builder, ISession session, IPrincipal principal, string purposeOfUse, string[] policies, bool success)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.EmergencyOverrideStarted)
                .WithAuditableObjects(
                    new AuditableObject()
                    {
                        ObjectId = SanteDBClaimTypes.PurposeOfUse,
                        LifecycleType = AuditableObjectLifecycle.NotSet,
                        Role = AuditableObjectRole.SecurityGranularityDefinition,
                        Type = AuditableObjectType.SystemObject,
                        NameData = purposeOfUse
                    },
                    null != session ? new AuditableObject()
                    {
                        Role = AuditableObjectRole.SecurityResource,
                        ObjectId = BitConverter.ToString(session.Id).Replace("-", ""),
                        IDTypeCode = AuditableObjectIdType.Custom,
                        CustomIdTypeCode = new AuditCode("SecuritySession", "http://santedb.org/model"),
                        LifecycleType = AuditableObjectLifecycle.Creation
                    } : null
                )
                .WithAuditableObjects(policies.Select(o => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    ObjectId = $"urn:oid:{o}",
                    Type = AuditableObjectType.SystemObject,
                    Role = AuditableObjectRole.SecurityGranularityDefinition
                }));


        /// <summary>
        /// Creates a new <see cref="IAuditBuilder" /> instance tied to this service for dispatch.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="actionCode"></param>
        /// <param name="outcome"></param>
        /// <param name="eventIdentifier"></param>
        /// <param name="eventTypeCode"></param>
        /// <returns></returns>
        public static IAuditBuilder Audit(this IAuditService service, DateTimeOffset timeStamp, ActionType actionCode, OutcomeIndicator outcome, EventIdentifierType eventIdentifier, AuditCode eventTypeCode)
            => service.Audit()
                .WithTimestamp(timeStamp)
                .WithAction(actionCode)
                .WithOutcome(outcome)
                .WithEventIdentifier(eventIdentifier)
                .WithEventType(eventTypeCode);

        /// <summary>
        /// Creates a new <see cref="IAuditBuilder" /> instance tied to this service for dispatch.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="actionCode"></param>
        /// <param name="outcome"></param>
        /// <param name="eventIdentifier"></param>
        /// <param name="eventTypeCode"></param>
        /// <returns></returns>
        public static IAuditBuilder Audit(this IAuditService service, DateTimeOffset timeStamp, ActionType actionCode, OutcomeIndicator outcome, EventIdentifierType eventIdentifier, EventTypeCodes eventTypeCode)
            => service.Audit()
                .WithTimestamp(timeStamp)
                .WithAction(actionCode)
                .WithOutcome(outcome)
                .WithEventIdentifier(eventIdentifier)
                .WithEventType(eventTypeCode);
    }
}