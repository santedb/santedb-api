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
    /// <see cref="ICdssProtocolAsset"/> and <see cref="ICdssLibraryAsset"/> instances which are used by the CDSS 
    /// engine to actually perform their duties</remarks>
    public interface ICdssAssetRepository : IServiceImplementation
    {

        /// <summary>
        /// Find all protocol assets which match the specified filter
        /// </summary>
        /// <param name="filter">The filter to be applied</param>
        /// <returns>The query result set containing the objects</returns>
        IQueryResultSet<ICdssAsset> Find(Expression<Func<ICdssAsset, bool>> filter);

        /// <summary>
        /// Get the protocol asset by identifier
        /// </summary>
        /// <param name="protocolAssetId">The protocol asset identifier</param>
        /// <returns>The protocol asset with the matching asset id</returns>
        ICdssAsset Get(Guid protocolAssetId);

        /// <summary>
        /// Get the protocol asset by OID
        /// </summary>
        /// <param name="protocolAssetOid">The protocol asset OID to fetch</param>
        /// <returns>The protocol asset with matching oid</returns>
        ICdssAsset GetByOid(String protocolAssetOid);

        /// <summary>
        /// Insert a protocol asset into the store
        /// </summary>
        /// <param name="protocolAsset">The protocol asset to insert</param>
        /// <returns>The inserted protocol asset</returns>
        ICdssAsset InsertOrUpdate(ICdssAsset protocolAsset);

        /// <summary>
        /// Remove a protocol asset from the repository by identifier
        /// </summary>
        /// <param name="protocolAssetId">The protocol asset to be removed</param>
        /// <returns>The removed protocol asset definition</returns>
        ICdssAsset Remove(Guid protocolAssetId);

    }
}
