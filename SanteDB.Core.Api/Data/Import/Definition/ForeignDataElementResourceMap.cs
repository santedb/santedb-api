﻿/*
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
using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Foreign data element resource map
    /// </summary>
    [XmlType(nameof(ForeignDataElementResourceMap), Namespace = "http://santedb.org/import")]
    [XmlInclude(typeof(Concept))]
    [XmlInclude(typeof(ReferenceTerm))]
    [XmlInclude(typeof(Act))]
    [XmlInclude(typeof(TextObservation))]
    [XmlInclude(typeof(ConceptSet))]
    [XmlInclude(typeof(CodedObservation))]
    [XmlInclude(typeof(QuantityObservation))]
    [XmlInclude(typeof(PatientEncounter))]
    [XmlInclude(typeof(ExtensionType))]
    [XmlInclude(typeof(SubstanceAdministration))]
    [XmlInclude(typeof(UserEntity))]
    [XmlInclude(typeof(ApplicationEntity))]
    [XmlInclude(typeof(DeviceEntity))]
    [XmlInclude(typeof(Entity))]
    [XmlInclude(typeof(Patient))]
    [XmlInclude(typeof(AssigningAuthority))]
    [XmlInclude(typeof(ControlAct))]
    [XmlInclude(typeof(Account))]
    [XmlInclude(typeof(InvoiceElement))]
    [XmlInclude(typeof(FinancialContract))]
    [XmlInclude(typeof(FinancialTransaction))]
    [XmlInclude(typeof(Procedure))]
    [XmlInclude(typeof(Provider))]
    [XmlInclude(typeof(Organization))]
    [XmlInclude(typeof(TemplateDefinition))]
    [XmlInclude(typeof(Place))]
    [XmlInclude(typeof(Material))]
    [XmlInclude(typeof(ManufacturedMaterial))]
    [XmlInclude(typeof(CarePlan))]
    [XmlInclude(typeof(DeviceEntity))]
    [XmlInclude(typeof(ApplicationEntity))]
    [XmlInclude(typeof(DeviceEntity))]
    [XmlInclude(typeof(ConceptClass))]
    [XmlInclude(typeof(ConceptRelationship))]
    [XmlInclude(typeof(ConceptRelationshipType))]
    [XmlInclude(typeof(SecurityUser))]
    [XmlInclude(typeof(SecurityProvenance))]
    [XmlInclude(typeof(SecurityRole))]
    [XmlInclude(typeof(SecurityChallenge))]
    [XmlInclude(typeof(CodeSystem))]
    public class ForeignDataElementResourceMap : ResourceTypeReferenceConfiguration
    {

        /// <summary>
        /// Gets or sets the skeleton
        /// </summary>
        [XmlElement("skel"), JsonProperty("skel")]
        public IdentifiedData Skeleton { get; set; }

        /// <summary>
        /// Only perform the mapping when this condition is true
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public List<ForeignDataMapOnlyWhenCondition> OnlyWhen { get; set; }

        /// <summary>
        /// When set to true, if any <see cref="DuplicateCheck"/> returns an existing value, just include it as is and don't update it
        /// </summary>
        [XmlAttribute("preserveExisting"), JsonProperty("preserveExisting")]
        public bool PreserveExisting { get; set; }

        /// <summary>
        /// Mappings for this object
        /// </summary>
        [XmlArray("maps"), XmlArrayItem("map"), JsonProperty("maps")]
        public List<ForeignDataElementMap> Maps { get; set; }

        /// <summary>
        /// Gets or sets the duplicate checks for this object
        /// </summary>
        [XmlArray("existing"), XmlArrayItem("where"), JsonProperty("existing")]
        public List<String> DuplicateCheck { get; set; }

        /// <summary>
        /// An object transformer which is applied to the source data 
        /// </summary>
        [XmlElement("transform"), JsonProperty("transform")]
        public ForeignDataTransformValueModifier Transform { get; set; }


        /// <summary>
        /// Validate this transform
        /// </summary>
        internal IEnumerable<ValidationResultDetail> Validate(string source)
        {
            if (this.Type == null)
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, "Missing resource type", null, source);
            }
            else if (this.Transform == null)
            {
                if (this.Maps?.Any() != true)
                {
                    yield return new ValidationResultDetail(ResultDetailType.Error, "Either Transform or Maps must be defined", null, source);
                }
                foreach (var map in this.Maps)
                {
                    foreach (var val in map.Validate(this.Type))
                    {
                        yield return val;
                    }
                }
            }
            else if (!this.Transform.Validate())
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, $"Transform {this.Transform.Transformer} failed validation", null, source);
            }
        }

    }
}