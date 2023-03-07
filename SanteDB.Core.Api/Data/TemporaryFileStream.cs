using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Data
{
    /// <summary>
    /// Represents a stream which wraps a <see cref="FileStream"/> 
    /// which deletes the file when disposed
    /// </summary>
    public class TemporaryFileStream : Stream
    {
        // Stream
        private Stream m_stream;
        private String m_fileName;

        public TemporaryFileStream()
        {
            this.m_fileName = Path.GetTempFileName();
            this.m_stream = File.Create(this.m_fileName);
        }

        /// <inheritdoc/>
        public override bool CanRead => this.m_stream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => this.m_stream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => this.m_stream.CanWrite;

        /// <inheritdoc/>
        public override long Length => this.m_stream.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => this.m_stream.Position;
            set => this.m_stream.Position = value;
        }

        /// <inheritdoc/>
        public override void Flush() => this.m_stream.Flush();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => this.m_stream.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => this.m_stream.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value) => this.m_stream.SetLength(value);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => this.m_stream.Write(buffer, offset, count);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_stream.Dispose();
            File.Delete(this.m_fileName);
            base.Dispose(disposing);
        }
    }
}
