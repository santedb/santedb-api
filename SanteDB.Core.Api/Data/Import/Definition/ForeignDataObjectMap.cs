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
    /// Foreign data map definition that maps a complex object into a SanteDB
    /// </summary>
    [XmlType(nameof(ForeignDataObjectMap), Namespace = "http://santedb.org/import")]
    public class ForeignDataObjectMap : ForeignDataMapBase
    {

        /// <summary>
        /// The SanteDB resource which should be mapped
        /// </summary>
        [XmlElement("resource"), JsonProperty("resource")]
        public ResourceTypeReferenceConfiguration Resource { get; set; }

        /// <summary>
        /// Mappings for this object
        /// </summary>
        [XmlArray("maps"), XmlArrayItem("add"), JsonProperty("maps")]
        public List<ForeignDataElementMap> Maps { get; set; }

        /// <summary>
        /// An object transformer which is applied to the source data 
        /// </summary>
        [XmlElement("transform"), JsonProperty("transform")]
        public ForeignDataElementTransform Transform { get; set; }

        /// <summary>
        /// Validate this transform
        /// </summary>
        internal IEnumerable<ValidationResultDetail> Validate()
        {
            if(this.Resource == null || this.Resource.Type == null)
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, "Missing resource type", null, this.Source);
            }
            else if(this.Transform == null)
            {
                if(this.Maps?.Any() != true)
                {
                    yield return new ValidationResultDetail(ResultDetailType.Error, "Either Transform or Maps must be defined", null, this.Source);
                }
                foreach(var map in this.Maps)
                {
                    foreach(var val in map.Validate(this.Resource.Type))
                    {
                        yield return val;
                    }
                }
            }
            else if(!this.Transform.Validate())
            {
                yield return new ValidationResultDetail(ResultDetailType.Error, $"Transform {this.Transform.Transformer} failed validation", null, this.Source);
            }
        }
    }
}