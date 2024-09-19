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
 */
using SanteDB.Core.Model.DataTypes;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a barcode generator (QR, CODE39, etc.) which generates visual pointers to provided data
    /// </summary>
    /// <remarks>
    /// <para>The barcode generator provider is responsible for accepting one or more <see cref="EntityIdentifier"/> or 
    /// <see cref="ActIdentifier"/> instances from an object, and generating a secure barcode which points at the provided 
    /// resource. Additionally, the barcode provider provides SanteDB with the tooling to generate barcodes from raw data. 
    /// These barcodes should be returned as an appropriate stream (containing PNG, PDF, or other data) which can be served
    /// to the other SanteDB components such as BI reports, or the REST API</para>
    /// <para>This interface is the basis for the <see href="https://help.santesuite.org/developers/service-apis/health-data-service-interface-hdsi/digitally-signed-visual-code-api">Visual Resource Pointer</see> API</para>
    /// </remarks>
    [System.ComponentModel.Description("Barcode Generator Provider")]
    public interface IBarcodeProviderService : IServiceImplementation
    {

        /// <summary>
        /// Get the <see cref="IBarcodeGenerator"/> for <paramref name="barcodeAlgorithm"/>
        /// </summary>
        /// <param name="barcodeAlgorithm">The algorithm for which the generator is to be retrieved</param>
        /// <returns>The barocde generator</returns>
        IBarcodeGenerator GetBarcodeGenerator(String barcodeAlgorithm);
    }
}
