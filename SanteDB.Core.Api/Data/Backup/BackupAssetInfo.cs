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
using System;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Data.Backup
{

    /// <summary>
    /// Backup asset metadata
    /// </summary>
    public class BackupAssetInfo
    {

        /// <summary>
        /// Create backup from asset buffer
        /// </summary>
        internal BackupAssetInfo(byte[] assetBuffer)
        {
            this.AssetClassId = new Guid(assetBuffer.Take(16).ToArray());
            this.AssetName = Encoding.UTF8.GetString(assetBuffer, 16, 256).Trim();
        }

        /// <summary>
        /// Create backup from asset info
        /// </summary>
        internal BackupAssetInfo(IBackupAsset asset)
        {
            this.AssetClassId = asset.AssetClassId;
            this.AssetName = asset.Name;
        }

        /// <summary>
        /// Gets the class identifier of the asset
        /// </summary>
        public Guid AssetClassId { get; set; }

        /// <summary>
        /// Gets the name of the asset
        /// </summary>
        public String AssetName { get; set; }

        /// <summary>
        /// Convert to an entry array for the backup file
        /// </summary>
        internal byte[] ToByteArray()
        {
            var retVal = Enumerable.Range(0, 272).Select(o => (byte)' ').ToArray();
            Array.Copy(this.AssetClassId.ToByteArray(), 0, retVal, 0, 16);
            var nameBytes = Encoding.UTF8.GetBytes(this.AssetName).Take(256).ToArray();
            Array.Copy(nameBytes, 0, retVal, 16, nameBytes.Length);
            return retVal;
        }
    }
}
