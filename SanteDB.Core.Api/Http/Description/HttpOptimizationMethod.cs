/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 */
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
