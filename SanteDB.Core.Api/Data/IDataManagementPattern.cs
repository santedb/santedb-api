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
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Services;
using System.Collections.Generic;

namespace SanteDB.Core.Data
{
    /// <summary>
    /// Indicates a class is a data management pattern
    /// </summary>
    /// <remarks>
    /// This interface is a marker interface
    /// </remarks>
    public interface IDataManagementPattern : IServiceImplementation
    {

        /// <summary>
        /// When a data management pattern (like MDM) masks or performs specialized linking in the database
        /// this method will allow callers to discern the true record.
        /// </summary>
        /// <typeparam name="T">The type of record to be resolved</typeparam>
        /// <param name="forSource">The record returned from the persistence layer</param>
        /// <returns>The resolved target object</returns>
        T ResolveManagedTarget<T>(T forSource) where T : class, IHasClassConcept, IHasTypeConcept, IIdentifiedData;

        /// <summary>
        /// When a data management pattern (like MDM) masks or performs specialized linking in the database
        /// and a target has been returned, this method will allow callers to discern the record in the database.
        /// </summary>
        /// <typeparam name="T">The type of record to be resolved</typeparam>
        /// <param name="forTarget">The record returned from the persistence layer</param>
        /// <returns>The resolved target object</returns>
        T ResolveManagedSource<T>(T forTarget) where T : class, IHasClassConcept, IHasTypeConcept, IIdentifiedData;

        /// <summary>
        /// Get the managed reference links for the collection of relationships
        /// </summary>
        /// <typeparam name="T">The type of relationship</typeparam>
        /// <param name="forRelationships">The relationship collection on the object</param>
        /// <returns>The reference links on the object</returns>
        IEnumerable<T> GetManagedReferenceLinks<T>(IEnumerable<T> forRelationships) where T : class, ITargetedAssociation;

        /// <summary>
        /// Add a managed reference link between <paramref name="sourceObject"/> and <paramref name="targetObject"/>
        /// </summary>
        /// <typeparam name="T">The type of object to add a reference link to</typeparam>
        /// <param name="sourceObject">The source object of the link</param>
        /// <param name="targetObject">The target object of the link</param>
        /// <returns>The created target</returns>
        ITargetedAssociation AddManagedReferenceLink<T>(T sourceObject, T targetObject) where T : class, IHasRelationships;
    }
}
