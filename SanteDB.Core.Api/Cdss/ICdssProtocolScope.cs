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
 * Date: 2023-11-27
 */
using SanteDB.Core.Model.Attributes;
using System;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Represents an asset grouping
    /// </summary>
    public interface ICdssProtocolScope
    {
        /// <summary>
        /// Gets the ID of the asset group
        /// </summary>
        [QueryParameter("uuid")]
        Guid Uuid { get; }

        /// <summary>
        /// Gets the name of the asset group
        /// </summary>
        [QueryParameter("name")]
        String Name { get; }

        /// <summary>
        /// Gets the OID of the asset group
        /// </summary>
        [QueryParameter("oid")]
        String Oid { get; }

        /// <summary>
        /// Gets the ID of the asset group
        /// </summary>
        [QueryParameter("id")]
        String Id { get; }
    }
}
