using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                _InnerStream.Close();
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
