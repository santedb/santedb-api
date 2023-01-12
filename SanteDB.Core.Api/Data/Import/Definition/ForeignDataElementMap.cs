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
        /// Gets or sets the fixed value
        /// </summary>
        [XmlElement("fixed"), JsonProperty("fixed")]
        public string FixedValue { get; set; }

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
        /// When the mapping fails and the target property is missing
        /// </summary>
        [XmlAttribute("whenTargetMissing"), JsonProperty("whenTargetMissing")]
        public DetectedIssuePriorityType TargetMissing { get; set; }

        /// <summary>
        /// Gets or sets the target HDSI path
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public string TargetHdsiPath { get; set; }

        /// <summary>
        /// Gets or sets the transformations on the source column
        /// </summary>
        [XmlArray("transforms"), XmlArrayItem("transform"), JsonProperty("transforms")]
        public List<ForeignDataElementTransform> Transforms { get; set; }

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
            if(String.IsNullOrEmpty(this.TargetHdsiPath))
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, $"Need target path", null, this.Source);
            }
            Exception buildError = null;
            try
            {
                QueryExpressionParser.BuildPropertySelector(context, this.TargetHdsiPath);
            }
            catch(Exception e)
            {
                buildError = e;
            }
            if(buildError != null)
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, $"Target path {this.TargetHdsiPath} is not valid {buildError.Message}", buildError, this.Source);
            }

            foreach(var map in this.Transforms)
            {
                if(!map.Validate())
                {
                    yield return new ValidationResultDetail(ResultDetailType.Warning, $"Validator {map.Transformer} failed validation", null, this.Source);
                }
            }
        }
    }
}