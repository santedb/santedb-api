/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
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
 * User: justin
 * Date: 2018-6-21
 */
using SanteDB.Core.Model.Roles;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents the patient repository service. This service is responsible
    /// for ensuring that patient roles in the IMS database are in a consistent
    /// state.
    /// </summary>
    public interface IPatientRepositoryService : IRepositoryService<Patient>, IValidatingRepositoryService<Patient>
    {

        /// <summary>
        /// Merges two patients together
        /// </summary>
        /// <param name="survivor">The surviving patient record</param>
        /// <param name="victim">The victim patient record</param>
        /// <returns>A new version of patient representing the merge</returns>
        Patient Merge(Patient survivor, Patient victim);

        /// <summary>
        /// Un-merges two patients from each other
        /// </summary>
        /// <param name="patient">The patient which is to be un-merged</param>
        /// <param name="versionKey">The version of patient P where the split should occur</param>
        /// <returns>A new patient representing the split record</returns>
        Patient UnMerge(Patient patient, Guid versionKey);

    }
}