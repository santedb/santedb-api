using SanteDB.Core.Http.Description;
using SharpCompress.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Http.Compression
{
    /// <summary>
    /// Compression scheme for none
    /// </summary>
    public class NoneCompressionScheme : ICompressionScheme
    {
        /// <summary>
        /// Get the accept header name
        /// </summary>
        public string AcceptHeaderName => "none";

        /// <summary>
        /// The optimization method
        /// </summary>
        public HttpCompressionAlgorithm ImplementedMethod => HttpCompressionAlgorithm.None;

        /// <summary>
        /// Get the compression stream
        /// </summary>
        public Stream CreateCompressionStream(Stream underlyingStream) => NonDisposingStream.Create(underlyingStream);

        /// <summary>
        /// Create decompression stream
        /// </summary>
        public Stream CreateDecompressionStream(Stream underlyingStream) => NonDisposingStream.Create(underlyingStream);
    }
}
