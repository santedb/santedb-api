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
 * Date: 2023-6-21
 */
using Newtonsoft.Json;
using SanteDB.Core.Model;
using System;

namespace SanteDB.Core.ViewModel.Json
{
    /// <summary>
    /// Represents a view model type formatter that is for writing/reading to JSON streams
    /// </summary>
    public interface IJsonViewModelTypeFormatter : IViewModelTypeFormatter
    {


        /// <summary>
        /// Serialize specified object <paramref name="o"/> onto writer <paramref name="w"/> in context <paramref name="context"/>
        /// </summary>
        /// <param name="w">The writer to write data to</param>
        /// <param name="o">The object to be graphed</param>
        /// <param name="context">The current serialization context</param>
        void Serialize(JsonWriter w, Object o, JsonSerializationContext context);

        /// <summary>
        /// Parses the specified object from the reader
        /// </summary>
        /// <param name="r">The reader type</param>
        /// <param name="context">The current parse context</param>
        /// <param name="asType">The type which is being constructed</param>
        /// <returns></returns>
        object Deserialize(JsonReader r, Type asType, JsonSerializationContext context);

        /// <summary>
        /// Convert from simple value
        /// </summary>
        object FromSimpleValue(object simpleValue);

        /// <summary>
        /// Gets the simple value
        /// </summary>
        object GetSimpleValue(object instance);
    }
}