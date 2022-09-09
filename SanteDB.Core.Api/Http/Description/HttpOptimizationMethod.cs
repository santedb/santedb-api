using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Core.Http.Description
{
    /// <summary>
    /// Optimization method
    /// </summary>
    [XmlType(nameof(HttpOptimizationMethod), Namespace = "http://santedb.org/configuration")]
    public enum HttpOptimizationMethod
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
        Bzip2 = 3,

        /// <summary>
        /// Use LZMA
        /// </summary>
        [XmlEnum("7z")]
        Lzma = 4
    }

}
