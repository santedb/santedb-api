/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Implementations of this class are expected to be able to transform a single input object
    /// into a single output object 
    /// </summary>
    /// <remarks>
    /// Implementations of this class may perform terminology lookups, parsing, translation etc.
    /// </remarks>
    public interface IForeignDataElementTransform
    {

        /// <summary>
        /// Gets the name of the transform
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Transform data from the source foreign data object to an appropriate type
        /// </summary>
        /// <param name="input">The input object</param>
        /// <param name="args">The arguments to the transformer (context specific)</param>
        /// <param name="sourceRecord">The source reader</param>
        /// <returns>The transformed object</returns>
        object Transform(object input, IForeignDataRecord sourceRecord, params object[] args);
    }
}