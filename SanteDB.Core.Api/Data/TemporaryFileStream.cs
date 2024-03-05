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
 * User: fyfej
 * Date: 2023-6-21
 */
using System;
using System.IO;

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

        /// <summary>
        /// Create a new temporary file stream
        /// </summary>
        public TemporaryFileStream()
        {
            this.m_fileName = Path.Combine(Path.GetTempPath(), $"sdb-tfs-{Guid.NewGuid()}.tmp");
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
