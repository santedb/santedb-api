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
 * Date: 2024-6-21
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Event;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Extensions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Type redirect
    /// </summary>
    [Obsolete("Use SimpleDecisionSupportService")]
    public class SimpleCarePlanService : SimpleDecisionSupportService
    {
        public SimpleCarePlanService(ICdssLibraryRepository protocolRepository, IRepositoryService<ActParticipation> actParticipationRepository, ICarePathwayDefinitionRepositoryService carePathwayDefinitionRepositoryService) : base(protocolRepository, actParticipationRepository, carePathwayDefinitionRepositoryService)
        {
        }
    }

    /// <summary>
    /// Represents a care plan service that can bundle protocol acts together based on their start/stop times
    /// </summary>
    /// <remarks>
    /// <para>This implementation of the care plan service is capable of calling <see cref="ICdssProtocol"/> instances
    /// registered from the clinical protocol manager to construct <see cref="Act"/> instances representing the proposed
    /// actions to take for the patient. The care planner is also capable of simple interval hull functions to group 
    /// these acts together into <see cref="PatientEncounter"/> instances based on safe time for grouping.</para>
    /// </remarks>
    [ServiceProvider("Default Care Planning Service")]
    public class SimpleDecisionSupportService : IDecisionSupportService
    {

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Default Care Planning Service";

        /// <summary>
        /// Represents a parameter dictionary
        /// </summary>
        private class ParameterDictionary : Dictionary<String, Object>
        {


            /// <summary>
            /// Create a new parameter dictionary
            /// </summary>
            public ParameterDictionary(IDictionary<String, Object> other) : base(other ?? new Dictionary<String, Object>())
            {
            }

            /// <summary>
            /// Add new item key
            /// </summary>
            public new void Add(String key, Object value)
            {
                if (value == null)
                {
                    // adding null value is pointless...
                    return;
                }
                base.Add(key, value);
            }

            /// <summary>
            /// Remove key
            /// </summary>
            /// <param name="key"></param>
            public new void Remove(String key)
            {
                if (!ContainsKey(key))
                {
                    // nothing to do
                    return;
                }
                base.Remove(key);
            }

            /// <summary>
            /// Indexer
            /// </summary>
            public new Object this[String key]
            {
                get
                {
                    Object value;
                    return TryGetValue(key, out value) ? value : null;
                }
                set
                {
                    if (value == null)
                    {
                        // setting value null is same as removing it
                        Remove(key);
                    }
                    else
                    {
                        base[key] = value;
                    }
                }
            }
        }

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(SimpleDecisionSupportService));
        private readonly ICdssLibraryRepository m_cdssLibraryRepository;
        private readonly IRepositoryService<ActParticipation> m_actParticipationRepository;
        private readonly ICarePathwayDefinitionRepositoryService m_carePathwayRepository;

        /// <summary>
        /// Constructs the aggregate care planner
        /// </summary>
        public SimpleDecisionSupportService(
            ICdssLibraryRepository protocolRepository, 
            IRepositoryService<ActParticipation> actParticipationRepository,
            ICarePathwayDefinitionRepositoryService carePathwayDefinitionRepositoryService)
        {
            this.m_cdssLibraryRepository = protocolRepository;
            this.m_actParticipationRepository = actParticipationRepository;
            this.m_carePathwayRepository = carePathwayDefinitionRepositoryService;
        }

        /// <summary>
        /// Create a care plan for the specified patient
        /// </summary>
        public CarePlan CreateCarePlan(Patient p)
        {
            return this.CreateCarePlan(p, false);
        }

        /// <inheritdoc/>
        public CarePlan CreateCarePlan(Patient p, bool asEncounters)
        {
            return this.CreateCarePlan(p, asEncounters, null);
        }

        /// <inheritdoc/>
        public CarePlan CreateCarePlan(Patient target, bool asEncounters, IDictionary<String, Object> parameters, params ICdssLibrary[] libraries)
        {
            if (target == null)
            {
                return null;
            }

            try
            {

                using (AuthenticationContext.EnterSystemContext())
                {
                    // Sometimes the patient will have participations which the protocol requires - however these are 
                    // not directly loaded from the database - so let's load them
                    var patientCopy = target.Clone() as Patient; // don't mess up the original
                    if (patientCopy.Key.HasValue && patientCopy.Participations.IsNullOrEmpty())
                    {
                        patientCopy.Participations = this.m_actParticipationRepository.Find(o => o.ParticipationRoleKey == ActParticipationKeys.RecordTarget && o.PlayerEntityKey == patientCopy.Key && o.Act.MoodConceptKey == ActMoodKeys.Eventoccurrence)
                            .ToList();
                    }
                    else
                    {
                        patientCopy.Participations = patientCopy.Participations?.ToList() ?? patientCopy.GetParticipations()?.ToList();
                    }

                    // We only want events which DID occur to be considered in the CDSS
                    patientCopy.Participations?.RemoveAll(o => o.LoadProperty(a=>a.Act).MoodConceptKey != ActMoodKeys.Eventoccurrence || !StatusKeys.ActiveStates.Contains(o.Act.StatusConceptKey.Value));

                    patientCopy.Participations.OfType<ActParticipation>()
                            .AsParallel()
                            .ForAll(p =>
                            {
                                using (AuthenticationContext.EnterSystemContext())
                                {
                                    p.LoadProperty(o => o.ParticipationRole);
                                    p.LoadProperty(o => o.Act);
                                    p.Act.LoadProperty(o => o.TypeConcept);
                                    p.Act.LoadProperty(o => o.MoodConcept);
                                    p.Act.LoadProperty(o => o.Participations).Where(r => r.ParticipationRoleKey != ActParticipationKeys.RecordTarget).ForEach(t =>
                                    {
                                        t.LoadProperty(o => o.ParticipationRole);
                                        t.LoadProperty(o => o.PlayerEntity).LoadProperty(o => o.TypeConcept);
                                    });
                                    p.PlayerEntity = patientCopy;
                                }
                            });

                    // No libraries spec = all libraries
                    if(libraries.Length == 0)
                    {
                        libraries = this.m_cdssLibraryRepository.Find(o => true).ToArray();
                    }

                    // Initialize
                    var parmDict = new ParameterDictionary(parameters);
                    parmDict.Add(CdssParameterNames.PROTOCOL_IDS, libraries.Distinct());

                    var detectedIssueList = new ConcurrentBag<DetectedIssue>();
                    var appliedProtocols = new ConcurrentBag<ICdssProtocol>();
                    CarePathwayDefinition pathwayDef = null;
                    if(parmDict.TryGetValue(CdssParameterNames.PATHWAY_SCOPE, out var pathway) && !String.IsNullOrEmpty(pathway?.ToString()))
                    {
                        if (Guid.TryParse(pathway.ToString(), out var pathwayUuid))
                        {
                            pathwayDef = this.m_carePathwayRepository.Get(pathwayUuid);
                        }
                        else
                        {
                            pathwayDef = this.m_carePathwayRepository.GetCarepathDefinition(pathway.ToString());
                        }

                        if(pathwayDef == null)
                        {
                            throw new KeyNotFoundException(String.Format(ErrorMessages.CARE_PATHWAY_NOT_FOUND, pathway.ToString()));
                        }
                    }
                    _ = parmDict.TryGetValue(CdssParameterNames.ENCOUNTER_SCOPE, out var encounterType);

   
                    // Compute the protocols
                    var protocolOutput = libraries
                        .AsParallel()
                        .WithDegreeOfParallelism(2)
                        .SelectMany(library => library.GetProtocols(patientCopy, parmDict, pathway?.ToString(), pathwayDef?.LoadProperty(o=>o.Template)?.Mnemonic, encounterType?.ToString()))
                        .SelectMany(proto =>
                        {
                            try
                            {
                                using (AuthenticationContext.EnterSystemContext())
                                {
                                    var retVal = proto.ComputeProposals(patientCopy, parmDict);
                                    appliedProtocols.Add(proto);
                                    return retVal;
                                }
                            }
                            catch (Exception e)
                            {
                                detectedIssueList.Add(new DetectedIssue(DetectedIssuePriorityType.Error, e.HResult.ToString(), e.Message, DetectedIssueKeys.OtherIssue));
                                return new Act[0];
                            }
                        })
                        .Select(o=>
                        {
                            if(o is Act a && !a.LoadProperty(p => p.Participations).Any(p=>p.ParticipationRoleKey == ActParticipationKeys.RecordTarget))
                            {
                                a.Participations.Add(new ActParticipation(ActParticipationKeys.RecordTarget, target.Key));
                                a.StartTime = a.StartTime?.EnsureWeekday();
                                a.StopTime = a.StopTime?.EnsureWeekday();
                                a.ActTime = a.ActTime?.EnsureWeekday();
                            }
                            return o;
                        })
                        .ToList();

                    var protocolActs = protocolOutput.OfType<Act>().OrderBy(o => o.StartTime ?? o.ActTime).ToList();

                    // Tag back entry?
                    if(parmDict.TryGetValue(CdssParameterNames.INCLUDE_BACKENTRY, out var backEntryRaw) &&
                           (backEntryRaw is bool backEntry || bool.TryParse(backEntryRaw.ToString(), out backEntry)))
                    {
                        protocolActs.Where(act => act.StopTime < DateTimeOffset.Now).ForEach(a => a.AddTag(SystemTagNames.BackEntry, Boolean.TrueString));
                    }
                    // Filter
                    if (parmDict.TryGetValue(CdssParameterNames.PERIOD_OF_EVENTS, out var dateRaw) && 
                        (dateRaw is DateTime periodOutput || DateTime.TryParse(dateRaw?.ToString(), out periodOutput)))
                    {
                        protocolActs = protocolActs.Where(act =>
                        {
                            return act.TryGetTag(SystemTagNames.BackEntry, out var tag) && tag.Value == Boolean.TrueString ||
                                (act.StartTime.HasValue && act.StartTime <= periodOutput.Date || !act.StartTime.HasValue) &&
                                ((act.StopTime.HasValue && act.StopTime >= periodOutput.Date || !act.StopTime.HasValue) ||
                                (act.ActTime.Value.Year == periodOutput.Year && act.ActTime.Value.EnsureWeekday().IsoWeek() == periodOutput.IsoWeek()));
                        }).ToList();
                    }
                    if(parmDict.TryGetValue(CdssParameterNames.FIRST_APPLICAPLE, out var firstApplicableRaw) &&
                        (firstApplicableRaw is bool firstApplicable || Boolean.TryParse(firstApplicableRaw.ToString(), out firstApplicable)) && firstApplicable)
                    {
                        protocolActs = protocolActs.GroupBy(o => $"{o.TypeConceptKey}{o.Protocols.First().ProtocolKey}").Select(o => o.First()).ToList();
                    }

                    protocolOutput.OfType<DetectedIssue>().ForEach(o => detectedIssueList.Add(o));
                    // Group these as appointments 
                    if (asEncounters)
                    {
                        List<PatientEncounter> encounters = new List<PatientEncounter>();
                        Queue<Act> protocolStack = new Queue<Act>(protocolActs.OrderBy(o => o.StartTime ?? o.ActTime).ThenBy(o => (o.StopTime ?? o.ActTime?.AddDays(7)) - (o.StartTime ?? o.ActTime)));
                       
                        while(protocolStack.Any())
                        {
                            var act = protocolStack.Dequeue();

                            // First we want to find a candidate which has the same period properties
                            var periodStart = act.StartTime <= DateTimeOffset.Now ? act.StartTime.GreaterOf(act.ActTime) : act.StartTime;
                            var periodEnd = act.StopTime ?? act.ActTime.Value.AddDays(7);

                            // Find a candidate based on the start and end time
                            var candidate = encounters.Find(c =>
                                periodStart <= (c.StopTime ?? DateTimeOffset.MaxValue) &&
                                periodEnd >= c.StartTime);

                            if(candidate != null &&
                                (candidate?.StopTime == null && 
                                Math.Abs(candidate.StartTime.GreaterOf(candidate.ActTime)?.Subtract(candidate.StartTime.Value).TotalDays ?? 0) > 28 ||
                                (candidate.ActTime.GreaterOf(candidate.StartTime).Value.Month != periodStart.Value.Month))) // Don't allow multi-month suggestions
                            {
                                candidate.StopTime = candidate.Relationships.Select(o => o.TargetAct.StartTime.GreaterOf(o.TargetAct.ActTime)).Max()?.ClosestDay(DayOfWeek.Saturday);
                                candidate = null;
                            }

                            if(candidate == null)
                            {
                                candidate = this.CreateEncounter(act, patientCopy, pathwayDef?.TemplateKey);
                                encounters.Add(candidate);
                                protocolActs.Add(candidate);
                            }
                            else
                            {
                                // Found the candidate - Does the stop time of this candidate act shorter than the current
                                candidate.StopTime = (candidate.StopTime ?? candidate.StartTime?.AddDays(10)).LesserOf(periodEnd);
                                candidate.StartTime = candidate.StartTime.GreaterOf(periodStart);
                            }
                            candidate.LoadProperty(o => o.Relationships).Add(new ActRelationship(ActRelationshipTypeKeys.HasComponent, act));
                            // Remove so we don't have duplicates
                            protocolActs.Remove(act);
                        }

                    }

                    return new CarePlan(patientCopy, protocolActs.Select(o=>
                    {
                        o.ActTime = o.ActTime?.EnsureWeekday();
                        o.StartTime = o.StartTime?.EnsureWeekday();
                        o.StopTime = o.StopTime?.EnsureWeekday();
                        return o;
                    }).OrderBy(o=>o.ActTime).ToList())
                    {
                        MoodConceptKey = ActMoodKeys.Propose,
                        ActTime = DateTimeOffset.Now,
                        StartTime = DateTimeOffset.Now,
                        StatusConceptKey = StatusKeys.Active,
                        StopTime = protocolActs.Select(o => o.StopTime ?? o.ActTime).OrderByDescending(o => o).FirstOrDefault(),
                        CreatedByKey = Guid.Parse(Security.AuthenticationContext.SystemApplicationSid),
                        CarePathwayKey = pathwayDef?.Key,
                        Protocols = appliedProtocols.Select(o => new ActProtocol()
                        {
                            ProtocolKey = o.Uuid,
                            Version = o.Version,
                            Protocol = new Protocol()
                            {
                                Key = o.Uuid,
                                Name = o.Name,
                                Oid = o.Oid
                            }
                        }).ToList(),
                        Extensions = new List<Model.DataTypes.ActExtension>()
                        {
                            new Model.DataTypes.ActExtension(ExtensionTypeKeys.PatientSafetyConcernIssueExtension, typeof(DictionaryExtensionHandler), detectedIssueList.ToList())
                        }
                    };

                }
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error creating care plan: {0}", e);
                throw new CdssException(libraries, target, e);
            }
        }

        /// <inheritdoc/>
        private PatientEncounter CreateEncounter(Act act, Patient recordTarget, Guid? templateKey)
        {
            var retVal = new PatientEncounter()
            {
                Participations = new List<ActParticipation>()
                {
                    new ActParticipation(ActParticipationKeys.RecordTarget, recordTarget.Key)
                },
                TemplateKey = templateKey,
                ActTime = act.ActTime?.EnsureWeekday(),
                StartTime = (act.StartTime <= DateTimeOffset.Now ? act.StartTime.GreaterOf(act.ActTime) : act.StartTime)?.EnsureWeekday(),
                StopTime = act.StopTime?.EnsureWeekday(),
                MoodConceptKey = ActMoodKeys.Propose,
                Key = Guid.NewGuid()
            };
            recordTarget.Participations.Add(new ActParticipation()
            {
                ParticipationRoleKey = ActParticipationKeys.RecordTarget,
                Act = retVal
            });
            return retVal;
        }


        /// <inheritdoc/>
        public IEnumerable<ICdssResult> Analyze(IdentifiedData collectedData, IDictionary<String, Object> parameters, params ICdssLibrary[] librariesToApply)
        {
            if(librariesToApply.Length == 0 )
            {
                librariesToApply = this.m_cdssLibraryRepository.Find(o => true).ToArray();
            }

            foreach (var lib in librariesToApply)
            {
                foreach (var iss in lib.Analyze(collectedData, parameters))
                {
                    yield return iss;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ICdssResult> AnalyzeGlobal(IdentifiedData collectedData, IDictionary<String, Object> parameters)
        {
            return this.Analyze(collectedData, parameters, this.m_cdssLibraryRepository.Find(o => true).ToArray());
        }
    }
}