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
using System.IO;
using System.Threading;

namespace SanteDB.Core.IO
{
    /// <summary>
    /// A wrapping stream that counts the number of bytes read and written through to the inner stream.
    /// </summary>
    public class ByteCountingStream : System.IO.Stream
    {
        readonly Stream _InnerStream;
        readonly bool _DisposeInnerStream;

        private long _BytesRead;
        private long _BytesWritten;

        /// <summary>
        /// Creates a new instance of the <see cref="ByteCountingStream"/> using an inner stream.
        /// </summary>
        /// <param name="innerStream">The stream to wrap</param>
        public ByteCountingStream(Stream innerStream) : this(innerStream, true)
        {

        }

        /// <summary>
        /// Creates a new instance of the <see cref="ByteCountingStream"/> using an inner stream.
        /// </summary>
        /// <param name="innerStream">The stream to wrap</param>
        /// <param name="disposeInnerStream">True to dispose the inner stream. False to leave it open.</param>
        public ByteCountingStream(Stream innerStream, bool disposeInnerStream)
        {
            _BytesRead = 0;
            _BytesWritten = 0;
            _InnerStream = innerStream;
            _DisposeInnerStream = disposeInnerStream;
        }

        /// <summary>
        /// Gets the number of bytes that have been read through this stream.
        /// </summary>
        public long BytesRead => _BytesRead;
        /// <summary>
        /// Gets the number of bytes that have been written through this stream.
        /// </summary>
        public long BytesWritten => _BytesWritten;

        /// <inheritdoc />
        public override bool CanRead => _InnerStream.CanRead;
        /// <inheritdoc />
        public override bool CanSeek => _InnerStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _InnerStream.CanWrite;
        /// <inheritdoc />
        public override long Length => _InnerStream.Length;
        /// <inheritdoc />
        public override long Position { get => _InnerStream.Position; set => _InnerStream.Position = value; }
        /// <inheritdoc />
        public override void Flush() => _InnerStream.Flush();
        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesread = _InnerStream.Read(buffer, offset, count);

            Interlocked.Add(ref _BytesRead, bytesread);

            return bytesread;
        }
        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _InnerStream.Seek(offset, origin);
        }
        /// <inheritdoc />
        public override void SetLength(long value)
        {
            _InnerStream.SetLength(value);
        }
        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            _InnerStream.Write(buffer, offset, count);
            Interlocked.Add(ref _BytesWritten, count);
        }
        /// <inheritdoc />
        public override void Close()
        {
            base.Close();
            if (_DisposeInnerStream)
            {
                _InnerStream.Close();
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing && _DisposeInnerStream)
            {
                _InnerStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
