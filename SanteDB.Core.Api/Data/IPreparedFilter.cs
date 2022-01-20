using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Data
{

    /// <summary>
    /// The status of the filter index
    /// </summary>
    public enum PreparedFilterIndexState
    {
        /// <summary>
        /// The filter index is active and in use
        /// </summary>
        Active,
        /// <summary>
        /// The filter index is being re-built
        /// </summary>
        Rebuilding,
        /// <summary>
        /// The fiter index is inactive (not being used for filtering)
        /// </summary>
        Inactive
    }

    /// <summary>
    /// Represents prepared filter metadata
    /// </summary>
    public interface IPreparedFilter 
    {

        /// <summary>
        /// Get the unique identifier for the filter
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the user that created the index
        /// </summary>
        Guid CreatedByKey { get; }

        /// <summary>
        /// Gets the time that the index was updated
        /// </summary>
        DateTimeOffset? UpdatedTime { get; }

        /// <summary>
        /// Gets the user that updated the index
        /// </summary>
        Guid? UpdatedByKey { get; }

        /// <summary>
        /// Gets the time that the index was created
        /// </summary>
        DateTimeOffset CreationTime { get; }

        /// <summary>
        /// Get the name of the prepared filter index
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the type of data to which the prepared filter index applies
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// The objects to be prepared
        /// </summary>
        Expression IndexExpression { get; }

        /// <summary>
        /// Indexing strategy provider
        /// </summary>
        String Indexer { get;  }

        /// <summary>
        /// Gets the status of the filter index
        /// </summary>
        PreparedFilterIndexState Status { get; }

        /// <summary>
        /// Last re-index
        /// </summary>
        DateTimeOffset? LastReindex { get; }
    }
}
