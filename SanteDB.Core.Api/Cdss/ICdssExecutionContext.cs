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
 * Date: 2024-6-21
 */
using SanteDB.Core.Model;
using System;

namespace SanteDB.Core.Cdss
{
    /// <summary>
    /// Represents a CDSS execution context to be shared between protocols
    /// </summary>
    public interface ICdssExecutionContext
    {

        /// <summary>
        /// Gets a variable value by name
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns></returns>
        object GetValue(String name);

        /// <summary>
        /// Get the target of the CDSS context
        /// </summary>
        IdentifiedData Target { get; }

        /// <summary>
        /// Gets the type of target which the context is set for
        /// </summary>
        Type TargetType { get; }
    }
}