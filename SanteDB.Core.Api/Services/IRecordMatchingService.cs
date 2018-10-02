﻿using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
        Match,
        /// <summary>
        /// There is a probable match
        /// </summary>
        Probable,
        /// <summary>
        /// The is a non-match
        /// </summary>
        NonMatch
    }

    /// <summary>
    /// Represents the match result
    /// </summary>
    public interface IRecordMatchResult<T> 
    {
        /// <summary>
        /// Gets or sets the score of the result
        /// </summary>
        double Score { get; }

        /// <summary>
        /// The record that was matched
        /// </summary>
        T Record { get; }

        /// <summary>
        /// Gets the classification from the matcher
        /// </summary>
        RecordMatchClassification Classification { get; }
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
    }

    /// <summary>
    /// Represents a service that performs record matching and classification
    /// </summary>
    public interface IRecordMatchingService
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
        /// <returns>The record which match the blocking configuration for type <typeparamref name="T"/></returns>
        IEnumerable<T> Block<T>(T input, String configurationName) where T : IdentifiedData;

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
        /// <returns>True if classification was successful</returns>
        /// <remarks>
        /// The match method for some implementations of record matching may be equivalent to Block()/Classify() function calls, however
        /// some matching implementations may optimize database round-trips using a single pass.
        /// </remarks>
        IEnumerable<IRecordMatchResult<T>> Match<T>(T input, string configurationName) where T : IdentifiedData;

        /// <summary>
        /// Performs a score against the specified query (how confident the match is that the <paramref name="input"/> matches the <paramref name="query"/>
        /// </summary>
        /// <typeparam name="T">The type of data being matched</typeparam>
        /// <param name="input">The record being scored</param>
        /// <param name="query">The original query</param>
        /// <param name="configurationName">The configuration name to use for scoring</param>
        /// <returns>The record match result of the object</returns>
        IRecordMatchResult<T> Score<T>(T input, Expression<Func<T, bool>> query, String configurationName) where T: IdentifiedData;
    }
}
