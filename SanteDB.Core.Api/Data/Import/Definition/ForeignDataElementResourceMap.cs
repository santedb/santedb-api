using Newtonsoft.Json;
using SanteDB.Core.Configuration;
using SanteDB.Core.Model.Map;
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
    public class ForeignDataElementResourceMap : ResourceTypeReferenceConfiguration
    {

        /// <summary>
        /// Only perform the mapping when this condition is true
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public List<ForeignDataMapOnlyWhenCondition> OnlyWhen { get; set; }


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