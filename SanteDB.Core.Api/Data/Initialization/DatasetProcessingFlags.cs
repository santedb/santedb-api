/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System.Xml.Serialization;

namespace SanteDB.Core.Data.Initialization
{
    /// <summary>
    /// Processing flags for datasets
    /// </summary>
    [XmlType(nameof(DatasetProcessingFlags), Namespace = "http://santedb.org/data"), Flags]
    public enum DatasetProcessingFlags
    {
        /// <summary>
        /// Overwrite existing relationshipos
        /// </summary>
        [XmlEnum("overwrite.relationships")]
        OverwriteRelationships = 0x1,
        /// <summary>
        /// Don't run in a transaction
        /// </summary>
        [XmlEnum("no.transaction")]
        NoTransaction = 0x2,
        /// <summary>
        /// Validate objects before persisting them (may slow down inserts)
        /// </summary>
        [XmlEnum("validate.objects")]
        ValidateDataBeforeInsert = 0x4
    }
}