﻿/*
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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Represents an implementation of a clinical protocol library
    /// </summary>
    /// <remarks>
    /// A clinical protocol library represents a collection of common elements such as when, then definitions, or variables which can be referenced by other
    /// objects.
    /// </remarks>
    public interface ICdssLibrary : ICdssAsset
    {

        /// <summary>
        /// Get the protocols which are defined in the library (note that implementers typically indicate that protocols are only for type Patient)
        /// </summary>
        /// <param name="forPatient">The patient for which applicable protocols should be obtained</param>
        /// <param name="forScope">The scope(s) for which the protocols should be obtained</param>
        IEnumerable<ICdssProtocol> GetProtocols(Patient forPatient, IDictionary<String, object> parameters, params String[] forScope);

        /// <summary>
        /// Analyze the collected samples and determine if there are any detected issues
        /// </summary>
        /// <param name="analysisTarget">The target object souce which is to be analyzed</param>
        /// <param name="parameters">The parameters to supply for the analysis</param>
        /// <remarks>This method allows callers to invoke the CDSS to analyse data which was provided in the user interface. 
        /// <para>This method is equivalent to calling <c>ICdssLibrary.Execute(target).OfType&lt;DetectedIssue></c></para>
        /// <para>Some decision logic may update the properties in <paramref name="target"/>, so calling this repeatedly may have different results. It is recommended 
        /// if callers do not want <paramref name="target"/> to be modified, that they use <see cref="ICanDeepCopy.DeepCopy"/></para>
        /// </remarks>
        IEnumerable<ICdssResult> Analyze(IdentifiedData analysisTarget, IDictionary<String, object> parameters = null);

        /// <summary>
        /// Execute all applicable decision logic for <paramref name="target"/> and emit all of the proposed objects and raised issues
        /// </summary>
        /// <param name="target">The target to be analyzed</param>
        /// <param name="parameters">The parameters to be supplied in the execution of the protocol</param>
        /// <returns>The decision logic target</returns>
        /// <remarks>Some decision logic may update the properties in <paramref name="target"/>, so calling this repeatedly may have different results. It is recommended 
        /// if callers do not want <paramref name="target"/> to be modified, that they use <see cref="ICanDeepCopy.DeepCopy"/></remarks>
        IEnumerable<ICdssResult> Execute(IdentifiedData target, IDictionary<String, object> parameters = null);

        /// <summary>
        /// Load the protocl definition to <paramref name="definitionStream"/>
        /// </summary>
        void Load(Stream definitionStream);

        /// <summary>
        /// Save the protocol definition to <paramref name="definitionStream"/>
        /// </summary>
        void Save(Stream definitionStream);

        /// <summary>
        /// Get the protocol definitions from the CDSS library
        /// </summary>
        IEnumerable<Protocol> GetProtocolDefinitions();

        /// <summary>
        /// If the CDSS library data came from storage, this is the metadata
        /// </summary>
        [QueryParameter("storage")]
        ICdssLibraryRepositoryMetadata StorageMetadata { get; set; }

    }
}