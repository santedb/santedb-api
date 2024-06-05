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

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// A stream based backup asset
    /// </summary>
    public class StreamBackupAsset : IBackupAsset
    {

        private Stream m_fetchedStream;
        private readonly Func<Stream> m_getStreamFunc;

        /// <summary>
        /// Create a new stream backup asset
        /// </summary>
        public StreamBackupAsset(Guid assetClassId, String assetName, Func<Stream> getStreamFunc)
        {
            this.AssetClassId = assetClassId;
            this.Name = assetName;
            this.m_getStreamFunc = getStreamFunc;
        }

        /// <inheritdoc/>
        public Guid AssetClassId { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Open or return the stream
        /// </summary>
        public Stream Open()
        {
            this.m_fetchedStream = this.m_getStreamFunc();
            return this.m_fetchedStream;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.m_fetchedStream?.Dispose();
        }

    }
}
