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

namespace SanteDB.Core.Interop.Description
{

    /// <summary>
    /// Parameter location
    /// </summary>
    public enum OperationParameterLocation
    {
        /// <summary>
        /// Parameter is in the body
        /// </summary>
        Body,
        /// <summary>
        /// Parameter is in the URL
        /// </summary>
        Path,
        /// <summary>
        /// Parameter is in the query
        /// </summary>
        Query
    }

    /// <summary>
    /// A single parameter which is expressed to the service
    /// </summary>
    public class OperationParameterDescription : ResourcePropertyDescription
    {
        /// <summary>
        /// Resource property description
        /// </summary>
        public OperationParameterDescription(String name, Type type, OperationParameterLocation location) : base(name, type)
        {
            this.Location = location;
        }

        /// <summary>
        /// Creates a new resoure 
        /// </summary>
        public OperationParameterDescription(String name, ResourceDescription resourceType, OperationParameterLocation location) : base(name, resourceType)
        {
            this.Location = location;
        }

        /// <summary>
        /// Gets the location of the parameter
        /// </summary>
        public OperationParameterLocation Location { get; }

    }
}