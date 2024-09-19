﻿/*
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
using System;
using System.IO;

namespace SanteDB.Core.Data.Backup
{
    /// <summary>
    /// Represents a single asset (file, database, etc.) which can be backed up
    /// </summary>
    public interface IBackupAsset : IBackupAssetDescriptor, IDisposable
    {

        /// <summary>
        /// Open the backup stream 
        /// </summary>
        /// <remarks>Implementers of this interface should ensure that on an Open() the file 
        /// or backing source is frozen - so that changes from other threads cannot be written</remarks>
        /// <returns>A stream containing the data to be backed up</returns>
        Stream Open();

    }

}
