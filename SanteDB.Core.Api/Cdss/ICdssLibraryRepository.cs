using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Represents a generic repository which is used for the management of <see cref="ICdssAsset"/>
    /// </summary>
    /// <remarks>The clinical protocol asset repository is responsible for the storage and creation of relevant 
    /// <see cref="ICdssProtocol"/> and <see cref="ICdssLibrary"/> instances which are used by the CDSS 
    /// engine to actually perform their duties</remarks>
    public interface ICdssLibraryRepository : IServiceImplementation
    {

        /// <summary>
        /// Find all protocol assets which match the specified filter
        /// </summary>
        /// <param name="filter">The filter to be applied</param>
        /// <returns>The query result set containing the objects</returns>
        IQueryResultSet<ICdssLibrary> Find(Expression<Func<ICdssLibrary, bool>> filter);

        /// <summary>
        /// Get the protocol asset by identifier
        /// </summary>
        /// <param name="libraryUuid">The protocol asset identifier</param>
        /// <param name="versionUuid">The version of the data to fetch</param>
        /// <returns>The protocol asset with the matching asset id</returns>
        ICdssLibrary Get(Guid libraryUuid, Guid? versionUuid);

        /// <summary>
        /// Insert a protocol asset into the store
        /// </summary>
        /// <param name="libraryToInsert">The protocol asset to insert</param>
        /// <returns>The inserted protocol asset</returns>
        ICdssLibrary InsertOrUpdate(ICdssLibrary libraryToInsert);

        /// <summary>
        /// Remove a protocol asset from the repository by identifier
        /// </summary>
        /// <param name="libraryUuid">The protocol asset to be removed</param>
        /// <returns>The removed protocol asset definition</returns>
        ICdssLibrary Remove(Guid libraryUuid);

    }
}
