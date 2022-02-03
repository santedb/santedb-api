/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-27
 */
using System;

namespace SanteDB.Core.Interop.Description
{
    /// <summary>
    /// Represents a property descriptor
    /// </summary>
    public class ResourcePropertyDescription
    {


        /// <summary>
        /// Resource property description
        /// </summary>
        public ResourcePropertyDescription(String name, Type type)
        {
            this.Name = name;
            this.Type = type;
        }

        /// <summary>
        /// Creates a new resoure 
        /// </summary>
        public ResourcePropertyDescription(String name, ResourceDescription resourceType)
        {
            this.Name = name;
            this.Type = typeof(ResourceDescription);
            this.ResourceType = resourceType;
        }

        /// <summary>
        /// Gets the name of the object
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// Gets the type of resource
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets a resource description
        /// </summary>
        public ResourceDescription ResourceType { get; }

    }
}