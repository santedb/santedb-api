using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data.Import
{

    /// <summary>
    /// Foreign data transformation service is responsible for executing foreign data transforms
    /// </summary>
    public interface IForeignDataImporter 
    {

        /// <summary>
        /// Validate the <paramref name="foreignDataObjectMap"/> can be run against <paramref name="sourceReader"/>
        /// </summary>
        /// <param name="foreignDataObjectMap">The foreign data object map to validate</param>
        /// <param name="sourceReader">The source reader</param>
        /// <returns>The detected issues</returns>
        IEnumerable<DetectedIssue> Validate(ForeignDataObjectMap foreignDataObjectMap, IForeignDataReader sourceReader);

        /// <summary>
        /// Transform the foreign data <paramref name="sourceReader"/> into the database using <paramref name="transactionMode"/>
        /// </summary>
        /// <param name="rejectWriter">The writer where rejects should be written</param>
        /// <param name="sourceReader">The reader from where foreign data objects should be read</param>
        /// <param name="foreignDataObjectMap">The foreign data map</param>
        /// <param name="transactionMode">The transaction mode to use (using <see cref="TransactionMode.Rollback"/> performs a validation</param>
        /// <returns>The detected issues with the transform</returns>
        IEnumerable<DetectedIssue> Import(ForeignDataObjectMap foreignDataObjectMap, IForeignDataReader sourceReader, IForeignDataWriter rejectWriter, TransactionMode transactionMode);

    }
}
