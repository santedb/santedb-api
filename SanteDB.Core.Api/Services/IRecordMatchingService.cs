using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
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
        IEnumerable<T> Block<T>(T input, String configurationName);

        /// <summary>
        /// Instructs the record matcher to run a detailed classification on the matching blocks in <paramref name="blocks"/>
        /// </summary>
        /// <typeparam name="T">The type of records being matched</typeparam>
        /// <param name="input">The input entity to be matched</param>
        /// <param name="blocks">The blocks which are to be classified as matches</param>
        /// <param name="configurationName">The name of the configuration to use for matching</param>
        /// <param name="matched">The records whose match score exceeds the match threshold</param>
        /// <param name="probable">The record which are probable matches but require review</param>
        /// <param name="discard">The records whose match score is less than unmatch threshold</param>
        /// <returns>True if the classification was successful</returns>
        bool Classify<T>(T input, IEnumerable<T> blocks, string configurationName, out IEnumerable<T> matched, out IEnumerable<T> probable, out IEnumerable<T> discard);

        /// <summary>
        /// Instructs the record matcher to run a block and match operation against <paramref name="input"/>
        /// </summary>
        /// <typeparam name="T">The type of records being matched</typeparam>
        /// <param name="input">The record being compared to</param>
        /// <param name="configurationName">The name of the configuration to be used</param>
        /// <param name="matched">The records whose match score exceeds the match threshold</param>
        /// <param name="probable">The record which are probable matches but require review</param>
        /// <param name="discard">The records whose match score is less than unmatch threshold</param>
        /// <returns>True if classification was successful</returns>
        /// <remarks>
        /// The match method for some implementations of record matching may be equivalent to Block()/Classify() function calls, however
        /// some matching implementations may optimize database round-trips using a single pass.
        /// </remarks>
        bool Match<T>(T input, string configurationName, out IEnumerable<T> matched, out IEnumerable<T> probable, out IEnumerable<T> discard);
    }
}
