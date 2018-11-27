using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanteDB.Core.Configuration
{
    /// <summary>
    /// Represents a class that can be used to reference types in configuration files
    /// </summary>
    [XmlType(nameof(TypeReferenceConfiguration), Namespace = "http://santedb.org/configuration")]
    public sealed class TypeReferenceConfiguration
    {

        public TypeReferenceConfiguration()
        {

        }

        /// <summary>
        /// Gets the type operation
        /// </summary>
        public TypeReferenceConfiguration(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the type
        /// </summary>
        [XmlAttribute("type")]
        public String TypeXml { get; set; }

        /// <summary>
        /// Gets the type
        /// </summary>
        [XmlIgnore]
        public Type Type
        {
            get => Type.GetType(this.TypeXml);
            set => this.TypeXml = value?.AssemblyQualifiedName;
        }
    }
}
