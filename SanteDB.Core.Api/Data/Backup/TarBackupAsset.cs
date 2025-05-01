/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
    /// Represents a backup asset which is from a backup file
    /// </summary>
    internal class TarBackupAsset : IBackupAsset
    {
        private readonly Stream m_underlyingStream;

        /// <summary>
        /// A new backup asset from a TAR file
        /// </summary>
        public TarBackupAsset(string assetName, Guid assetClassId, Stream entryStream)
        {
            this.Name = assetName;
            this.AssetClassId = assetClassId;
            this.m_underlyingStream = entryStream;
        }

        /// <summary>
        /// Gets the asset class identifier
        /// </summary>
        public Guid AssetClassId { get; }


        /// <summary>
        /// Get the asset name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            this.m_underlyingStream.Dispose();
        }

        /// <summary>
        /// Open the stream
        /// </summary>
        public Stream Open() => this.m_underlyingStream;
    }
}