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
 * Date: 2023-3-10
 */
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Protocol;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Service contract for service implementations which generate <see cref="CarePlan"/> instances
    /// </summary>
    /// <remarks>
    /// <para>The care plan generator is responsible for using the <see cref="IClinicalProtocolRepositoryService"/> (which 
    /// stores and manages <see cref="IClinicalProtocol"/> instances) to generate instances of patient <see cref="CarePlan"/>
    /// objects which can then be conveyed to the caller and/or stored in the primary CDR.</para>
    /// </remarks>
    [System.ComponentModel.Description("Care Plan Generation Service")]
    public interface ICarePlanService : IServiceImplementation
    {

        /// <summary>
        /// Create a new care plan (using all available protocols for which the patient is eligible)
        /// </summary>
        /// <param name="patient">The patient for which the care plan should be generated</param>
        /// <returns>The generated care plan</returns>
        /// <see cref="CreateCarePlan(Patient, bool)"/>
        CarePlan CreateCarePlan(Patient patient);

        /// <summary>
        /// Create a new care plan (using all available protocols which the patient is eligible) and group
        /// the instructions as <see cref="PatientEncounter"/> instances
        /// </summary>
        /// <param name="groupAsEncounters">True if the produced care plan instructions should be grouped into encounters</param>
        /// <param name="patient">The patient for which the care plan is being generated</param>
        /// <returns>The generated care plan</returns>
        CarePlan CreateCarePlan(Patient patient, bool groupAsEncounters);

        /// <summary>
        /// Creates a care plan for the patient providing custom parameters (such as reference data)
        /// </summary>
        /// <param name="patient">The patient for which the care plan is being generated</param>
        /// <param name="groupAsEncounters">When true, instructs the care plan service to group suggested actions into <see cref="PatientEncounter"/></param>
        /// <param name="parameters">Custom parameters which the caller wishes to pass to the planner</param>
        /// <param name="groupId">The group to which the clinical protocol should belong</param>
        /// <returns>The generated care plan</returns>
        CarePlan CreateCarePlan(Patient patient, bool groupAsEncounters, IDictionary<String, Object> parameters, string groupId);

        /// <summary>
        /// Creates a care plan for the specified patient, using only the protocols provided
        /// </summary>
        /// <param name="parameters">The custom parameters that the caller is passing to the care-planner</param>
        /// <param name="groupAsEncounters">True if the suggested actions are to be grouped into <see cref="PatientEncounter"/> instances</param>
        /// <param name="patient">The patient for which the care plan is being generated</param>
        /// <param name="protocols">The protocols which the care plan should be restricted to</param>
        /// <returns>The generated care plan</returns>
        CarePlan CreateCarePlan(Patient patient, bool groupAsEncounters, IDictionary<String, Object> parameters, params IClinicalProtocol[] protocols);
    }
}