/*
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
 * Date: 2023-6-21
 */
using SanteDB.Core.Matching;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// An interface which describes a query result score
    /// </summary>
    /// <typeparam name="T">The type of data the scored</typeparam>
    public interface IQueryResultScore<T>
    {
        /// <summary>
        /// Gets the result
        /// </summary>
        T Result { get; }

        /// <summary>
        /// Gets the score
        /// </summary>
        double Score { get; }

        /// <summary>
        /// Indicates the method used to match
        /// </summary>
        RecordMatchMethod Method { get; }

    }

    /// <summary>
    /// Represents a service that can score queries
    /// </summary>
    [System.ComponentModel.Description("Query Result Scoring Provider")]
    public interface IQueryScoringService : IServiceImplementation
    {
        /// <summary>
        /// Requests that the matching operation score the specified series of results
        /// </summary>
        /// <typeparam name="T">The type of record being scored</typeparam>
        /// <param name="filter">The initial filter that the caller has placed on the result set</param>
        /// <param name="results">The results which are to be returned to the caller</param>
        /// <returns>The equivalent record match with score</returns>
        IEnumerable<IQueryResultScore<T>> Score<T>(Expression<Func<T, bool>> filter, IEnumerable<T> results) where T : IdentifiedData;
    }
}
