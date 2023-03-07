using System;
using System.Xml.Serialization;

namespace SanteDB.Core.Http.Description
{
    /// <summary>
    /// Optimization method
    /// </summary>
    [XmlType(nameof(HttpCompressionAlgorithm), Namespace = "http://santedb.org/configuration"), Flags]
    public enum HttpCompressionAlgorithm
    {
        /// <summary>
        /// Do not optimize
        /// </summary>
        [XmlEnum("off")]
        None = 0,

        /// <summary>
        /// Use deflate algorithm
        /// </summary>
        [XmlEnum("df")]
        Deflate = 1,

        /// <summary>
        /// Use GZIP algorithm
        /// </summary>
        [XmlEnum("gz")]
        Gzip = 2,

        /// <summary>
        /// Use BZIP 2
        /// </summary>
        [XmlEnum("bz2")]
        Bzip2 = 4,

        /// <summary>
        /// Use LZMA
        /// </summary>
        [XmlEnum("7z")]
        Lzma = 8
    }

}
