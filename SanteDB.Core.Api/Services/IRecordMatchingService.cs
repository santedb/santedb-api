/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Record match classifications
    /// </summary>
    public enum RecordMatchClassification
    {
        /// <summary>
        /// The records match
        /// </summary>
        Match = 0x2,
        /// <summary>
        /// There is a probable match
        /// </summary>
        Probable = 0x1,
        /// <summary>
        /// The is a non-match
        /// </summary>
        NonMatch = 0x0
    }

    /// <summary>
    /// Identifies the method used to calculate the match score
    /// </summary>
    public enum RecordMatchMethod
    {
        /// <summary>
        /// The match was recommended based on an known good identifier
        /// </summary>
        Identifier,
        /// <summary>
        /// Exact matching/deterministic
        /// </summary>
        Deterministic,
        /// <summary>
        /// The match was determined using a probability / weighted algorithm
        /// </summary>
        Weighted
    }

    /// <summary>
    /// Match attribute
    /// </summary>
    public interface IRecordMatchAttribute
    {
        /// <summary>
        /// Gets the name of the attribute
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the score
        /// </summary>
        double Score { get; }
    
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
        IEnumerable<IRecordMatchAttribute> Attributes { get;}
    }


    /// <summary>
    /// Represents the match result
    /// </summary>
    public interface IRecordMatchResult<T> : IRecordMatchResult
        where T : IdentifiedData
    {
        /// <summary>
        /// The record that was matched
        /// </summary>
        T Record { get; }

    }

    /// <summary>
    /// Gets the record matching configuration
    /// </summary>
    public interface IRecordMatchingConfiguration
    {
        /// <summary>
        /// Gets the name of the record matching configuration for use in blocking
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the types that this applies to
        /// </summary>
        Type[] AppliesTo { get; }
    }

    /// <summary>
    /// Represents a service that can construct a report from a match result
    /// </summary>
    public interface IMatchReportFactory
    {

        /// <summary>
        /// Create a match report for the matches
        /// </summary>
        /// <typeparam name="T">The type of result to construct a match report for</typeparam>
        /// <param name="input">The input record</param>
        /// <param name="matches">The matches</param>
        /// <returns>A serializable object representing the match reports</returns>
        Object CreateMatchReport<T>(T input, IEnumerable<IRecordMatchResult<T>> matches)
            where T: IdentifiedData;
    }


    /// <summary>
    /// Represents a service that performs record matching and classification
    /// </summary>
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
        /// <param name="configurationName">The configuration that should be used for blocking</param>
        /// <param name="ignoreList">The list of keys which should be ignored (in addition to the IRecordMergingService instructions)</param>
        /// <returns>The record which match the blocking configuration for type <typeparamref name="T"/></returns>
        IEnumerable<T> Block<T>(T input, String configurationName, IEnumerable<Guid> ignoreList) where T : IdentifiedData;

        /// <summary>
        /// Instructs the record matcher to run a detailed classification on the matching blocks in <paramref name="blocks"/>
        /// </summary>
        /// <typeparam name="T">The type of records being matched</typeparam>
        /// <param name="input">The input entity to be matched</param>
        /// <param name="blocks">The blocks which are to be classified as matches</param>
        /// <param name="configurationName">The name of the configuration to use for matching</param>
        /// <returns>True if the classification was successful</returns>
        IEnumerable<IRecordMatchResult<T>> Classify<T>(T input, IEnumerable<T> blocks, string configurationName) where T : IdentifiedData;

        /// <summary>
        /// Instructs the record matcher to run a block and match operation against <paramref name="input"/>
        /// </summary>
        /// <typeparam name="T">The type of records being matched</typeparam>
        /// <param name="input">The record being compared to</param>
        /// <param name="configurationName">The name of the configuration to be used</param>
        /// <param name="ignoreList">A list of object to ignore for matching</param>
        /// <returns>True if classification was successful</returns>
        /// <remarks>
        /// The match method for some implementations of record matching may be equivalent to Block()/Classify() function calls, however
        /// some matching implementations may optimize database round-trips using a single pass.
        /// </remarks>
        IEnumerable<IRecordMatchResult<T>> Match<T>(T input, string configurationName, IEnumerable<Guid> ignoreList) where T : IdentifiedData;

        /// <summary>
        /// A non-generic method which uses the type of <paramref name="input"/> to call Match&lt;T>
        /// </summary>
        /// <param name="input">The record being compared</param>
        /// <param name="configurationName">The configuration to use</param>
        /// <returns>The candidate match results</returns>
        IEnumerable<IRecordMatchResult> Match(IdentifiedData input, string configurationName, IEnumerable<Guid> ignoreList);
    }
}
