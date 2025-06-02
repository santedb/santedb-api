/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace SanteDB.Core.Data
{

    /// <summary>
    /// Data management link events
    /// </summary>
    public class DataManagementLinkEventArgs : EventArgs
    {

        /// <summary>
        /// Get the managed link that was altered
        /// </summary>
        public DataManagementLinkEventArgs(ITargetedAssociation targetedAssociation)
        {
            this.TargetedAssociation = targetedAssociation;
        }

        /// <summary>
        /// Gets the managed link that was impacted
        /// </summary>
        public ITargetedAssociation TargetedAssociation { get; }

    }

    /// <summary>
    /// Data maanged link provider
    /// </summary>
    public interface IDataManagedLinkProvider
    {
        /// <summary>
        /// When a data management pattern (like MDM) masks or performs specialized linking or synthesization in the database
        /// this method will allow callers to have the data provider synthesize that data.
        /// </summary>
        /// <remarks>
        /// Sometimes when relationships point to a source or target via an <see cref="ITargetedAssociation"/> 
        /// the source or target will point to a container or linking record. Implementations of this method should
        /// resolve the correct managed record for <paramref name="forSource"/> or return <paramref name="forSource"/>
        /// if the object is unmanaged.
        /// </remarks>
        /// <param name="forSource">The record returned from the persistence layer</param>
        /// <returns>The resolved target object</returns>
        IdentifiedData ResolveManagedRecord(IdentifiedData forSource);

        /// <summary>
        /// When a data management pattern (like MDM) performs compartmentalization of source data 
        /// there is a need for the caller to get the record which is owned by <paramref name="ownerPrincipal"/>
        /// to perform an update (this is common in MDM data imports and migrations)
        /// </summary>
        /// <param name="forTarget">The record returned from the persistence layer</param>
        /// <param name="ownerPrincipal">The owner principal which the method should return</param>
        /// <returns>The resolved record under management which is owned by <paramref name="ownerPrincipal"/></returns>
        IdentifiedData ResolveOwnedRecord(IdentifiedData forTarget, IPrincipal ownerPrincipal);

        /// <summary>
        /// Get the managed reference links for the collection of relationships
        /// </summary>
        /// <param name="forRelationships">The relationship collection on the object</param>
        /// <returns>The reference links on the object</returns>
        IEnumerable<ITargetedAssociation> FilterManagedReferenceLinks(IEnumerable<ITargetedAssociation> forRelationships);

        /// <summary>
        /// Resolve the golden record for the <paramref name="forSource"/> or if <paramref name="forSource"/> is the golden record return it back
        /// </summary>
        /// <param name="forSource">The record to be resolved</param>
        /// <returns>The golden record as determined by the data management pattern</returns>
        IdentifiedData ResolveGoldenRecord(IdentifiedData forSource);
    }

    /// <summary>
    /// Represents a specific data manager within a <see cref="IDataManagementPattern"/> which is responsible for resolving and linking together logical
    /// objects
    /// </summary>
    public interface IDataManagedLinkProvider<T> : IDataManagedLinkProvider
        where T : IdentifiedData
    {
        /// <summary>
        /// Fired when a managed link is established
        /// </summary>
        event EventHandler<DataManagementLinkEventArgs> ManagedLinkEstablished;

        /// <summary>
        /// Fired when a managed link is removed
        /// </summary>
        event EventHandler<DataManagementLinkEventArgs> ManagedLinkRemoved;

        /// <summary>
        /// When a data management pattern (like MDM) masks or performs specialized linking or synthesization in the database
        /// this method will allow callers to have the data provider synthesize that data.
        /// </summary>
        /// <remarks>
        /// Sometimes when relationships point to a source or target via an <see cref="ITargetedAssociation"/> 
        /// the source or target will point to a container or linking record. Implementations of this method should
        /// resolve the correct managed record for <paramref name="forSource"/> or return <paramref name="forSource"/>
        /// if the object is unmanaged.
        /// </remarks>
        /// <param name="forSource">The record returned from the persistence layer</param>
        /// <returns>The resolved target object</returns>
        T ResolveManagedRecord(T forSource);

        /// <summary>
        /// When a data management pattern (like MDM) performs compartmentalization of source data 
        /// there is a need for the caller to get the record which is owned by <paramref name="ownerPrincipal"/>
        /// to perform an update (this is common in MDM data imports and migrations)
        /// </summary>
        /// <param name="forTarget">The record returned from the persistence layer</param>
        /// <param name="ownerPrincipal">The owner principal which the method should return</param>
        /// <returns>The resolved record under management which is owned by <paramref name="ownerPrincipal"/></returns>
        T ResolveOwnedRecord(T forTarget, IPrincipal ownerPrincipal);


        /// <summary>
        /// Add a managed reference link between <paramref name="sourceObject"/> and <paramref name="targetObject"/>
        /// </summary>
        /// <param name="sourceObject">The source object of the link</param>
        /// <param name="targetObject">The target object of the link</param>
        /// <returns>The created target</returns>
        ITargetedAssociation AddManagedReferenceLink(T sourceObject, T targetObject);

    }
}
