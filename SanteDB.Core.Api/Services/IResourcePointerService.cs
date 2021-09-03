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
using SanteDB.Core.Model.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a service which is tasked with generating verified pointers to data
    /// </summary>
    [System.ComponentModel.Description("Resource Pointer Service")]
    public interface IResourcePointerService : IServiceImplementation
    {

        /// <summary>
        /// Generate a structured pointer for the identified object
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="identifer">The list of identifiers to include in the pointer</param>
        /// <returns>The structured pointer</returns>
        String GeneratePointer<TEntity>(IEnumerable<IdentifierBase<TEntity>> identifer)
            where TEntity : VersionedEntityData<TEntity>, new();

        /// <summary>
        /// Resolve the specified resource
        /// </summary>
        /// <param name="pointerData">The pointer to be resolved</param>
        /// <param name="validate">True if validation should be performed</param>
        /// <returns>The resource</returns>
        IdentifiedData ResolveResource(String pointerData, bool validate = false);
    }
}
