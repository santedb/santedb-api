﻿using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Jobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a class which can manage the import and export of data to/from foreign data sources
    /// </summary>
    /// <remarks>
    /// In a data import, the SanteDB services use the following methodology:
    /// <list type="number">
    ///     <item>The source data is first uploaded and stored in SanteDB in a staged form using this <see cref="IForeignDataManager"/></item>
    ///     <item>The source data is validated using the <see cref="IForeignDataFormat"/> appropriate for its format, and rules in the <see cref="ForeignDataMap"/> selected</item>
    ///     <item>The source data is set as "ready for import" if the reviewer is satisfied with the outcome of the staging and validation</item>    
    ///     <item>The staged data is then scheduled for processing on a background <see cref="IJob"/> when the SanteDB instance is not busy and staged data is available</item>
    ///     <item>The staged data is processed and inserted into the underlying data persistence engine</item>
    ///     <item>Any rejected files are placed in a reject file</item>
    /// </list>
    /// </remarks>
    public interface IForeignDataManager
    {
        /// <summary>
        /// Stage the <paramref name="inputStream"/> for future processing
        /// </summary>
        /// <param name="inputStream">The source data stream in the foreign data format</param>
        /// <param name="name">The original name of the source data</param>
        /// <returns>The created foreign data information pointer</returns>
        IForeignDataInfo Stage(Stream inputStream, String name);

        /// <summary>
        /// Flag the <paramref name="foreignDataId"/> for import on the next job execution
        /// </summary>
        /// <param name="foreignDataId">The foreign data to be imported</param>
        /// <param name="foreignDataMap">The foreign data map that should be used on the import</param>
        IForeignDataInfo Import(Guid foreignDataId, ForeignDataMap foreignDataMap);

        /// <summary>
        /// Get the foreign data import information by UUID
        /// </summary>
        /// <param name="foreignDataId">The identifier of the foreign data to fetch</param>
        /// <returns>The foreign data structure</returns>
        IForeignDataInfo Get(Guid foreignDataId);

        /// <summary>
        /// Find foreign data by identifier
        /// </summary>
        /// <param name="query">The query which should be used to match the foreign data import</param>
        /// <returns>The matching foreign data information</returns>
        IEnumerable<IForeignDataInfo> Find(Expression<Func<IForeignDataInfo, bool>> query);

        /// <summary>
        /// Delete foreign data from the server
        /// </summary>
        /// <param name="foreignDataId">The foreign data to be deleted</param>
        /// <returns>The foreign data information that was deleted</returns>
        IForeignDataInfo Delete(Guid foreignDataId);
        
    }
}
