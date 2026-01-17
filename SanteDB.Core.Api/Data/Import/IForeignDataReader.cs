/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-6-21
 */
using System;
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
        /// Add a computed column to the reader
        /// </summary>
        /// <param name="columnName">The name of the computed column</param>
        /// <param name="computation">The computation for the column</param>
        void AddComputedColumn(string columnName, Func<IForeignDataReader, Object> computation);

        /// <summary>
        /// Clear computed columns
        /// </summary>
        void ClearComputedColumns();

        /// <summary>
        /// True if the source reader has a computed column
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>True if the source reader has the computed column</returns>
        bool HasComputedColumn(string columnName);

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