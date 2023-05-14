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
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Represents a single relationship validation rule
    /// </summary>
    public interface IRelationshipValidationRule
    {
        /// <summary>
        /// The key associated with the rule. This key is used to remove the record at a later date.
        /// </summary>
        Guid? Key { get; }

        /// <summary>
        /// The source class key
        /// </summary>
        Guid? SourceClassKey { get; }

        /// <summary>
        /// The target class key
        /// </summary>
        Guid? TargetClassKey { get; }

        /// <summary>
        /// The relationship which can exist between <see cref="SourceClassKey"/> and <see cref="TargetClassKey"/>
        /// </summary>
        Guid RelationshipTypeKey { get; }

        /// <summary>
        /// The relationship description
        /// </summary>
        String Description { get; }

        /// <summary>
        /// Gets the relationship type that this applies to
        /// </summary>
        Type AppliesTo { get; }
    }

    /// <summary>
    /// Represents a class which can manage the valid relationship types between two objects
    /// </summary>
    public interface IRelationshipValidationProvider : IServiceImplementation
    {

        /// <summary>
        /// Get all valid relationships
        /// </summary>
        /// <returns>All valid relationships</returns>
        IEnumerable<IRelationshipValidationRule> GetValidRelationships<TRelationship>()
            where TRelationship : ITargetedAssociation;

        /// <summary>
        /// Get all valid relationship types between <paramref name="sourceClassKey"/> and all targets
        /// </summary>
        /// <param name="sourceClassKey">The classification of the source on the valid relationship</param>
        /// <returns>The list of validation rules for the applicable source class key</returns>
        IEnumerable<IRelationshipValidationRule> GetValidRelationships<TRelationship>(Guid sourceClassKey)
            where TRelationship : ITargetedAssociation;

        /// <summary>
        /// Query for all relationships registered
        /// </summary>
        IQueryResultSet<IRelationshipValidationRule> QueryRelationships(Expression<Func<IRelationshipValidationRule, bool>> query);

        /// <summary>
        /// Add a valid relationship between <paramref name="sourceClassKey"/> and <paramref name="targetClassKey"/>
        /// </summary>
        /// <param name="sourceClassKey">The source of the relationship</param>
        /// <param name="targetClassKey">The target of the relationship</param>
        /// <param name="relationshipTypeKey">The relationship type key</param>
        /// <param name="description">The textual description of the validation rule</param>
        /// <returns>The created / configured relationship type</returns>
        IRelationshipValidationRule AddValidRelationship<TRelationship>(Guid? sourceClassKey, Guid? targetClassKey, Guid relationshipTypeKey, String description)
            where TRelationship : ITargetedAssociation;

        /// <summary>
        /// Remove the valid relationship type key between 
        /// </summary>
        /// <param name="sourceClassKey">The source classification key type</param>
        /// <param name="targetClassKey">The target classification key</param>
        /// <param name="relationshipTypeKey">The relationship type key</param>
        void RemoveValidRelationship<TRelationship>(Guid? sourceClassKey, Guid? targetClassKey, Guid relationshipTypeKey)
            where TRelationship : ITargetedAssociation;

        /// <summary>
        /// Get a relationship directly using the key of the relationship.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IRelationshipValidationRule GetRuleByKey(Guid key);

        /// <summary>
        /// Remove a relationship directly using the key of the relationship.
        /// </summary>
        /// <param name="key"></param>
        IRelationshipValidationRule RemoveRuleByKey(Guid key);
    }
}
