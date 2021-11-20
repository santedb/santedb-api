/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */

using SanteDB.Core.Auditing;
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
    /// Event type codes
    /// </summary>
#pragma warning disable CS1591

#pragma warning restore CS1591

    /// <summary>
    /// Security utility
    /// </summary>
    public static class AuditExtensions
    {
        /// <summary>
        /// Append the outcome indicator to the audit
        /// </summary>
        public static AuditData WithOutcome(this AuditData me, OutcomeIndicator outcome)
        {
            me.Outcome = outcome;
            return me;
        }

        /// <summary>
        /// Set the action on the audit
        /// </summary>
        public static AuditData Action(this AuditData me, ActionType action)
        {
            me.ActionCode = action;
            return me;
        }

        /// <summary>
        /// With event type
        /// </summary>
        public static AuditData WithEventType(this AuditData me, String eventTypeCode, String eventTypeCodeSystem = "http://santedb.org/conceptset/SecurityAuditCode")
        {
            me.EventTypeCode = new AuditCode(eventTypeCode, eventTypeCodeSystem);
            return me;
        }

        /// <summary>
        /// With an enum set event type
        /// </summary>
        public static AuditData WithEventType(this AuditData me, EventTypeCodes typeCode)
        {
            var typeCodeWire = typeof(EventTypeCodes).GetRuntimeField(typeCode.ToString()).GetCustomAttribute<XmlEnumAttribute>();
            me.EventTypeCode = new AuditCode(typeCodeWire.Name, "http://santedb.org/conceptset/SecurityAuditCode");
            return me;
        }

        /// <summary>
        /// With the specified action code
        /// </summary>
        public static AuditData WithAction(this AuditData me, ActionType action)
        {
            me.ActionCode = action;
            return me;
        }

        /// <summary>
        /// Add timestamp
        /// </summary>
        public static AuditData WithTimestamp(this AuditData me, DateTime? timestamp = null)
        {
            me.Timestamp = timestamp ?? DateTime.Now;
            return me;
        }

        /// <summary>
        /// Event identiifer set
        /// </summary>
        public static AuditData WithEventIdentifier(this AuditData me, EventIdentifierType identifier)
        {
            me.EventIdentifier = identifier;
            return me;
        }

        /// <summary>
        /// Add a query performed data element
        /// </summary>
        public static AuditData WithQueryPerformed(this AuditData me, String queryPerformed)
        {
            me.AuditableObjects.Add(new AuditableObject()
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
        public static AuditData WithPolicyAuthorization(this AuditData me, PolicyDecision policy)
        {
            me.AuditableObjects.AddRange(policy.Details.Select(o => new AuditableObject()
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
        public static AuditData WithLocalDevice(this AuditData me)
        {
            me.Actors.Add(new AuditActorData()
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
        public static AuditData WithUser(this AuditData me, IPrincipal principal = null)
        {
            // Use all remote endpoint providers to find the current request
            principal = principal ?? AuthenticationContext.Current.Principal;

            if (principal is IClaimsPrincipal cp)
            {
                foreach (var identity in cp.Identities)
                {
                    if (identity is IDeviceIdentity && identity is IClaimsIdentity did)
                    {
                        me.Actors.Add(new AuditActorData()
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
                        me.Actors.Add(new AuditActorData()
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
                        me.Actors.Add(new AuditActorData()
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
                me.Actors.Add(actor);
            }
            return me;
        }

        /// <summary>
        /// With object of patient
        /// </summary>
        public static AuditData WithPatient(this AuditData me, Patient patient, AuditableObjectLifecycle lifecycle)
        {
            me.AuditableObjects.Add(new AuditableObject()
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
        public static AuditData WithPerson(this AuditData me, Person person, AuditableObjectLifecycle lifecycle)
        {
            me.AuditableObjects.Add(new AuditableObject()
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
        public static AuditData WithProvider(this AuditData me, Provider provider, AuditableObjectLifecycle lifecycle)
        {
            me.AuditableObjects.Add(new AuditableObject()
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
        public static AuditData WithAct(this AuditData me, Act act, AuditableObjectLifecycle lifecycle)
        {
            me.AuditableObjects.Add(new AuditableObject()
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
        public static AuditData WithHttpInformation(this AuditData me, HttpListenerRequest request)
        {
            me.AuditableObjects.Add(new AuditableObject()
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
        public static AuditData WithSystemObjects(this AuditData me, AuditableObjectRole role, AuditableObjectLifecycle lifecycle, params Uri[] objectIds)
        {
            me.AuditableObjects.AddRange(objectIds.Select(o => new AuditableObject()
            {
                ObjectId = o.ToString(),
                IDTypeCode = AuditableObjectIdType.Uri,
                Type = AuditableObjectType.SystemObject,
                Role = role,
                LifecycleType = lifecycle
            }));
            return me;
        }
    }
}