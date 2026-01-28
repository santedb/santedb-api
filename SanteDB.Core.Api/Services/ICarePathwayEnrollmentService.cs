/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-12-12
 */
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Service which manages the enrolment of patients into care pathways
    /// </summary>
    public interface ICarePathwayEnrollmentService : IServiceImplementation
    {

        /// <summary>
        /// Gets the carepaths that the <paramref name="patient"/> meets eligibility criteria for 
        /// </summary>
        /// <param name="patient">The for which eligibility should be determined</param>
        /// <returns>The care pathways that the patient is eligible to enrol in</returns>
        IEnumerable<CarePathwayDefinition> GetEligibleCarePaths(Patient patient);

        /// <summary>
        /// Get all the care pathways in which the patient is enrolled (i.e. has an active care plan)
        /// </summary>
        /// <param name="patient">The patient for which the enrolled care plans should be fetched</param>
        /// <returns>The enrolled carepaths</returns>
        IEnumerable<CarePathwayDefinition> GetEnrolledCarePaths(Patient patient);

        /// <summary>
        /// Enrol the patient in the specified <paramref name="carePathway"/>
        /// </summary>
        /// <param name="patient">The patient to be enrolled</param>
        /// <param name="carePathway">The care pathway which is to be enrolled in</param>
        /// <returns>The care plan representing the registration into the care pathway</returns>
        CarePlan Enroll(Patient patient, CarePathwayDefinition carePathway);

        /// <summary>
        /// Enrol the patient in the specified <paramref name="carePathwayKey"/>
        /// </summary>
        /// <param name="patient">The patient to be enrolled</param>
        /// <param name="carePathwayKey">The care pathway which is to be enrolled in</param>
        /// <returns>The care plan representing the registration into the care pathway</returns>
        CarePlan Enroll(Patient patient, Guid carePathwayKey);

        /// <summary>
        /// Un-enrols the patient from the <paramref name="carePathway"/>
        /// </summary>
        /// <param name="patient">The patient which is to be un-enroled in the care pathway</param>
        /// <param name="carePathway">The care pathway from which the patient is to be un-enroled</param>
        /// <returns>The care plan that was removed (marked obsolete)</returns>
        CarePlan UnEnroll(Patient patient, CarePathwayDefinition carePathway);

        /// <summary>
        /// Un-enrols the patient from the carepathway having key <paramref name="carePathwayKey"/>
        /// </summary>
        /// <param name="patient">The patient which is to be un-enroled in the care pathway</param>
        /// <param name="carePathwayKey">The care pathway from which the patient is to be un-enroled</param>
        /// <returns>The care plan that was removed (marked obsolete)</returns>
        CarePlan UnEnroll(Patient patient, Guid carePathwayKey);

        /// <summary>
        /// Determines if <paramref name="patient"/> is enrolled in <paramref name="carePathway"/>
        /// </summary>
        /// <param name="patient">The patient to check for enrolment</param>
        /// <param name="carePathway">The care pathway which enrolment is to be checked</param>
        /// <param name="carePlan">The care plan which is registered</param>
        /// <returns>True if <paramref name="patient"/> is enrolled in <paramref name="carePathway"/></returns>
        bool TryGetEnrollment(Patient patient, CarePathwayDefinition carePathway, out CarePlan carePlan);

        /// <summary>
        /// Determines if <paramref name="patient"/> is enrolled in care pathway with key <paramref name="carePathwayKey"/>
        /// </summary>
        /// <param name="patient">The patient to check for enrolment</param>
        /// <param name="carePathwayKey">The care pathway which enrolment is to be checked</param>
        /// <param name="carePlan">The care plan which is registered</param>
        /// <returns>True if <paramref name="patient"/> is enrolled in <paramref name="carePathwayKey"/></returns>
        bool TryGetEnrollment(Patient patient, Guid carePathwayKey, out CarePlan carePlan);

        /// <summary>
        /// Recompute the care plan for <paramref name="patient"/> in pathway <paramref name="pathwayId"/>
        /// </summary>
        /// <param name="patient">The patient for which the care pathway is to be re-computed</param>
        /// <param name="pathwayId">The care pathways identifier to be re-computed</param>
        /// <returns>The updated care plan</returns>
        CarePlan RecomputeOrEnroll(Patient patient, Guid pathwayId);
    }
}
