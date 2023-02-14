using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Import.Definition
{
    /// <summary>
    /// Foreign data target base
    /// </summary>
    [XmlType(nameof(ForeignDataValueModifier), Namespace = "http://santedb.org/import")]
    public abstract class ForeignDataValueModifier
    {

        /// <summary>
        /// When the source value matches this value apply the transform
        /// </summary>
        [XmlAttribute("when"), JsonProperty("when")]
        public string When { get; set; }

    }
}
