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
using SanteDB.Core.Model.Interfaces;
using System.IO;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a provider of barcode formats (QR, GS1, etc) to render data
    /// </summary>
    public interface IBarcodeGenerator
    {

        /// <summary>
        /// Gets the barcode algorithm
        /// </summary>
        string BarcodeAlgorithm { get; }

        /// <summary>
        /// Generate a barcode from the specified identifier
        /// </summary>
        Stream Generate(IHasIdentifiers entity);

        /// <summary>
        /// Generate the barcode from raw data
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        Stream Generate(byte[] rawData);
    }
}