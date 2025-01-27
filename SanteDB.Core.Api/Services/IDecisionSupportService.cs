/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Marker interface for analysis results
    /// </summary>
    public interface ICdssResult
    {
    }

    /// <summary>
    /// CDSS proposal analysis result
    /// </summary>
    public class CdssProposeResult : ICdssResult
    {
        /// <summary>
        /// Create a new proposal result
        /// </summary>
        public CdssProposeResult(Act proposal, Guid? derivedFrom = null)
        {
            this.ProposedAction = proposal;

            if(proposal != null && derivedFrom.HasValue)
            {
                proposal.Relationships = proposal.Relationships ?? new List<ActRelationship>();
                proposal.Relationships.Add(new ActRelationship(ActRelationshipTypeKeys.IsDerivedFrom, derivedFrom.Value));
            }

        }

        /// <summary>
        /// The proposed action
        /// </summary>
        public Act ProposedAction { get; }
    }
    /// <summary>
    /// CDSS Detected issue
    /// </summary>
    public class CdssDetectedIssueResult : ICdssResult
    {
        /// <summary>
        /// Creates a new detected issue result
        /// </summary>
        public CdssDetectedIssueResult(DetectedIssue issue)
        {
            this.Issue = issue;
        }

        /// <summary>
        /// Gets the issue
        /// </summary>
        public DetectedIssue Issue { get; }
    }

    /// <summary>
    /// Service contract for service implementations which generate <see cref="CarePlan"/> instances
    /// </summary>
    /// <remarks>
    /// <para>The care plan generator is responsible for using the <see cref="IClinicalProtocolRepositoryService"/> (which 
    /// stores and manages <see cref="ICdssProtocol"/> instances) to generate instances of patient <see cref="CarePlan"/>
    /// objects which can then be conveyed to the caller and/or stored in the primary CDR.</para>
    /// </remarks>
    [System.ComponentModel.Description("Care Plan Generation Service")]
    public interface IDecisionSupportService : IServiceImplementation
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
        /// Creates a care plan for the specified patient, using only the protocols provided
        /// </summary>
        /// <param name="parameters">The custom parameters that the caller is passing to the care-planner</param>
        /// <param name="groupAsEncounters">True if the suggested actions are to be grouped into <see cref="PatientEncounter"/> instances</param>
        /// <param name="patient">The patient for which the care plan is being generated</param>
        /// <param name="librariesToUse">The libraries from which the care plan should be restricted to</param>
        /// <returns>The generated care plan</returns>
        CarePlan CreateCarePlan(Patient patient, bool groupAsEncounters, IDictionary<String, Object> parameters, params ICdssLibrary[] librariesToUse);

        /// <summary>
        /// Instructs the implementation to analyze the data for <paramref name="collectedData"/> according to the protocols specified in <paramref name="librariesToApply"/>
        /// </summary>
        /// <param name="collectedData">The collected data from the end user</param>
        /// <param name="librariesToApply">The protocol(s) which should be used to evaluate or analyze the data</param>
        /// <param name="parameters">The parameters to apply to the CDSS library</param>
        /// <remarks>
        /// If the <paramref name="librariesToApply"/> parameter is omitted, then the <see cref="Act.Protocols"/> from the <paramref name="collectedData" /> is used
        /// as the list of protocols to be analyzed. A global analysis of the provided data can be requested using the <see cref="AnalyzeGlobal(Act)"/> 
        /// </remarks>
        /// <returns>The detected issues analyzed in the data</returns>
        IEnumerable<ICdssResult> Analyze(IdentifiedData collectedData, IDictionary<String, Object> parameters, params ICdssLibrary[] librariesToApply);

        /// <summary>
        /// Instructs the implementation to analyze the data provided in <paramref name="collectedData"/> using every registered clinical protocol in the 
        /// SanteDB instance.
        /// </summary>
        /// <param name="collectedData">The collected data which is to be analyzed</param>
        /// <param name="parameters">The parameters to apply to the CDSS library</param>
        /// <returns>The detected issues in the analyzed data</returns>
        /// <remarks>This method, while more computationally intensive, allows the CDSS planner to analyze the data elements for all possible protocols which apply to <paramref name="collectedData"/></remarks>
        IEnumerable<ICdssResult> AnalyzeGlobal(IdentifiedData collectedData, IDictionary<String, Object> parameters);
    }
}