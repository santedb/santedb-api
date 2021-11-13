/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */

using SanteDB.Core.Model;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Matching
{
    /// <summary>
    /// Represents a service that performs record matching and classification
    /// </summary>
    [System.ComponentModel.Description("Record Matching Provider")]
    public interface IRecordMatchingService : IServiceImplementation
    {
        /// <summary>
        /// Instructs the record matching service to perform a quick block function of records
        /// for type <typeparamref name="T"/> with <paramref name="input"/>
        /// </summary>
        /// <remarks>
        /// The blocking stage of the record matching process is a process whereby deterministic rules
        /// are applied to the underlying datastore to come up with a reduced set of records which can be
        /// classified
        /// </remarks>
        /// <typeparam name="T">The type of records being matched</typeparam>
        /// <param name="input">The input record from which blocks should be returned</param>
        /// <param name="configurationId">The configuration that should be used for blocking</param>
        /// <param name="ignoreList">The list of keys which should be ignored (in addition to the IRecordMergingService instructions)</param>
        /// <returns>The record which match the blocking configuration for type <typeparamref name="T"/></returns>
        IQueryResultSet<T> Block<T>(T input, string configurationId, IEnumerable<Guid> ignoreList) where T : IdentifiedData;

        /// <summary>
        /// Instructs the record matcher to run a detailed classification on the matching blocks in <paramref name="blocks"/>
        /// </summary>
        /// <typeparam name="T">The type of records being matched</typeparam>
        /// <param name="input">The input entity to be matched</param>
        /// <param name="blocks">The blocks which are to be classified as matches</param>
        /// <param name="configurationId">The name of the configuration to use for matching</param>
        /// <returns>True if the classification was successful</returns>
        IEnumerable<IRecordMatchResult<T>> Classify<T>(T input, IEnumerable<T> blocks, string configurationId) where T : IdentifiedData;

        /// <summary>
        /// Instructs the record matcher to run a block and match operation against <paramref name="input"/>
        /// </summary>
        /// <typeparam name="T">The type of records being matched</typeparam>
        /// <param name="input">The record being compared to</param>
        /// <param name="configurationId">The name of the configuration to be used</param>
        /// <param name="ignoreList">A list of object to ignore for matching</param>
        /// <returns>True if classification was successful</returns>
        /// <remarks>
        /// The match method for some implementations of record matching may be equivalent to Block()/Classify() function calls, however
        /// some matching implementations may optimize database round-trips using a single pass.
        /// </remarks>
        IEnumerable<IRecordMatchResult<T>> Match<T>(T input, string configurationId, IEnumerable<Guid> ignoreList) where T : IdentifiedData;

        /// <summary>
        /// A non-generic method which uses the type of <paramref name="input"/> to call Match&lt;T>
        /// </summary>
        /// <param name="input">The record being compared</param>
        /// <param name="configurationId">The configuration to use</param>
        /// <param name="ignoreList">The list of data to ignore</param>
        /// <returns>The candidate match results</returns>
        IEnumerable<IRecordMatchResult> Match(IdentifiedData input, string configurationId, IEnumerable<Guid> ignoreList);

        /// <summary>
        /// A non-generic method which uses the type of <paramref name="input"/> to call Classify&lt;T>
        /// </summary>
        /// <param name="input">The record being compared</param>
        /// <param name="configurationId">The configuration to use</param>
        /// <param name="blocks">The blocked data to classify</param>
        /// <returns>The candidate match results</returns>
        IEnumerable<IRecordMatchResult> Classify(IdentifiedData input, IEnumerable<IdentifiedData> blocks, string configurationId);
    }
}