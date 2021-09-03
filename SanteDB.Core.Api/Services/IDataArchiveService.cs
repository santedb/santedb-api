/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Data Archive service
    /// </summary>
    [System.ComponentModel.Description("Data Archiving Service")]
    public interface IDataArchiveService : IServiceImplementation
    {
        /// <summary>
        /// Push the specified records to the archive
        /// </summary>
        void Archive(Type modelType, params Guid[] keysToBeArchived);

        /// <summary>
        /// Retrieve a record from the archive by key and type
        /// </summary>
        IdentifiedData Retrieve(Type modelType, Guid keyToRetrieve);

        /// <summary>
        /// Validates whether the specified key exists in the archive
        /// </summary>
        bool Exists(Type modelType, Guid keyToCheck);

        /// <summary>
        /// Purge the specified object from the archive
        /// </summary>
        void Purge(Type modelType, params Guid[] keysToBePurged);

    }

}
