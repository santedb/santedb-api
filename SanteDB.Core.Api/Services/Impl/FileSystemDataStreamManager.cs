/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using SanteDB.Core.i18n;
using SanteDB.Core.Security.Services;
using System;
using System.IO;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// File system based stream manager
    /// </summary>
    public class FileSystemDataStreamManager : IDataStreamManager
    {
        private readonly ISymmetricCryptographicProvider m_symmetricCryptographicProvider;
        private readonly string m_fileLocation;

        /// <summary>
        /// DI constructor
        /// </summary>
        public FileSystemDataStreamManager(ISymmetricCryptographicProvider symmetricCryptographicProvider)
        {
            this.m_symmetricCryptographicProvider = symmetricCryptographicProvider;
            this.m_fileLocation = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString() ?? ".", "datastream");
            if (!Directory.Exists(this.m_fileLocation))
            {
                Directory.CreateDirectory(this.m_fileLocation);
            }
        }

        /// <inheritdoc/>
        public Guid Add(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            else if (!stream.CanRead)
            {
                throw new ArgumentException(nameof(stream), ErrorMessages.CANT_READ_WRITE_ONLY_STREAM);
            }

            var fileId = Guid.NewGuid();
            using (var fs = File.Create(Path.Combine(this.m_fileLocation, fileId.ToString())))
            {
                var iv = this.m_symmetricCryptographicProvider.GenerateIV();
                fs.Write(iv, 0, iv.Length);
                using (var cs = this.m_symmetricCryptographicProvider.CreateEncryptingStream(fs, this.m_symmetricCryptographicProvider.GetContextKey(), iv))
                {
                    stream.CopyTo(cs);
                }
            }
            return fileId;
        }

        /// <inheritdoc/>
        public Stream Get(Guid streamId)
        {
            var fileName = Path.Combine(this.m_fileLocation, streamId.ToString());
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            var ms = new MemoryStream();
            using (var fs = File.OpenRead(fileName))
            {
                var iv = new byte[16];
                fs.Read(iv, 0, iv.Length);
                using (var cs = this.m_symmetricCryptographicProvider.CreateDecryptingStream(fs, this.m_symmetricCryptographicProvider.GetContextKey(), iv))
                {
                    cs.CopyTo(ms);
                }
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        /// <inheritdoc/>
        public void Remove(Guid streamId)
        {
            var fileName = Path.Combine(this.m_fileLocation, streamId.ToString());
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            File.Delete(fileName);
        }
    }
}
