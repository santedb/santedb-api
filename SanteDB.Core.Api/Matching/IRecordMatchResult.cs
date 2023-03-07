/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Model;
using System.Collections.Generic;

namespace SanteDB.Core.Matching
{
    /// <summary>
    /// Represents the match result
    /// </summary>
    public interface IRecordMatchResult<T> : IRecordMatchResult
        where T : IdentifiedData
    {
        /// <summary>
        /// The record that was matched
        /// </summary>
        new T Record { get; }

    }
    /// <summary>
    /// Represents a general purpose match result interface
    /// </summary>
    public interface IRecordMatchResult
    {
        /// <summary>
        /// Gets or sets the record which match was performe don 
        /// </summary>
        IdentifiedData Record { get; }

        /// <summary>
        /// Gets or sets the score of the result, usually an absolute score
        /// </summary>
        double Score { get; }

        /// <summary>
        /// Gets or sets the relative strength of the result
        /// </summary>
        double Strength { get; }

        /// <summary>
        /// Gets the classification from the matcher
        /// </summary>
        RecordMatchClassification Classification { get; }

        /// <summary>
        /// Indicates the method used to match
        /// </summary>
        RecordMatchMethod Method { get; }

        /// <summary>
        /// Match record attributes
        /// </summary>
        IEnumerable<IRecordMatchVector> Vectors { get; }

        /// <summary>
        /// Gets the configuration name
        /// </summary>
        IRecordMatchingConfiguration Configuration { get; }
    }
}
