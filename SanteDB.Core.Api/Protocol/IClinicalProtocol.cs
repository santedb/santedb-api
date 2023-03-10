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
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Protocol
{
    /// <summary>
    /// Represents a clinical protocol
    /// </summary>
    public interface IClinicalProtocol
    {

        /// <summary>
        /// Load the specified protocol data
        /// </summary>
        IClinicalProtocol Load(Core.Model.Acts.Protocol protocolData);

        /// <summary>
        /// Get the protocol data
        /// </summary>
        Core.Model.Acts.Protocol GetProtocolData();

        /// <summary>
        /// Gets the identifier for the protocol
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the name of the protocol
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the version of the protocol
        /// </summary>
        String Version { get; }

        /// <summary>
        /// Calculate the clinical protocol for the given patient
        /// </summary>
        IEnumerable<Act> Calculate(Patient p, IDictionary<String, Object> parameters);

        /// <summary>
        /// Update the care plan based on new data
        /// </summary>
        IEnumerable<Act> Update(Patient p, IEnumerable<Act> existingPlan);

        /// <summary>
        /// Called prior to performing calculation of the care protocol allowing the object to prepare the object for whatever 
        /// pre-requisite data is needed for the protocol
        /// </summary>
        void Prepare(Patient p, IDictionary<String, Object> parameters);
    }
}
