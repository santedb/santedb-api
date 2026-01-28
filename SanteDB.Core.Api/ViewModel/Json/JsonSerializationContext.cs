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

namespace SanteDB.Core.ViewModel.Json
{
    /// <summary>
    /// Represents a JSON serialization context
    /// </summary>
    public class JsonSerializationContext : SerializationContext
    {
        /// <summary>
        /// Creates a new JSON Serialization context
        /// </summary>
        public JsonSerializationContext(String propertyName, JsonViewModelSerializer context, Object instance) : base(propertyName, context, instance)
        {
        }

        /// <summary>
        /// Creates a new JSON serialization context
        /// </summary>
        public JsonSerializationContext(String propertyName, JsonViewModelSerializer context, Object instance, JsonSerializationContext parent) : base(propertyName, context, instance, parent)
        {

        }

        /// <summary>
        /// Gets the Context as a JSON serializer
        /// </summary>
        public JsonViewModelSerializer JsonContext { get { return base.Context as JsonViewModelSerializer; } }

        /// <summary>
        /// Gets the JSON parent context
        /// </summary>
        public JsonSerializationContext JsonParent { get { return base.Parent as JsonSerializationContext; } }


    }
}
