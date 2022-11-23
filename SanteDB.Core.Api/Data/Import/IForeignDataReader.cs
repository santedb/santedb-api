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
    public interface IForeignDataReader : IDisposable
    {

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
        object this[String name] { get; }

        /// <summary>
        /// Set the value of the current record for field at 
        /// </summary>
        /// <param name="index">The index of the field to set</param>
        /// <returns>The value of the field to set</returns>
        object this[int index] { get; }

        /// <summary>
        /// Get the name of the column at index
        /// </summary>
        /// <param name="index">The index of the name</param>
        /// <returns>The name of the column at the index</returns>
        String GetName(int index);

        /// <summary>
        /// Get the index of the named column
        /// </summary>
        /// <param name="name">The name of the column</param>
        /// <returns>The index of the column</returns>
        int IndexOf(String name);

        /// <summary>
        /// Get the number of columns
        /// </summary>
        int ColumnCount { get; }
    }
}