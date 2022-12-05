using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Represents a transform which can be applied against a source object 
    /// </summary>
    [XmlType(nameof(ForeignDataElementTransform), Namespace = "http://santedb.org/import")]
    public class ForeignDataElementTransform
    {

        /// <summary>
        /// Gets the type of transform to use
        /// </summary>
        [XmlAttribute("transformer"), JsonProperty("transformer")]
        public string Transformer { get; set; }

        /// <summary>
        /// Gets or sets the list of arguments
        /// </summary>
        [XmlArray("args"),
            XmlArrayItem("int", typeof(Int32)),
            XmlArrayItem("string", typeof(String)),
            XmlArrayItem("bool", typeof(Boolean)),
            XmlArrayItem("dateTime", typeof(DateTime)),
            JsonProperty("args")]
        public List<Object> Arguments { get; set; }

        /// <summary>
        /// Validate this transformer exists
        /// </summary>
        internal bool Validate()
        {
            return ForeignDataImportUtil.Current.TryGetElementTransformer(this.Transformer, out _);
        }
    }

}