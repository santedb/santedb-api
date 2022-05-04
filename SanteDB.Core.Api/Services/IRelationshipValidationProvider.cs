using SanteDB.Core.Model.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Services
{

    /// <summary>
    /// Represents a single relationship validation rule
    /// </summary>
    public interface IRelationshipValidationRule
    {
        /// <summary>
        /// The source class key
        /// </summary>
        Guid SourceClassKey { get; }

        /// <summary>
        /// The target class key
        /// </summary>
        Guid TargetClassKey { get; }

        /// <summary>
        /// The relationship which can exist between <see cref="SourceClassKey"/> and <see cref="TargetClassKey"/>
        /// </summary>
        Guid RelationshipTypeKey { get; }

        /// <summary>
        /// The relationship description
        /// </summary>
        String Description { get; }
    }

    /// <summary>
    /// Represents a class which can manage the valid relationship types between two objects
    /// </summary>
    public interface IRelationshipValidationProvider : IServiceProvider
    {

        /// <summary>
        /// Get all valid relationships
        /// </summary>
        /// <returns>All valid relationships</returns>
        IEnumerable<IRelationshipValidationRule> GetValidRelationships();

        /// <summary>
        /// Get all valid relationship types between <paramref name="sourceClassKey"/> and all targets
        /// </summary>
        /// <param name="sourceClassKey">The classification of the source on the valid relationship</param>
        /// <returns>The list of validation rules for the applicable source class key</returns>
        IEnumerable<IRelationshipValidationRule> GetValidRelationships(Guid sourceClassKey);

        /// <summary>
        /// Add a valid relationship between <paramref name="sourceClassKey"/> and <paramref name="targetClassKey"/>
        /// </summary>
        /// <param name="sourceClassKey">The source of the relationship</param>
        /// <param name="targetClassKey">The target of the relationship</param>
        /// <param name="relationshipTypeKey">The relationship type key</param>
        /// <param name="description">The textual description of the validation rule</param>
        /// <returns>The created / configured relationship type</returns>
        IRelationshipValidationRule AddValidRelationship(Guid sourceClassKey, Guid targetClassKey, Guid relationshipTypeKey, String description);

        /// <summary>
        /// Remove the valid relationship type key between 
        /// </summary>
        /// <param name="sourceClassKey">The source classification key type</param>
        /// <param name="targetClassKey">The target classification key</param>
        /// <param name="relationshipTypeKey">The relationship type key</param>
        /// <returns>The removed validation rule</returns>
        IRelationshipValidationRule RemoveValidRelationship(Guid sourceClassKey, Guid targetClassKey, Guid relationshipTypeKey);

    }
}
