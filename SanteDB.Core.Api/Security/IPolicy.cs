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
 * User: fyfej
 * Date: 2023-6-21
 */
using System;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Represents a simple policy
    /// </summary>
    public interface IPolicy
    {
        /// <summary>
        /// Gets the UUID for the policy
        /// </summary>
        Guid Key { get; }
        /// <summary>
        /// Gets whether the policy can be overridden
        /// </summary>
        bool CanOverride { get; }
        /// <summary>
        /// Gets whether the policy is active
        /// </summary>
        bool IsActive { get; }
        /// <summary>
        /// Gets the name of the policy
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the OID of the policy
        /// </summary>
        string Oid { get; }
    }
}

