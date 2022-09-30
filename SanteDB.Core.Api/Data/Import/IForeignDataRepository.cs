/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using System;
using System.IO;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a class which manages foreign data import and export to/from SanteDB
    /// </summary>
    public interface IForeignDataRepository
    {

        /// <summary>
        /// Stages the foreign data stream in a place which can be accessed by SanteDB
        /// </summary>
        /// <param name="foreignDataStream">The stream containing the source foreign data</param>
        /// <param name="fileName">The tag of the foreign data import object</param>
        /// <returns>The description of the foreign data which was uploaded</returns>
        ForeignDataElementGroup Stage(Stream foreignDataStream, String fileName);

        /// <summary>
        /// The stream which represents the staged data
        /// </summary>
        /// <param name="fileName">The tag which was assigned to the staged data</param>
        /// <returns>The staged source data</returns>
        Stream Get(String fileName);

        /// <summary>
        /// Delete the staged data tag
        /// </summary>
        /// <param name="fileName">The tag which should be deleted</param>
        void DeleteStagedData(String fileName);

    }
}
