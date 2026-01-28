/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * User: fyfej
 * Date: 2023-6-21
 */
using SanteDB.Core.Http.Description;
using SharpCompress.Compressors.BZip2;
using SharpCompress.IO;
using System.IO;

namespace SanteDB.Core.Http.Compression
{
    /// <summary>
    /// BZip2 Compression stream
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] // TODO: Design a shim for testing REST context functions
    public class BZip2CompressionScheme : ICompressionScheme
    {
        /// <summary>
        /// Get the encoding
        /// </summary>
        public string AcceptHeaderName
        {
            get
            {
                return "bzip2";
            }
        }

        /// <summary>
        /// Gets the implemented method
        /// </summary>
        public HttpCompressionAlgorithm ImplementedMethod => HttpCompressionAlgorithm.Bzip2;

        /// <summary>
        /// Create compression stream
        /// </summary>
        public Stream CreateCompressionStream(Stream underlyingStream)
        {
            return new BZip2Stream(NonDisposingStream.Create(underlyingStream), SharpCompress.Compressors.CompressionMode.Compress, false);
        }

        /// <summary>
        /// Create decompression stream
        /// </summary>
        public Stream CreateDecompressionStream(Stream underlyingStream)
        {
            return new BZip2Stream(underlyingStream, SharpCompress.Compressors.CompressionMode.Decompress, true);

        }
    }
}
