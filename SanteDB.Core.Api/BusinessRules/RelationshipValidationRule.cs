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
using Newtonsoft.Json;
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
