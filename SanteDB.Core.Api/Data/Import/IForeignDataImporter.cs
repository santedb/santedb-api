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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Services;
using System.Collections.Generic;

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
