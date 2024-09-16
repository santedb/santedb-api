/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Represents a single foreign data element to be mapped
    /// </summary>
    [XmlType(nameof(ForeignDataElementMap), Namespace = "http://santedb.org/import")]
    public class ForeignDataElementMap : ForeignDataMapBase
    {

        /// <summary>
        /// Only when the conditions are true
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public List<ForeignDataMapOnlyWhenCondition> OnlyWhen { get; set; }

        /// <summary>
        /// Gets or sets the value modifiers
        /// </summary>
        [XmlElement("fixed", typeof(ForeignDataFixedValueModifier)),
            XmlElement("lookup", typeof(ForeignDataLookupValueModifier)),
            XmlElement("xref", typeof(ForeignDataOutputReferenceModifier)),
            XmlElement("parameter", typeof(ForeignDataParameterValueModifier)),
            XmlElement("transform", typeof(ForeignDataTransformValueModifier)), JsonProperty("values")]
        public List<ForeignDataValueModifier> ValueModifiers { get; set; }

        /// <summary>
        /// True if the source is required
        /// </summary>
        [XmlAttribute("required"), JsonProperty("required")]
        public bool SourceRequired { get; set; }

        /// <summary>
        /// True if the <see cref="TargetMissing"/> property was in the definition file
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool TargetMissingSpecified { get; set; }


        /// <summary>
        /// Error message
        /// </summary>
        [XmlAttribute("errorMessage"), JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// When the mapping fails and the target property is missing
        /// </summary>
        [XmlAttribute("whenTargetMissing"), JsonProperty("whenTargetMissing")]
        public DetectedIssuePriorityType TargetMissing { get; set; }

        /// <summary>
        /// Gets or sets the target HDSI path
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public ForeignDataTargetExpression TargetHdsiPath { get; set; }


        /// <summary>
        /// Replace the current value
        /// </summary>
        [XmlAttribute("replace"), JsonProperty("replace")]
        public bool ReplaceExisting { get; set; }

        /// <summary>
        /// Validate this map
        /// </summary>
        internal IEnumerable<ValidationResultDetail> Validate(Type context)
        {
            if (String.IsNullOrEmpty(this.TargetHdsiPath?.Value))
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, $"Need target path", null, this.Source);
            }
            Exception buildError = null;
            try
            {
                QueryExpressionParser.BuildPropertySelector(context, this.TargetHdsiPath?.Value);
            }
            catch (Exception e)
            {
                buildError = e;
            }
            if (buildError != null)
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, $"Target path {this.TargetHdsiPath} is not valid {buildError.Message}", buildError, this.Source);
            }

            foreach (var map in this.ValueModifiers)
            {
                if (map is ForeignDataTransformValueModifier fdx && !fdx.Validate())
                {
                    yield return new ValidationResultDetail(ResultDetailType.Warning, $"Validator {fdx.Transformer} failed validation", null, this.Source);
                }
            }
        }
    }
}