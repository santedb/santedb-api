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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.EntityLoader;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Core.Protocol
{
    /// <summary>
    /// Represents a care plan service that can bundle protocol acts together based on their start/stop times
    /// </summary>
    /// <remarks>
    /// <para>This implementation of the care plan service is capable of calling <see cref="IClinicalProtocol"/> instances
    /// registered from the clinical protocol manager to construct <see cref="Act"/> instances representing the proposed
    /// actions to take for the patient. The care planner is also capable of simple interval hull functions to group 
    /// these acts together into <see cref="PatientEncounter"/> instances based on safe time for grouping.</para>
    /// </remarks>
    [ServiceProvider("Default Care Planning Service")]
    public class SimpleCarePlanService : ICarePlanService
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Default Care Planning Service";

        /// <summary>
        /// True if the view model initializer for the care plans should be ignored
        /// </summary>
        public bool IgnoreViewModelInitializer { get; set; }

        /// <summary>
        /// Represents a parameter dictionary
        /// </summary>
        public class ParameterDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : class
        {
            /// <summary>
            /// Add new item key
            /// </summary>
            public new void Add(TKey key, TValue value)
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
            public new void Remove(TKey key)
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
            public new TValue this[TKey key]
            {
                get
                {
                    TValue value;
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
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(SimpleCarePlanService));
        private readonly IClinicalProtocolRepositoryService m_protocolRepository;

        // Care plan loading promise dictionary (prevents double-loading of patients)
        private Dictionary<Guid, Patient> m_patientPromise = new Dictionary<Guid, Patient>();

        /// <summary>
        /// Constructs the aggregate care planner
        /// </summary>
        public SimpleCarePlanService(IClinicalProtocolRepositoryService protocolRepository)
        {
            this.m_protocolRepository = protocolRepository;
        }

        /// <summary>
        /// Create a care plan for the specified patient
        /// </summary>
        public CarePlan CreateCarePlan(Patient p)
        {
            return this.CreateCarePlan(p, false, null);
        }

        /// <summary>
        /// Create a care plan
        /// </summary>
        /// <param name="p">The patient to calculate the care plan for</param>
        /// <param name="asEncounters">True if the data should be grouped as an encounter</param>
        public CarePlan CreateCarePlan(Patient p, bool asEncounters)
        {
            return this.CreateCarePlan(p, asEncounters, null, this.m_protocolRepository.FindProtocol().ToArray());
        }

        /// <summary>
        /// Create a care plan
        /// </summary>
        public CarePlan CreateCarePlan(Patient p, bool asEncounters, IDictionary<String, Object> parameters)
        {
            return this.CreateCarePlan(p, asEncounters, parameters, this.m_protocolRepository.FindProtocol().ToArray());
        }

        /// <summary>
        /// Create a care plan with the specified protocols only
        /// </summary>
        public CarePlan CreateCarePlan(Patient patient, bool asEncounters, IDictionary<String, Object> parameters, params IClinicalProtocol[] protocols)
        {
            if (patient == null)
            {
                return null;
            }

            try
            {
                var parmDict = new ParameterDictionary<String, Object>();
                if (parameters != null)
                {
                    foreach (var itm in parameters)
                    {
                        parmDict.Add(itm.Key, itm.Value);
                    }
                }

                // Allow each protocol to initialize itself
                var execProtocols = protocols.OrderBy(o => o.Name).Distinct().ToList();

                Patient currentProcessing = null;
                bool isCurrentProcessing = false;
                if (patient.Key.HasValue)
                {
                    isCurrentProcessing = this.m_patientPromise.TryGetValue(patient.Key.Value, out currentProcessing);
                }

                if (patient.Key.HasValue && !isCurrentProcessing)
                {
                    lock (this.m_patientPromise)
                    {
                        if (!this.m_patientPromise.TryGetValue(patient.Key.Value, out currentProcessing))
                        {
                            currentProcessing = patient.DeepCopy() as Patient;

                            // Are the participations of the patient null?
                            if (patient.LoadProperty(o => o.Participations).IsNullOrEmpty() && patient.VersionKey.HasValue)
                            {
                                patient.Participations = EntitySource.Current.Provider.Query<Act>(o => o.Participations.Where(g => g.ParticipationRole.Mnemonic == "RecordTarget").Any(g => g.PlayerEntityKey == currentProcessing.Key) &&
                                    StatusKeys.ActiveStates.Contains(o.StatusConceptKey.Value)).OfType<Act>()
                                    .Select(a =>
                                    new ActParticipation()
                                    {
                                        Act = a,
                                        ParticipationRole = new Concept() { Mnemonic = "RecordTarget" },
                                        PlayerEntity = currentProcessing
                                    }).ToList();

                                //EntitySource.Current.Provider.Query<SubstanceAdministration>(o => o.Participations.Where(g => g.ParticipationRole.Mnemonic == "RecordTarget").Any(g => g.PlayerEntityKey == currentProcessing.Key)).OfType<Act>()
                                //    .Union(EntitySource.Current.Provider.Query<QuantityObservation>(o => o.Participations.Where(g => g.ParticipationRole.Mnemonic == "RecordTarget").Any(g => g.PlayerEntityKey == currentProcessing.Key))).OfType<Act>()
                                //    .Union(EntitySource.Current.Provider.Query<CodedObservation>(o => o.Participations.Where(g => g.ParticipationRole.Mnemonic == "RecordTarget").Any(g => g.PlayerEntityKey == currentProcessing.Key))).OfType<Act>()
                                //    .Union(EntitySource.Current.Provider.Query<TextObservation>(o => o.Participations.Where(g => g.ParticipationRole.Mnemonic == "RecordTarget").Any(g => g.PlayerEntityKey == currentProcessing.Key))).OfType<Act>()
                                //    .Union(EntitySource.Current.Provider.Query<PatientEncounter>(o => o.Participations.Where(g => g.ParticipationRole.Mnemonic == "RecordTarget").Any(g => g.PlayerEntityKey == currentProcessing.Key))).OfType<Act>()

                                (ApplicationServiceContext.Current.GetService(typeof(IDataCachingService)) as IDataCachingService)?.Add(patient);
                            }
                            currentProcessing.Participations = new List<ActParticipation>(patient.LoadProperty(o => o.Participations));

                            // The record target here is also a record target for any /relationships
                            // TODO: I think this can be removed no?
                            //currentProcessing.Participations = currentProcessing.Participations.Union(currentProcessing.Participations.SelectMany(pt =>
                            //{
                            //    if (pt.Act == null)
                            //        pt.Act = EntitySource.Current.Get<Act>(pt.ActKey);
                            //    return pt.Act?.Relationships?.Select(r =>
                            //    {
                            //        var retVal = new ActParticipation(ActParticipationKey.RecordTarget, currentProcessing)
                            //        {
                            //            ActKey = r.TargetActKey,
                            //            ParticipationRole = new Model.DataTypes.Concept() { Mnemonic = "RecordTarget", Key = ActParticipationKey.RecordTarget }
                            //        };
                            //        if (r.TargetAct != null)
                            //            retVal.Act = r.TargetAct;
                            //        else
                            //        {
                            //            retVal.Act = currentProcessing.Participations.FirstOrDefault(o=>o.ActKey == r.TargetActKey)?.Act ?? EntitySource.Current.Get<Act>(r.TargetActKey);
                            //        }
                            //        return retVal;
                            //    }
                            //    );
                            //})).ToList();

                            // Add to the promised patient
                            this.m_patientPromise.Add(patient.Key.Value, currentProcessing);
                        }
                    }
                }
                else if (!patient.Key.HasValue) // Not persisted
                {
                    currentProcessing = patient.Clone() as Patient;
                }

                // Initialize for protocol execution
                parmDict.Add("runProtocols", execProtocols.Distinct());
                if (!this.IgnoreViewModelInitializer)
                {
                    foreach (var o in execProtocols)
                    {
                        o.Prepare(currentProcessing, parmDict);
                    }
                }

                parmDict.Remove("runProtocols");

                List<Act> protocolActs = new List<Act>();
                lock (currentProcessing)
                {
                    var thdPatient = currentProcessing.DeepCopy() as Patient;
                    thdPatient.Participations = new List<ActParticipation>(currentProcessing.Participations.ToList().Where(o => o.Act?.MoodConceptKey != ActMoodKeys.Propose && StatusKeys.ActiveStates.Contains(o.Act.StatusConceptKey.Value)));

                    // Let's ensure that there are some properties loaded eh?
                    if (this.IgnoreViewModelInitializer)
                    {
                        foreach (var itm in thdPatient.LoadCollection<ActParticipation>("Participations"))
                        {
                            itm.LoadProperty<Act>("TargetAct").LoadProperty<Concept>("TypeConcept");
                            foreach (var itmPtcpt in itm.LoadProperty<Act>("TargetAct").LoadCollection<ActParticipation>("Participations"))
                            {
                                itmPtcpt.LoadProperty<Concept>("ParticipationRole");
                                itmPtcpt.LoadProperty<Entity>("PlayerEntity").LoadProperty<Concept>("TypeConcept");
                                itmPtcpt.LoadProperty<Entity>("PlayerEntity").LoadProperty<Concept>("MoodConcept");
                            };
                        }
                    }

                    protocolActs = execProtocols.AsParallel().WithDegreeOfParallelism(2).SelectMany(o => o.Calculate(thdPatient, parmDict)).OrderBy(o => o.StopTime - o.StartTime).ToList();
                }

                // Current processing
                if (asEncounters)
                {
                    List<PatientEncounter> encounters = new List<PatientEncounter>();
                    foreach (var act in new List<Act>(protocolActs).Where(o => o.StartTime.HasValue && o.StopTime.HasValue).OrderBy(o => o.StartTime).OrderBy(o => (o.StopTime ?? o.ActTime?.AddDays(7)) - o.StartTime))
                    {
                        act.StopTime = act.StopTime ?? act.ActTime;
                        // Is there a candidate encounter which is bound by start/end
                        var candidate = encounters.FirstOrDefault(e => (act.StartTime ?? DateTimeOffset.MinValue) <= (e.StopTime ?? DateTimeOffset.MaxValue)
                            && (act.StopTime ?? DateTimeOffset.MaxValue) >= (e.StartTime ?? DateTimeOffset.MinValue)
                            && !e.Relationships.Any(r => r.TargetAct?.Protocols.Intersect(act.Protocols, new ProtocolComparer()).Count() == r.TargetAct?.Protocols.Count())
                        );

                        // Create candidate
                        if (candidate == null)
                        {
                            candidate = this.CreateEncounter(act, currentProcessing);
                            encounters.Add(candidate);
                            protocolActs.Add(candidate);
                        }
                        else
                        {
                            TimeSpan[] overlap = {
                            (candidate.StopTime ?? DateTimeOffset.MaxValue) - (candidate.StartTime ?? DateTimeOffset.MinValue),
                            (candidate.StopTime ?? DateTimeOffset.MaxValue) - (act.StartTime ?? DateTimeOffset.MinValue),
                            (act.StopTime ?? DateTimeOffset.MaxValue) - (candidate.StartTime ?? DateTimeOffset.MinValue),
                            (act.StopTime ?? DateTimeOffset.MaxValue) - (act.StartTime ?? DateTimeOffset.MinValue)
                        };
                            // find the minimum overlap
                            var minOverlap = overlap.Min();
                            var overlapMin = Array.IndexOf(overlap, minOverlap);
                            // Adjust the dates based on the start / stop time
                            if (overlapMin % 2 == 1)
                            {
                                candidate.StartTime = act.StartTime;
                            }

                            if (overlapMin > 1)
                            {
                                candidate.StopTime = act.StopTime;
                            }

                            candidate.ActTime = candidate.StartTime ?? candidate.ActTime;
                        }

                        // Add the protocol act
                        candidate.LoadProperty(o => o.Relationships).Add(new ActRelationship(ActRelationshipTypeKeys.HasComponent, act));

                        // Remove so we don't have duplicates
                        protocolActs.Remove(act);
                        currentProcessing.Participations.RemoveAll(o => o.Act == act);
                    }

                    // for those acts which do not have a stop time, schedule them in the first appointment available
                    foreach (var act in new List<Act>(protocolActs).Where(o => !o.StopTime.HasValue))
                    {
                        var candidate = encounters.OrderBy(o => o.StartTime).FirstOrDefault(e => e.StartTime >= act.StartTime);
                        if (candidate == null)
                        {
                            candidate = this.CreateEncounter(act, currentProcessing);
                            encounters.Add(candidate);
                            protocolActs.Add(candidate);
                        }
                        // Add the protocol act
                        candidate.Relationships.Add(new ActRelationship(ActRelationshipTypeKeys.HasComponent, act));

                        // Remove so we don't have duplicates
                        protocolActs.Remove(act);
                        currentProcessing.Participations.RemoveAll(o => o.Act == act);
                    }
                }

                // TODO: Configure for days of week
                foreach (var itm in protocolActs)
                {
                    while (itm.ActTime?.DayOfWeek == DayOfWeek.Sunday || itm.ActTime?.DayOfWeek == DayOfWeek.Saturday)
                    {
                        itm.ActTime = itm.ActTime?.AddDays(1);
                    }
                }

                return new CarePlan(patient, protocolActs.ToList())
                {
                    CreatedByKey = Guid.Parse("fadca076-3690-4a6e-af9e-f1cd68e8c7e8")
                };
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error creating care plan: {0}", e);
                throw new CdssException(protocols, patient, e);
            }
            finally
            {
                lock (m_patientPromise)
                {
                    if (patient.Key.HasValue && this.m_patientPromise.ContainsKey(patient.Key.Value))
                    {
                        m_patientPromise.Remove(patient.Key.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Create an encounter
        /// </summary>
        private PatientEncounter CreateEncounter(Act act, Patient recordTarget)
        {
            var retVal = new PatientEncounter()
            {
                Participations = new List<ActParticipation>()
                        {
                            new ActParticipation(ActParticipationKeys.RecordTarget, recordTarget.Key)
                        },
                ActTime = act.ActTime,
                StartTime = act.StartTime,
                StopTime = act.StopTime,
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
    }
}