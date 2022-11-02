﻿using Newtonsoft.Json;
using SanteDB.Core.Model;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using SanteDB.Core.Model.Interfaces;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Attributes;

namespace SanteDB.Core.BusinessRules
{
    /// <summary>
    /// Represents a relationship validation rule between two <see cref="ITargetedAssociation"/>
    /// </summary>
    [XmlType(nameof(RelationshipValidationRule), Namespace = "http://santedb.org/model"), JsonObject(nameof(RelationshipValidationRule))]
    public class RelationshipValidationRule : IdentifiedData, IRelationshipValidationRule
    {

        /// <summary>
        /// Gets the time that the validation rule was modified on
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public override DateTimeOffset ModifiedOn => DateTimeOffset.MinValue;

        /// <summary>
        /// Gets the classification key of the source object to which this classification applies
        /// </summary>
        [XmlElement("sourceClass"), JsonProperty("sourceClass")]
        public Guid? SourceClassKey { get; set; }

        /// <summary>
        /// Gets the source class
        /// </summary>
        [SerializationReference(nameof(SourceClassKey)), XmlIgnore, JsonIgnore]
        public Concept SourceClass { get; set; }

        /// <summary>
        /// Get the target class key
        /// </summary>
        [XmlElement("targetClass"), JsonProperty("targetClass")]
        public Guid? TargetClassKey { get; set; }

        /// <summary>
        /// Gets the target classification delay load property
        /// </summary>
        [SerializationReference(nameof(TargetClassKey)), XmlIgnore, JsonIgnore]
        public Concept TargetClass { get; set; }

        /// <summary>
        /// Gets the relationship type between the <see cref="SourceClassKey"/> and <see cref="TargetClassKey"/>
        /// </summary>
        [XmlElement("relationshipType"), JsonProperty("relationshipType")]
        public Guid RelationshipTypeKey { get; set; }

        /// <summary>
        /// Gets the realtionship type delay load
        /// </summary>
        [SerializationReference(nameof(RelationshipTypeKey)), XmlIgnore, JsonIgnore]
        public Concept RelationshipType { get; set; }

        /// <summary>
        /// Gets the description for the validation
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Convert this class to a displayable string
        /// </summary>
        public override string ToDisplay()
        {
            if (null != Description)
            {
                return Description;
            }

            return $"{SourceClassKey?.ToString() ?? "*"} ==[{RelationshipTypeKey}]==> {TargetClassKey?.ToString() ?? "*"}";
        }
    }
}