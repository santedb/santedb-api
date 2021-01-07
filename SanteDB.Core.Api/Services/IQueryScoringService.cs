﻿using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Services
{
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

    }

    /// <summary>
    /// Represents a service that can score queries
    /// </summary>
    public interface IQueryScoringService : IServiceImplementation
    {
        /// <summary>
        /// Requests that the matching operation score the specified series of results
        /// </summary>
        /// <typeparam name="T">The type of record being scored</typeparam>
        /// <param name="filter">The initial filter that the caller has placed on the result set</param>
        /// <param name="results">The results which are to be returned to the caller</param>
        /// <param name="configurationName">The name of the match configurationto use for the scoring</param>
        /// <returns>The equivalent record match with score</returns>
        IEnumerable<IQueryResultScore<T>> Score<T>(Expression<Func<T, bool>> filter, IEnumerable<T> results) where T : IdentifiedData;
    }
}