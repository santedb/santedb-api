/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Data.Import;
using SanteDB.Core.Data.Quality.Configuration;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Model.Collection;
using SanteDB.Core.Model.Constants;
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
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Xml.Serialization;
#pragma warning disable CS0612

namespace SanteDB.Core.Security.Audit
{
    /// <summary>
    /// Extension methods for the audit builder to construct an audit event.
    /// </summary>
    public static class AuditExtensions
    {
        /// <summary>
        /// Convert to an auditable object
        /// </summary>
        private static AuditableObject ConvertSystemObjectToAuditableObject(AuditableObjectRole role, AuditableObjectLifecycle lifecycle, object item)
        {
            var ao = new AuditableObject()
            {
                Type = AuditableObjectType.SystemObject,
                Role = role,
                LifecycleType = lifecycle
            };

            switch (item)
            {
                case Uri ur:
                    ao.IDTypeCode = AuditableObjectIdType.Uri;
                    ao.ObjectId = ur.ToString();
                    break;
                case X509Certificate2 cert:
                    ao.IDTypeCode = AuditableObjectIdType.SearchCritereon;
                    ao.ObjectId = cert.SubjectName.ToString();
                    ao.NameData = cert.ToString();
                    break;
                case SecurityDevice sd:
                    ao.IDTypeCode = AuditableObjectIdType.Uri;
                    ao.ObjectId = $"urn:uuid:{sd.Key}";
                    ao.NameData = sd.Name;
                    break;
                case SecurityApplication sa:
                    ao.IDTypeCode = AuditableObjectIdType.Uri;
                    ao.ObjectId = $"urn:uuid:{sa.Key}";
                    ao.NameData = sa.Name;
                    break;
                case SecurityUser su:
                    ao.IDTypeCode = AuditableObjectIdType.Uri;
                    ao.ObjectId = $"urn:uuid:{su.Key}";
                    ao.NameData = su.UserName;
                    break;
                case SecurityProvenance sp:
                    ao.IDTypeCode = AuditableObjectIdType.Uri;
                    ao.ObjectId = $"urn:uuid:{sp.Key}";
                    ao.ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("app", sp.ApplicationKey.GetValueOrDefault().ToString()),
                        new ObjectDataExtension("dev", sp.DeviceKey.GetValueOrDefault().ToString()),
                        new ObjectDataExtension("usr", sp.UserKey.GetValueOrDefault().ToString()),
                        new ObjectDataExtension("ses", sp.SessionKey.GetValueOrDefault().ToString())
                    };
                    break;
                default:
                    ao.IDTypeCode = AuditableObjectIdType.NotSpecified;
                    ao.ObjectId = item.ToString();
                    break;
            }
            return ao;
        }

        /// <summary>
        /// Append the outcome indicator to the audit
        /// </summary>
        public static IAuditBuilder WithOutcome(this IAuditBuilder me, OutcomeIndicator outcome)
        {
            me.Audit.Outcome = outcome;
            return me;
        }

        /// <summary>
        /// Modify the audit <paramref name="builder"/> to have <paramref name="condition"/>
        /// </summary>
        /// <param name="builder">The audit build on which the outcome should be appended</param>
        /// <param name="condition">The condition which determines the outcome</param>
        /// <param name="trueOutcome">The status outcome code for the success case</param>
        /// <param name="falseOutcome">The status outcome code for the fail case</param>
        /// <returns>The modified audit builder</returns>
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
        /// With sensitivty
        /// </summary>
        /// <param name="me"></param>
        /// <param name="resourceSensitivity"></param>
        /// <returns></returns>
        public static IAuditBuilder WithSensitivity(this IAuditBuilder me, ResourceSensitivityClassification resourceSensitivity)
        {
            me.Audit.Sensitivity = resourceSensitivity;
            return me;
        }

        /// <summary>
        /// Event identifier set
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
                ObjectId = o.Policy.Oid
            }));
            return me;
        }

        /// <summary>
        /// Local is destination
        /// </summary>
        public static IAuditBuilder WithLocalDestination(this IAuditBuilder me)
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
                    ExtendedAuditCodes.ActorRoleDestination
                }
            });
            return me;

        }

        /// <summary>
        /// Add a destination device information
        /// </summary>
        public static IAuditBuilder WithRemoteDestination(this IAuditBuilder me, Uri remoteDestination)
        {
            if (null == me.Audit.Actors)
            {
                me.Audit.Actors = new List<AuditActorData>();
            }

            me.Audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = remoteDestination.Host,
                NetworkAccessPointType = remoteDestination.HostNameType == UriHostNameType.Dns ? NetworkAccessPointType.MachineName : NetworkAccessPointType.IPAddress,
                ActorRoleCode = new List<AuditCode>() {
                    ExtendedAuditCodes.ActorRoleDestination
                }
            });
            return me;
        }

        /// <summary>
        /// Add a destination device information
        /// </summary>
        public static IAuditBuilder WithRemoteDestination(this IAuditBuilder me, RemoteEndpointInfo remoteDestination)
        {
            if (null == me.Audit.Actors)
            {
                me.Audit.Actors = new List<AuditActorData>();
            }

            me.Audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = remoteDestination.RemoteAddress,
                NetworkAccessPointType = IPAddress.TryParse(remoteDestination.RemoteAddress, out _) ? NetworkAccessPointType.IPAddress : NetworkAccessPointType.MachineName,
                ActorRoleCode = new List<AuditCode>() {
                    ExtendedAuditCodes.ActorRoleDestination

                }
            });
            return me;
        }

        /// <summary>
        /// Add source device information
        /// </summary>
        public static IAuditBuilder WithRemoteSource(this IAuditBuilder me, RemoteEndpointInfo remoteEndpoint)
        {
            if (null == remoteEndpoint) // there is no remote endpoint information (common for file processing or tests)
            {
                return me;
            }
            if (null == me.Audit.Actors)
            {
                me.Audit.Actors = new List<AuditActorData>();
            }

            me.Audit.Actors.Add(new AuditActorData()
            {
                NetworkAccessPointId = remoteEndpoint.RemoteAddress,
                NetworkAccessPointType = NetworkAccessPointType.IPAddress,
                AlternativeUserId = remoteEndpoint.ForwardInformation,
                ActorRoleCode = new List<AuditCode>() {
                    ExtendedAuditCodes.ActorRoleSource
                }
            });

            return me;
        }

        /// <summary>
        /// Add source device information
        /// </summary>
        public static IAuditBuilder WithLocalSource(this IAuditBuilder me)
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
                    ExtendedAuditCodes.ActorRoleSource
                }
            });

            return me;
        }

        /// <summary>
        /// Append principal information to the audit. Principals can represent a user, application or device, and any combination thereof. For example, a single principal can be composed of a User, Application and Device together.
        /// </summary>
        public static IAuditBuilder WithPrincipal(this IAuditBuilder me, IPrincipal principal = null)
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
                            UserName = did.Name,
                            ActorRoleCode = new List<AuditCode>()
                            {
                                ExtendedAuditCodes.ActorRoleDevice,
                                ExtendedAuditCodes.ActorRoleCustodian
                            },
                            AlternativeUserId = did.FindFirst(SanteDBClaimTypes.SecurityId)?.Value
                        });
                    }
                    else if (identity is IApplicationIdentity && identity is IClaimsIdentity aid)
                    {
                        me.Audit.Actors.Add(new AuditActorData()
                        {
                            UserName = aid.Name,
                            ActorRoleCode = new List<AuditCode>()
                            {
                                ExtendedAuditCodes.ActorRoleApplication
                            },
                            AlternativeUserId = aid.FindFirst(SanteDBClaimTypes.SecurityId)?.Value
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
                                ExtendedAuditCodes.ActorRoleHuman
                            },
                            AlternativeUserId = uid.FindFirst(SanteDBClaimTypes.SecurityId)?.Value
                        });
                    }
                }
            }
            else
            {
                var actor = new AuditActorData()
                {
                    UserName = principal.Identity.Name
                };

                if (principal.Identity is IApplicationIdentity)
                {
                    actor.ActorRoleCode.Add(ExtendedAuditCodes.ActorRoleApplication);
                }
                else if (principal.Identity is IDeviceIdentity)
                {
                    actor.ActorRoleCode.Add(ExtendedAuditCodes.ActorRoleDevice);

                }
                else
                {
                    actor.UserIsRequestor = true;
                    actor.ActorRoleCode.Add(ExtendedAuditCodes.ActorRoleHuman);
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
        public static IAuditBuilder WithIdentifiedData(this IAuditBuilder me, AuditableObjectLifecycle lifecycle, IdentifiedData data)
        {
            var ao = new AuditableObject()
            {
                IDTypeCode = AuditableObjectIdType.Uri,
                Type = AuditableObjectType.NotSpecified,
                LifecycleType = lifecycle,
                ObjectId = $"urn:uuid:{data.Key}",
                ObjectData = new List<ObjectDataExtension>()
            };

            switch (data)
            {
                case Patient pat:
                    ao.Role = AuditableObjectRole.Patient;
                    ao.NameData = pat.LoadProperty(o => o.Names).FirstOrDefault(o => o.NameUseKey == NameUseKeys.OfficialRecord)?.ToDisplay();
                    break;
                case Provider pvd:
                    ao.Role = AuditableObjectRole.Provider;
                    ao.ObjectData.Add(new ObjectDataExtension("specialty", pvd.LoadProperty(o => o.Specialty)?.Mnemonic));
                    ao.NameData = pvd.LoadProperty(o => o.Names).FirstOrDefault(o => o.NameUseKey == NameUseKeys.License)?.ToDisplay();
                    break;
                case Person psn:
                    ao.Role = AuditableObjectRole.Resource;
                    ao.Type = AuditableObjectType.Person;
                    ao.NameData = psn.LoadProperty(o => o.Names).FirstOrDefault(o => o.NameUseKey == NameUseKeys.OfficialRecord)?.ToDisplay();
                    break;
                case Place plc:
                    ao.Role = AuditableObjectRole.Location;
                    ao.Type = AuditableObjectType.Other;
                    ao.NameData = plc.LoadProperty(o => o.Names).FirstOrDefault(o => o.NameUseKey == NameUseKeys.Assigned)?.ToDisplay();
                    break;
                case Organization org:
                    ao.Role = AuditableObjectRole.Resource;
                    ao.Type = AuditableObjectType.Organization;
                    ao.NameData = org.LoadProperty(o => o.Names).FirstOrDefault(o => o.NameUseKey == NameUseKeys.Assigned)?.ToDisplay();
                    break;
                case Entity de:
                    ao.Role = AuditableObjectRole.Resource;
                    ao.NameData = de.LoadProperty(o => o.Names).FirstOrDefault(o => o.NameUseKey == NameUseKeys.Assigned)?.ToDisplay();
                    break;
                case PatientEncounter pe:
                    ao.Role = AuditableObjectRole.MasterFile;
                    ao.Type = AuditableObjectType.Other;
                    break;

                case Act act:
                    if (act.ClassConceptKey == ActClassKeys.List || act.ClassConceptKey == ActClassKeys.Battery)
                    {
                        ao.Role = AuditableObjectRole.List;
                    }
                    else if (act.MoodConceptKey == MoodConceptKeys.Proposal || act.MoodConceptKey == MoodConceptKeys.Intent ||
                        act.MoodConceptKey == MoodConceptKeys.Promise)
                    {
                        ao.Role = AuditableObjectRole.Job;
                    }
                    else if (act.MoodConceptKey == MoodConceptKeys.Eventoccurrence)
                    {
                        ao.Role = AuditableObjectRole.Report;
                    }
                    else
                    {
                        return me;
                    }
                    ao.Type = AuditableObjectType.Other;
                    ao.ObjectData.Add(new ObjectDataExtension("mood", act.LoadProperty(o => o.MoodConcept)?.Mnemonic));
                    ao.ObjectData.Add(new ObjectDataExtension("state", act.LoadProperty(o => o.StatusConcept)?.Mnemonic));
                    break;
                default:
                    return me;
            }
            me.Audit.AuditableObjects.Add(ao);
            return me;
        }

        /// <summary>
        /// Add HTTP information
        /// </summary>
        public static IAuditBuilder WithHttpInformation(this IAuditBuilder me, HttpListenerRequest request)
        {
            if (request != null)
            {
                me.Audit.AuditableObjects.Add(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeHttpHeaders,
                    Role = AuditableObjectRole.Query,
                    Type = AuditableObjectType.SystemObject,
                    QueryData = request.Url.ToString(),
                    ObjectData = request.Headers.AllKeys.Where(o => o.Equals("accept", StringComparison.OrdinalIgnoreCase)).Select(
                        h => new ObjectDataExtension(h, Encoding.UTF8.GetBytes(request.Headers[h]))
                        ).ToList()
                });
            }
            return me;
        }

        /// <summary>
        /// With a system object
        /// </summary>
        public static IAuditBuilder WithSystemObjects(this IAuditBuilder me, AuditableObjectRole role, AuditableObjectLifecycle lifecycle, params object[] objectIds)
        {
            if (null == me.Audit.AuditableObjects)
            {
                me.Audit.AuditableObjects = new List<AuditableObject>();
            }

            me.Audit.AuditableObjects.AddRange(objectIds.Select(o => ConvertSystemObjectToAuditableObject(role, lifecycle, o)));

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
        /// Conditionally Applies a section of the fluent syntax to the audit. 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="condition">Condition that evaluates to true/false.</param>
        /// <param name="then">A function/lambda that is executed when the condition is true.</param>
        /// <param name="otherwise">An optional function/lambda that is executed when the condition is false.</param>
        /// <returns>The <see cref="IAuditBuilder"/> in the current chain.</returns>
        public static IAuditBuilder If(this IAuditBuilder builder, bool condition, Func<IAuditBuilder, IAuditBuilder> then, Func<IAuditBuilder, IAuditBuilder> otherwise = null)
        {
            if (condition)
            {
                return then(builder);
            }
            else if (null != otherwise)
            {
                return otherwise(builder);
            }
            return builder;
        }


        /// <summary>
        /// Audit that the audit log was used
        /// </summary>
        /// <param name="action">The action that occurred</param>
        /// <param name="outcome">The outcome of the action</param>
        /// <param name="query">The query which was being executed</param>
        /// <param name="auditIds">The identifiers of any objects disclosed</param>
        /// <param name="me">The audit build on which the log information should be appened</param>
        [Obsolete]
        public static IAuditBuilder ForAuditLogUsed(this IAuditBuilder me, ActionType action, OutcomeIndicator outcome, string query, params Guid[] auditIds)
            => me
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.AuditLogUsed)
                .WithAction(action)
                .WithOutcome(outcome)
                .WithPrincipal()
                .WithLocalDestination()
                .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithAuditableObjects(auditIds.Select(o => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    LifecycleType = action == ActionType.Delete ? AuditableObjectLifecycle.PermanentErasure : AuditableObjectLifecycle.Disclosure,
                    ObjectId = o.ToString(),
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeAuditObject
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
        [Obsolete]
        public static IAuditBuilder ForSynchronization(this IAuditBuilder me, AuditableObjectLifecycle lifecycle, string remoteTarget, OutcomeIndicator outcome, params IdentifiedData[] objects)
        {
            me
                .WithAction(ActionType.Execute)
                .WithOutcome(outcome)
                .WithEventIdentifier(lifecycle == AuditableObjectLifecycle.Import ? EventIdentifierType.Import : EventIdentifierType.Export)
                .WithEventType("Synchronization")
                .WithLocalSource()
                .WithRemoteDestination(new Uri(remoteTarget));

            if (lifecycle == AuditableObjectLifecycle.Export)
            {
                // I am the source
                me.Audit.Actors.First().ActorRoleCode = new List<AuditCode>() { ExtendedAuditCodes.ActorRoleSource };
                // Remote is the destination
                me.Audit.Actors.Add(new AuditActorData()
                {
                    ActorRoleCode = new List<AuditCode>() { ExtendedAuditCodes.ActorRoleDestination },
                    NetworkAccessPointType = NetworkAccessPointType.MachineName,
                    NetworkAccessPointId = remoteTarget
                });
            }
            else
            {
                // Remote is the destination
                me.Audit.Actors.Add(new AuditActorData()
                {
                    ActorRoleCode = new List<AuditCode>() { ExtendedAuditCodes.ActorRoleSource },
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
        [Obsolete]
        public static IAuditBuilder ForAccessControlDecision(this IAuditBuilder me, IPrincipal principal, string policy, PolicyGrantType action)
        {
            return me
                .WithAction(ActionType.Execute)
                .WithOutcome(action == PolicyGrantType.Grant ? OutcomeIndicator.Success : action == PolicyGrantType.Elevate ? OutcomeIndicator.MinorFail : OutcomeIndicator.SeriousFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)
                .WithEventType(EventTypeCodes.AccessControlDecision)
                .WithLocalSource()
                .WithPrincipal(principal)
                .WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypePolicy,
                    ObjectId = policy,
                    Role = AuditableObjectRole.SecurityGranularityDefinition,
                    Type = AuditableObjectType.SystemObject,
                    ObjectData = new List<ObjectDataExtension>() { new ObjectDataExtension(policy, action.ToString()) }
                });


        }

        /// <summary>
        /// Add the specified <paramref name="objects"/> to the <paramref name="builder"/>, converting them to appropriate auditing format
        /// </summary>
        /// <typeparam name="TData">The type of objects to be added</typeparam>
        /// <param name="builder">The <see cref="IAuditBuilder"/> to add <paramref name="objects"/></param>
        /// <param name="lifecycle">The lifecycle of these objects in the context of the audit</param>
        /// <param name="objects">The objects to be added</param>
        /// <returns>The updated objects</returns>
        public static IAuditBuilder WithObjects<TData>(this IAuditBuilder builder, AuditableObjectLifecycle lifecycle, params TData[] objects)
        {
            return builder.WithAuditableObjects(objects.SelectMany(o =>
                {
                    if (o is IResourceCollection bundle)
                    {
                        return bundle.Item.Select(i => i.ToAuditableObject());
                    }
                    else
                    {
                        return new AuditableObject[] { o.ToAuditableObject(lifecycle) };
                    }
                }
            ).OfType<AuditableObject>());
        }


        /// <summary>
        /// Autility utility which can be used to send a data audit
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForEventDataAction<TData>(this IAuditBuilder builder, AuditCode typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, string queryPerformed, PolicyDecision grantInfo, params TData[] data)
        {
            return builder
                .WithAction(action)
                .WithOutcome(outcome)
                .WithEventIdentifier(eventType)
                .WithEventType(typeCode)
                .WithLocalDestination()
                .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithPrincipal()
                .WithAuditableObjects(data?.OfType<TData>()?.SelectMany(o =>
                {
                    if (o is Bundle bundle)
                    {
                        return bundle.Item.Select(i => i.ToAuditableObject(lifecycle));
                    }
                    else
                    {
                        return new AuditableObject[] { o.ToAuditableObject(lifecycle) };
                    }
                }).OfType<AuditableObject>())
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
        [Obsolete]
        public static IAuditBuilder ForEventDataAction<TData>(this IAuditBuilder builder, EventTypeCodes typeCode, ActionType action, AuditableObjectLifecycle lifecycle, EventIdentifierType eventType, OutcomeIndicator outcome, String queryPerformed, params TData[] data)
            => builder.ForEventDataAction(AuditUtil.CreateAuditActionCode(typeCode), action, lifecycle, eventType, outcome, queryPerformed, null, data);


        private static AuditableObject ToAuditableObject<TData>(this TData obj, AuditableObjectLifecycle? lifecycle = null)
        {
            if(obj is ICanDeepCopy icd)
            {
                obj = (TData)icd.DeepCopy();
            }

            var retVal = new AuditableObject();

            switch (obj)
            {
                case Patient p:
                    retVal.Role = AuditableObjectRole.Patient;
                    retVal.Type = AuditableObjectType.Person;
                    retVal.ObjectId = p.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.PatientNumber;
                    if (ApplicationServiceContext.Current.HostType != SanteDBHostType.Client)
                    {
                        retVal.NameData = String.Join(",", p.LoadProperty(o => o.Identifiers).Select(o => o.ToDisplay()));
                    }
                    break;
                case UserEntity ue:
                    retVal.Role = AuditableObjectRole.User;
                    retVal.Type = AuditableObjectType.Person;
                    retVal.ObjectId = ue.SecurityUserKey.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.UserIdentifier;
                    if (ApplicationServiceContext.Current.HostType != SanteDBHostType.Client)
                    {
                        retVal.NameData = ue.LoadProperty(o => o.SecurityUser)?.UserName;
                    }
                    break;
                case Provider pvd:
                    retVal.Role = AuditableObjectRole.Doctor;
                    retVal.Type = AuditableObjectType.Person;
                    retVal.ObjectId = pvd.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeProvider;
                    if (ApplicationServiceContext.Current.HostType != SanteDBHostType.Client)
                    {
                        retVal.NameData = String.Join(",", pvd.LoadProperty(o => o.Identifiers).Select(o => o.ToDisplay()));
                    }
                    break;
                case Organization org:
                    retVal.Role = AuditableObjectRole.MasterFile;
                    retVal.Type = AuditableObjectType.Organization;
                    retVal.ObjectId = org.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeOrganization;
                    if (ApplicationServiceContext.Current.HostType != SanteDBHostType.Client)
                    {
                        retVal.NameData = String.Join(",", org.LoadProperty(o => o.Identifiers).Select(o => o.ToDisplay()));
                    }
                    break;
                case Place plc:
                    retVal.Role = AuditableObjectRole.Location;
                    retVal.Type = AuditableObjectType.Organization;
                    retVal.ObjectId = plc.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypePlace;
                    if (ApplicationServiceContext.Current.HostType != SanteDBHostType.Client)
                    {
                        retVal.NameData = String.Join(",", plc.LoadProperty(o => o.Identifiers).Select(o => o.ToDisplay()));
                    }
                    break;
                case Entity e:
                    retVal.Role = AuditableObjectRole.MasterFile;
                    retVal.Type = AuditableObjectType.Other;
                    retVal.ObjectId = e.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeEntity;
                    if (ApplicationServiceContext.Current.HostType != SanteDBHostType.Client)
                    {
                        retVal.NameData = String.Join(",", e.LoadProperty(o => o.Identifiers).Select(o => o.ToDisplay()));
                    }
                    break;
                case Act act:
                    var termService = ApplicationServiceContext.Current.GetService<IConceptRepositoryService>();
                    var classification = termService?.GetConceptReferenceTerm(act.ClassConceptKey.GetValueOrDefault(), "http://santedb.org/conceptset/v3-ActClassClinicalDocument", true);
                    retVal.Role = AuditableObjectRole.Report;
                    retVal.Type = AuditableObjectType.Other;
                    retVal.ObjectId = act.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.CustomIdTypeCode = new AuditCode(classification?.Mnemonic ?? "ACT", "http://terminology.hl7.org/CodeSystem/v3-ActClass") { DisplayName = classification?.GetDisplayName("en") };
                    if (ApplicationServiceContext.Current.HostType != SanteDBHostType.Client)
                    {
                        retVal.NameData = String.Join(",", act.LoadProperty(o => o.Identifiers).Select(o => o.ToDisplay()));
                    }
                    if (act.ReasonConceptKey == NullReasonKeys.Masked) // Masked
                    {
                        lifecycle = AuditableObjectLifecycle.Deidentification;
                    }

                    break;
                case SecurityUser su:
                    retVal.Role = AuditableObjectRole.SecurityUser;
                    retVal.Type = AuditableObjectType.SystemObject;
                    retVal.ObjectId = su.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.UserIdentifier;
                    retVal.NameData = su.UserName;
                    break;
                case SecurityRole sr:
                    retVal.Role = AuditableObjectRole.SecurityGroup;
                    retVal.Type = AuditableObjectType.SystemObject;
                    retVal.ObjectId = sr.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.AccountNumber;
                    retVal.NameData = sr.Name;
                    break;
                case SecurityPolicy sp:
                    retVal.Role = AuditableObjectRole.SecurityGranularityDefinition;
                    retVal.Type = AuditableObjectType.SystemObject;
                    retVal.ObjectId = sp.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypePolicy;
                    retVal.NameData = sp.Oid;
                    break;
                case AuditEventData aed:
                    retVal.Role = AuditableObjectRole.Report;
                    retVal.Type = AuditableObjectType.SystemObject;
                    retVal.ObjectId = aed.Key.ToString();
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeAuditObject;
                    retVal.NameData = aed.ToString();
                    break;
                case Guid g:
                    retVal.Role = AuditableObjectRole.MasterFile;
                    retVal.Type = AuditableObjectType.Other;
                    retVal.ObjectId = $"urn:uuid:{g}";
                    retVal.IDTypeCode = AuditableObjectIdType.Uri;
                    break;
                case PolicyDecision pde:
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.Role = AuditableObjectRole.Report;
                    retVal.Type = AuditableObjectType.SystemObject;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypePolicyDecision;
                    retVal.ObjectId = $"urn:oid:{pde.Details?.First().Policy.Oid}";
                    retVal.ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("securable", pde.Securable.ToString()),
                        new ObjectDataExtension("outcome", pde.Outcome.ToString()),
                        new ObjectDataExtension("details", String.Join(",",pde.Details.Select(o=>$"{o.Policy.Oid}={o.Outcome}")))
                    };
                    break;
                case IForeignDataSubmission fds:
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.Role = AuditableObjectRole.JobStream;
                    retVal.Type = AuditableObjectType.SystemObject;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeForeignFile;
                    retVal.ObjectId = fds.Key.ToString();
                    retVal.NameData = fds.Name;
                    break;
                case DataQualityRulesetConfiguration dqrc:
                    retVal.IDTypeCode = AuditableObjectIdType.Custom;
                    retVal.Type = AuditableObjectType.SystemObject;
                    retVal.CustomIdTypeCode = ExtendedAuditCodes.DataQualityConfiguration;
                    retVal.ObjectId = dqrc.Id;
                    retVal.NameData = dqrc.Name;
                    break;
                default:
                    if (obj is IdentifiedData iid)
                    {
                        retVal.IDTypeCode = AuditableObjectIdType.Custom;
                        retVal.Role = AuditableObjectRole.Report;
                        retVal.Type = AuditableObjectType.SystemObject;
                        retVal.CustomIdTypeCode = new AuditCode(iid.Type, "http://santedb.org/model");
                        retVal.ObjectId = iid.Key.ToString();
                        //retVal.NameData = iid.Tag;
                        break;
                    }
                    else
                    {
                        return null;
                    }
            }

            if (!lifecycle.HasValue && obj is IdentifiedData iir)
            {
                switch (iir.BatchOperation)
                {
                    case Model.DataTypes.BatchOperationType.Auto:
                    case Model.DataTypes.BatchOperationType.Ignore:
                        retVal.LifecycleType = AuditableObjectLifecycle.Disclosure;
                        break;
                    case Model.DataTypes.BatchOperationType.Delete:
                        retVal.LifecycleType = AuditableObjectLifecycle.LogicalDeletion;
                        break;
                    case Model.DataTypes.BatchOperationType.Insert:
                    case Model.DataTypes.BatchOperationType.InsertOrUpdate:
                        retVal.LifecycleType = AuditableObjectLifecycle.Creation;
                        break;
                    case Model.DataTypes.BatchOperationType.Update:
                        retVal.LifecycleType = AuditableObjectLifecycle.Amendment;
                        break;
                }
            }
            else
            {
                retVal.LifecycleType = lifecycle;
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
        [Obsolete]
        public static IAuditBuilder ForSensitiveDisclosure(this IAuditBuilder builder, IdentifiedData result, PolicyDecision decision, bool disclosed, params string[] properties)
            => builder
                .WithAction(ActionType.Read)
                .WithOutcome(disclosed ? OutcomeIndicator.Success : OutcomeIndicator.MinorFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType("SecurityAuditEvent-DisclosureOfSensitiveInformation", "SecurityAuditEventDataEvent", displayName: "Sensitive Data Was Disclosed to User")
                .WithRemoteDestination(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithLocalSource()
                .WithPrincipal()
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.PersonalHealthInformation)
                .WithAuditableObjects(
                    result.ToAuditableObject(AuditableObjectLifecycle.Disclosure),
                    decision.ToAuditableObject(AuditableObjectLifecycle.Verification),
                    new AuditableObject
                    {
                        IDTypeCode = AuditableObjectIdType.Custom,
                        CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeMaskedFields,
                        LifecycleType = AuditableObjectLifecycle.Access,
                        NameData = String.Join(";", properties),
                        Type = AuditableObjectType.SystemObject
                    }
                );

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForSecurityDeletionAction(this IAuditBuilder builder, IEnumerable<Object> objects, bool success, IEnumerable<string> changedProperties)
            => builder
                .WithAction(ActionType.Delete)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.SecurityObjectChanged)
                .WithLocalDestination()
                .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithPrincipal()
                .WithAuditableObjects(objects.Select(obj => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode(obj.GetType().Name, "http://santedb.org/model"),
                    ObjectId = ((obj as IAnnotatedResource)?.Key ?? Guid.Empty).ToString(),
                    LifecycleType = AuditableObjectLifecycle.LogicalDeletion,
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject
                }));


        /// <summary>
        /// Audit application start
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForApplicationStart(this IAuditBuilder builder)
            => builder
                .WithAction(ActionType.Execute)
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)

                .WithOutcome(OutcomeIndicator.Success)
                .WithEventIdentifier(EventIdentifierType.ApplicationActivity)
                .WithEventType(EventTypeCodes.ApplicationStart)
                .WithLocalSource()
            ;

        /// <summary>
        /// Audit application stop
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForApplicationStop(this IAuditBuilder builder)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.Success)
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)
                .WithEventIdentifier(EventIdentifierType.ApplicationActivity)
                .WithEventType(EventTypeCodes.ApplicationStop)
                .WithLocalSource()
            ;

        /// <summary>
        /// Audit a login of a user principal
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForUserLogin(this IAuditBuilder builder, IPrincipal principal, bool successfulLogin = true)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(successfulLogin, falseOutcome: OutcomeIndicator.SeriousFail)
                .WithEventIdentifier(EventIdentifierType.UserAuthentication)
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)
                .WithEventType(EventTypeCodes.Login)
                .WithLocalDestination()
            .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithPrincipal(principal)
                .If(successfulLogin, b2 => b2.WithAuditableObjects(new AuditableObject
                {
                    IDTypeCode = AuditableObjectIdType.UserIdentifier,
                    ObjectId = principal?.Identity?.Name,
                    LifecycleType = AuditableObjectLifecycle.NotSet,
                    Role = AuditableObjectRole.SecurityUser,
                    Type = AuditableObjectType.SystemObject
                })
                );

        /// <summary>
        /// Audit a logout of a user principal
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForUserLogout(this IAuditBuilder builder, IPrincipal principal)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.Success)
                .WithEventIdentifier(EventIdentifierType.UserAuthentication)
                .WithEventType(EventTypeCodes.Logout)
                .WithLocalDestination()
            .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithPrincipal(principal);

        /// <summary>
        /// Audit a network request failure
        /// </summary>
        public static IAuditBuilder ForNetworkRequestFailure(this IAuditBuilder builder, Exception ex, Uri url, NameValueCollection requestHeaders, NameValueCollection responseHeaders)
            => ForNetworkRequestFailure(builder, ex, url, requestHeaders.AllKeys.ToDictionary(o => o, o => requestHeaders[o]), responseHeaders?.AllKeys.ToDictionary(o => o, o => responseHeaders[o]));

        /// <summary>
        /// Audit a network request failure
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForNetworkRequestFailure(this IAuditBuilder builder, Exception ex, Uri url, IDictionary<String, String> requestHeaders, IDictionary<String, String> responseHeaders)
        {
            builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.MinorFail)
                .WithEventIdentifier(EventIdentifierType.NetworkActivity)
                .WithEventType(EventTypeCodes.NetworkActivity)
                .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithLocalDestination()
                .WithPrincipal()
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
            {
                builder.WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/policy/{polvex.PolicyId}",
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    NameData = ex.Message
                });
            }
            else
            {
                builder.WithAuditableObjects(new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Uri,
                    LifecycleType = AuditableObjectLifecycle.Report,
                    ObjectId = $"http://santedb.org/error/{ex.GetType().Name}",
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject,
                    NameData = ex.Message
                });
            }

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
        [Obsolete]
        public static IAuditBuilder ForSessionStart(this IAuditBuilder builder, ISession session, IPrincipal principal, bool success)
        {
            builder.WithAction(ActionType.Execute)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)
                .WithEventIdentifier(EventIdentifierType.UserAuthentication)
                .WithEventType(EventTypeCodes.SessionStarted)
                .WithRemoteDestination(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)
                .WithLocalSource()
                .WithPrincipal(principal);

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
                    CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeSession,
                    LifecycleType = AuditableObjectLifecycle.Creation,
                    ObjectData = new List<ObjectDataExtension>()
                    {
                        new ObjectDataExtension("method", principal.Identity?.AuthenticationType),
                        deviceIdentity != cprincipal.Identity && applicationIdentity != cprincipal.Identity ? new ObjectDataExtension("userIdentity", principal.Identity.Name) : null,
                        deviceIdentity != null ? new ObjectDataExtension("deviceIdentity", deviceIdentity?.Name) : null,
                        applicationIdentity != null ? new ObjectDataExtension("applicationIdentity", applicationIdentity?.Name) : null,
                        new ObjectDataExtension("scope", String.Join("; ", policies ?? new String[] { "*" }))
                    }.OfType<ObjectDataExtension>().Union(session.Claims.Where(c=>c.Type != SanteDBClaimTypes.SanteDBScopeClaim).Select(o => new ObjectDataExtension(o.Type, o.Value) )).ToList()
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
        [Obsolete]
        public static IAuditBuilder ForSessionStop(this IAuditBuilder builder, ISession session, IPrincipal principal, bool success)
        {
            if (null == principal)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            builder.WithAction(ActionType.Execute)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)
                .WithEventIdentifier(EventIdentifierType.UserAuthentication)
                .WithEventType(EventTypeCodes.SessionStopped)
                .WithLocalDestination()
                .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithPrincipal(principal);


            // Audit the actual session that is abandoned.
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
                    CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeSession,
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
        [Obsolete]
        public static void ForDataExport(this IAuditBuilder builder, params object[] exportedData)
            => builder
                .WithAction(ActionType.Execute)
                .WithOutcome(OutcomeIndicator.Success)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.Export)
                .WithRemoteDestination(RemoteEndpointUtil.Current.GetRemoteClient())
            .WithLocalSource()
                .WithPrincipal()
                .WithAuditableObjects(exportedData.Where(o => o != null).Select(o => o.ToAuditableObject(AuditableObjectLifecycle.Export)));


        /// <summary>
        /// Audit masking of a particular object
        /// </summary>
        /// <param name="targetOfMasking">The object which was masked</param>
        /// <param name="wasRemoved">True if the object was removed instead of masked</param>
        /// <param name="maskedObject">The object that was masked</param>
        /// <param name="decision">The decision which caused the masking to occur</param>
        /// <param name="builder">The audit builder on which the information should be appended</param>
        [Obsolete]
        public static IAuditBuilder ForMasking<TModel>(this IAuditBuilder builder, TModel targetOfMasking, PolicyDecision decision, bool wasRemoved, IdentifiedData maskedObject)
            => ForEventDataAction(
                builder,
                ExtendedAuditCodes.EventTypeMasking,
                ActionType.Execute,
                AuditableObjectLifecycle.Deidentification,
                EventIdentifierType.ApplicationActivity,
                OutcomeIndicator.Success,
                null,
                decision,
                targetOfMasking).WithSensitivity(ResourceSensitivityClassification.PersonalHealthInformation);

        /// <summary>
        /// Audit the creation of an object
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForCreate<TData>(this IAuditBuilder builder, OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
            => ForEventDataAction(
                builder,
                ExtendedAuditCodes.EventTypeCreate,
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
        [Obsolete]
        public static IAuditBuilder ForUpdate<TData>(this IAuditBuilder builder, OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
            => ForEventDataAction(
                builder,
                ExtendedAuditCodes.EventTypeUpdate,
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
        [Obsolete]
        public static IAuditBuilder ForDelete<TData>(this IAuditBuilder builder, OutcomeIndicator outcome, string queryPerformed, params TData[] resourceData)
            => ForEventDataAction(
                builder,
                ExtendedAuditCodes.EventTypeDelete,
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
        [Obsolete]
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
        [Obsolete]
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
        [Obsolete]
        public static IAuditBuilder ForSecurityCreationAction(this IAuditBuilder builder, IEnumerable<object> objects, bool success, IEnumerable<string> changedProperties)
            => builder
                .WithAction(ActionType.Create)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.SecurityObjectChanged)
                .WithLocalDestination()
            .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithPrincipal()
                .WithAuditableObjects(objects.Select(obj => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode(obj.GetType().Name, "http://santedb.org/model"),
                    ObjectId = ((obj as IAnnotatedResource)?.Key ?? Guid.Empty).ToString(),
                    LifecycleType = AuditableObjectLifecycle.Creation,
                    Role = AuditableObjectRole.SecurityResource,
                    Type = AuditableObjectType.SystemObject
                }));

        /// <summary>
        /// Create a security attribute action audit
        /// </summary>
        [Obsolete]
        public static IAuditBuilder ForSecurityAttributeAction(this IAuditBuilder builder, IEnumerable<object> objects, bool success, params string[] changedProperties)
            => builder
                .WithAction(ActionType.Update)
                .WithOutcome(success, falseOutcome: OutcomeIndicator.EpicFail)
                .WithEventIdentifier(EventIdentifierType.SecurityAlert)
                .WithEventType(EventTypeCodes.SecurityAttributesChanged)
                .WithLocalDestination()
            .WithRemoteSource(RemoteEndpointUtil.Current.GetRemoteClient())
                .WithPrincipal()
                .WithAuditableObjects(objects.Select(obj => new AuditableObject()
                {
                    IDTypeCode = AuditableObjectIdType.Custom,
                    CustomIdTypeCode = new AuditCode(obj.GetType().Name, "http://santedb.org/model"),
                    ObjectId = ((obj as IAnnotatedResource)?.Key ?? Guid.Empty).ToString(),
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
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)
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
                        CustomIdTypeCode = ExtendedAuditCodes.CustomIdTypeSession,
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
        /// <param name="service">The service to use to build the audit</param>
        /// <param name="timeStamp">The timestamp of the event</param>
        /// <param name="actionCode">The action type which classifies the audit action</param>
        /// <param name="outcome">The outcome of the audit</param>
        /// <param name="eventIdentifier">The event identification</param>
        /// <param name="eventTypeCode">The event type</param>
        /// <returns>The constructed audit builder</returns>
        public static IAuditBuilder Audit(this IAuditService service, DateTimeOffset timeStamp, ActionType actionCode, OutcomeIndicator outcome, EventIdentifierType eventIdentifier, AuditCode eventTypeCode)
            => service?.Audit()
                .WithTimestamp(timeStamp)
                .WithAction(actionCode)
                .WithOutcome(outcome)
                .WithEventIdentifier(eventIdentifier)
                .WithEventType(eventTypeCode);

        /// <summary>
        /// Creates a new <see cref="IAuditBuilder" /> instance tied to this service for dispatch.
        /// </summary>
        /// <param name="service">The audit service to use for creating the audit</param>
        /// <param name="timeStamp">The timestamp of the event</param>
        /// <param name="actionCode">The action code which was executed</param>
        /// <param name="outcome">The outcome of the audit</param>
        /// <param name="eventIdentifier">The event identification</param>
        /// <param name="eventTypeCode">The type code</param>
        /// <returns>The audit builder for building the audit</returns>
        public static IAuditBuilder Audit(this IAuditService service, DateTimeOffset timeStamp, ActionType actionCode, OutcomeIndicator outcome, EventIdentifierType eventIdentifier, EventTypeCodes eventTypeCode)
            => service.Audit()
                .WithTimestamp(timeStamp)
                .WithAction(actionCode)
                .WithOutcome(outcome)
                .WithEventIdentifier(eventIdentifier)
                .WithEventType(eventTypeCode);
    }
}
#pragma warning restore