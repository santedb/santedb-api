/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a writer for the foreign data format
    /// </summary>
    public interface IForeignDataWriter : IDisposable
    {
        /// <summary>
        /// Write the <paramref name="foreignDataRecord"/> to the writer
        /// </summary>
        /// <param name="foreignDataRecord">The foreign data record to be written</param>
        /// <returns>True if the record was written</returns>
        bool WriteRecord(IForeignDataRecord foreignDataRecord);

        /// <summary>
        /// Gets the row number of the writer
        /// </summary>
        int RecordsWritten { get; }
    }
}