/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Protocol;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{
    
    /// <summary>
    /// Represents a class which can create care plans
    /// </summary>
    public interface ICarePlanService : IServiceImplementation
    {
        /// <summary>
        /// Gets the list of protocols which can be or should be used to create the care plans
        /// </summary>
        List<IClinicalProtocol> Protocols { get; }

        /// <summary>
        /// Create a care plam
        /// </summary>
        CarePlan CreateCarePlan(Patient p);

        /// <summary>
        /// Create a care plan controlling the creation of encounters
        /// </summary>
        CarePlan CreateCarePlan(Patient p, bool asEncounters);

        /// <summary>
        /// Creates a care plan for the patient with the specified protocolsonly
        /// </summary>
        CarePlan CreateCarePlan(Patient p, bool asEncounters, IDictionary<String, Object> parameters);

        /// <summary>
        /// Creates a care plan for the patient with the specified protocolsonly
        /// </summary>
        CarePlan CreateCarePlan(Patient p, bool asEncounters, IDictionary<String, Object> parameters, params Guid[] protocols);
    }
}