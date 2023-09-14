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
 * Date: 2023-5-19
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Represents a clinical protocol
    /// </summary>
    public interface ICdssProtocolAsset : ICdssAsset
    {

        /// <summary>
        /// Calculate the clinical protocol for the given target data
        /// </summary>
        IEnumerable<Act> ComputeProposals(IdentifiedData target, IDictionary<String, Object> parameters);

        /// <summary>
        /// Analyze the collected samples and determine if there are any detected issues
        /// </summary>
        /// <remarks>This method allows callers to invoke the CDSS to analyse data which was provided in the user interface</remarks>
        IEnumerable<DetectedIssue> Analyze(IdentifiedData analysisTarget);

        /// <summary>
        /// Called prior to performing calculation of the care protocol allowing the protocol to prepare <paramref name="target"/> with
        /// pre-requisite data is needed for the protocol
        /// </summary>
        void Prepare(Patient target, IDictionary<String, Object> parameters);
    }
}
