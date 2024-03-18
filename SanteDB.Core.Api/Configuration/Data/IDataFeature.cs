/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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

namespace SanteDB.Core.Configuration.Data
{
    /// <summary>
    /// Defines a structure for configuration features which deploy or update data
    /// </summary>
    public interface IDataFeature
    {
        /// <summary>
        /// Get the name of the data feature
        /// </summary>
        String Name { get; }

        /// <summary>
        /// Gets the description of the feature
        /// </summary>
        String Description { get; }

        /// <summary>
        /// Gets the database provider for which the feature is intended
        /// </summary>
        String InvariantName { get; }

        /// <summary>
        /// Get the SQL required to deploy the feature
        /// </summary>
        String GetDeploySql();

        /// <summary>
        /// Get SQL required to determine if feature is installed
        /// </summary>
        String GetCheckSql();
    }
}