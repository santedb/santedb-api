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
 * Date: 2024-1-29
 */
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represents a single description for  a single backup taken on this machine
    /// </summary>
    public interface IBackupDescriptor
    {

        /// <summary>
        /// Gets the label for the backup
        /// </summary>
        String Label { get; }

        /// <summary>
        /// Gets the timestamp 
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets the user that created the backup
        /// </summary>
        String CreatedBy { get; }

        /// <summary>
        /// Gets the size of the backup
        /// </summary>
        long Size { get; }

        /// <summary>
        /// True if the backup is encrypted
        /// </summary>
        bool IsEnrypted { get; }

        /// <summary>
        /// Gets the descriptors of the assets in this backup
        /// </summary>
        IEnumerable<IBackupAssetDescriptor> Assets { get; }
    }
}
