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
        public List<ForeignDataElementResourceMap> Resource { get; set; }

        /// <summary>
        /// Validate this transform
        /// </summary>
        internal IEnumerable<ValidationResultDetail> Validate()
        {
            return this.Resource?.SelectMany(o => o.Validate(this.Source));
        }
    }
}