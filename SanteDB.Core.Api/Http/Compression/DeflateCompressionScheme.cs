﻿/*
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
using SanteDB.Core.Http.Description;
using SharpCompress.IO;
using System.IO;
using System.IO.Compression;

namespace SanteDB.Core.Http.Compression
{
    /// <summary>
    /// Compression scheme for deflate
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] // TODO: Design a shim for testing REST context functions
    public class DeflateCompressionScheme : ICompressionScheme
    {
        /// <summary>
        /// Encoding this compression scheme handles
        /// </summary>
        public string AcceptHeaderName
        {
            get
            {
                return "deflate";
            }
        }


        /// <summary>
        /// Gets the implemented method
        /// </summary>
        public HttpCompressionAlgorithm ImplementedMethod => HttpCompressionAlgorithm.Deflate;

        /// <summary>
        /// Create the compression stream
        /// </summary>
        public Stream CreateCompressionStream(Stream underlyingStream)
        {
            return new DeflateStream(NonDisposingStream.Create(underlyingStream), CompressionLevel.Fastest);
        }

        /// <summary>
        /// Create a decompression stream
        /// </summary>
        public Stream CreateDecompressionStream(Stream underlyingStream)
        {
            return new DeflateStream(underlyingStream, CompressionMode.Decompress, true);
        }
    }
}
