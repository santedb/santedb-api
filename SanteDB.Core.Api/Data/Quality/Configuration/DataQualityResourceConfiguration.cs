using SanteDB.Core.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Quality.Configuration
{
    /// <summary>
    /// Represents a single data quality configuration for a specific resource
    /// </summary>
    [XmlType(nameof(DataQualityResourceConfiguration), Namespace = "http://santedb.org/configuration")]
    public class DataQualityResourceConfiguration
    {

        /// <summary>
        /// Gets or sets the resource name
        /// </summary>
        [XmlAttribute("resource")]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource
        /// </summary>
        [XmlIgnore]
        public Type ResourceType {
            get => new ModelSerializationBinder().BindToType(null, this.ResourceName);
            set
            {
                new ModelSerializationBinder().BindToName(value, out string asm, out string type);
                this.ResourceName = type;
            }
        }

        /// <summary>
        /// Gets or sets the assertions
        /// </summary>
        [XmlElement("assert")]
        public List<DataQualityResourceAssertion> Assertions { get; set; }

    }
}