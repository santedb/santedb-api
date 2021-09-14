﻿/*
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
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Matching
{
    /// <summary>
    /// Represents a service that can construct a report from a match result
    /// </summary>
    public interface IMatchReportFactory
    {

        /// <summary>
        /// Create a match report for the matches
        /// </summary>
        /// <param name="input">The input record</param>
        /// <param name="matches">The matches</param>
        /// <param name="inputType">The type of input to create the report for</param>
        /// <returns>A serializable object representing the match reports</returns>
        object CreateMatchReport(Type inputType, object input, IEnumerable<IRecordMatchResult> matches);

        /// <summary>
        /// Create a match report for the matches
        /// </summary>
        /// <typeparam name="T">The type of result to construct a match report for</typeparam>
        /// <param name="input">The input record</param>
        /// <param name="matches">The matches</param>
        /// <returns>A serializable object representing the match reports</returns>
        object CreateMatchReport<T>(T input, IEnumerable<IRecordMatchResult<T>> matches)
            where T : IdentifiedData;
    }
}
