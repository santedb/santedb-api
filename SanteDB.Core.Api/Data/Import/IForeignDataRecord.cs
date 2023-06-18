/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using System;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a foreign data record which can be written to a foreign data file with the <see cref="IForeignDataWriter"/>
    /// </summary>
    public interface IForeignDataRecord
    {

        /// <summary>
        /// Set the value of the current record for the field
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The object</returns>
        object this[String name] { get; set; }

        /// <summary>
        /// Set the value of the current record for field at 
        /// </summary>
        /// <param name="index">The index of the field to set</param>
        /// <returns>The value of the field to set</returns>
        object this[int index] { get; set; }

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
