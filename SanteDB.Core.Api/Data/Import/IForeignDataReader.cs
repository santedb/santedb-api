using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// A data reader that can read records from a foreign data source
    /// </summary>
    public interface IForeignDataReader : IForeignDataRecord, IDisposable
    {

        /// <summary>
        /// Get the subset name for the map
        /// </summary>
        String SubsetName { get; }

        /// <summary>
        /// Move to the next record
        /// </summary>
        /// <returns>True if the next record was read from the reader</returns>
        bool MoveNext();

        /// <summary>
        /// Set the value of the current record for the field
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The object</returns>
        new object this[String name] { get; }

        /// <summary>
        /// Set the value of the current record for field at 
        /// </summary>
        /// <param name="index">The index of the field to set</param>
        /// <returns>The value of the field to set</returns>
        new object this[int index] { get; }

        /// <summary>
        /// The row number in this reader
        /// </summary>
        int RowNumber { get; }
    }
}