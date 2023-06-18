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
using SanteDB.Core.Model.DataTypes;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a repository service for managing assigning authorities.
    /// </summary>
    /// <remarks>This specialized <see cref="IRepositoryService"/> is intended to add functionality 
    /// to make the management of identity domains (<see cref="IdentityDomain"/>) objects simpler by including 
    /// methods for getting domains by name and URI</remarks>
    [System.ComponentModel.Description("Identity Domain Provider")]
    public interface IIdentityDomainRepositoryService : IRepositoryService<IdentityDomain>
    {

        /// <summary>
        /// Get by domain
        /// </summary>
        IdentityDomain Get(String domain);

        /// <summary>
        /// Get assigning authority by uri 
        /// </summary>
        IdentityDomain Get(Uri uri);
    }
}