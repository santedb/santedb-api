﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-11-24
 */
using SanteDB.Core.Model;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a factory service which can be used to generate default factories
    /// </summary>
    public interface IRepositoryServiceFactory : IServiceImplementation
    {

        /// <summary>
        /// Create the specified resource 
        /// </summary>
        IRepositoryService<T> CreateRepository<T>() where T : IdentifiedData;

    }
}
