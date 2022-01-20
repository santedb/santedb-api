using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Data
{

    /// <summary>
    /// The prepared index filter service permits for pre-execution of filters
    /// and storage of the results matching the filters
    /// </summary>
    public interface IPreparedFilterManager : IServiceImplementation
    {

        /// <summary>
        /// Get all filter indices which are configured
        /// </summary>
        IEnumerable<IPreparedFilter> GetPreparedFilters();

        /// <summary>
        /// Rebuild the specified index
        /// </summary>
        /// <remarks>A re-build instructs the filter index manager mark the filter
        /// prefetch index to be marked offline (don't use) and re-build the index
        /// expression</remarks>
        /// <param name="indexId">The filter index to be rebuilt</param>
        IPreparedFilter ReCompute(Guid indexId);

        /// <summary>
        /// Get the specified filter index definition
        /// </summary>
        /// <param name="indexId">The filter for which the definition should be fetched</param>
        /// <returns>The matching filter index</returns>
        IPreparedFilter Get(Guid indexId);

        /// <summary>
        /// Create the specified index on <typeparamref name="TTarget"/>
        /// </summary>
        /// <typeparam name="TTarget">The type of object to be indexed</typeparam>
        /// <param name="indexExpression">The expression(s) to index</param>
        /// <param name="indexProvider">The name of the indexing provider</param>
        /// <param name="name">The name of the filter</param>
        IPreparedFilter Create<TTarget>(String name, Expression<Func<TTarget, dynamic>> indexExpression, String indexProvider = null);

        /// <summary>
        /// Delete the specified index
        /// </summary>
        /// <param name="indexId">The index identifier which is to be deleted</param>
        /// <returns>The filter index which is being rebuilt</returns>
        IPreparedFilter Delete(Guid indexId);

        /// <summary>
        /// Transition the state of the index 
        /// </summary>
        /// <param name="indexId">The index to be rebuilt</param>
        /// <param name="newState">The new state to place the index into</param>
        /// <returns>The prepared filter index</returns>
        IPreparedFilter SetStatus(Guid indexId, PreparedFilterIndexState newState);
    }
}
