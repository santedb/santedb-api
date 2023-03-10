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
 * Date: 2023-3-10
 */
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Initialization
{
    /// <summary>
    /// Represents a service that can install <see cref="Dataset"/> files into the 
    /// registered <see cref="IDataPersistenceService"/> instance
    /// </summary>
    public interface IDatasetInstallerService : IServiceImplementation
    {

        /// <summary>
        /// Install the specified dataset
        /// </summary>
        /// <param name="dataset">The dataset which should be installed</param>
        /// <returns>True if the dataset was installed</returns>
        bool Install(Dataset dataset);

        /// <summary>
        /// Get all installed dataset identifiers
        /// </summary>
        IEnumerable<String> GetInstalled();

        /// <summary>
        /// Get the date that the specified dataset was installed
        /// </summary>
        DateTimeOffset? GetInstallDate(String dataSetId);

    }
}
