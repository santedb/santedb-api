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
 * Date: 2023-3-10
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IBarcodeProviderService"/>
    /// </summary>
    public class DefaultBarcodeProviderService : IBarcodeProviderService
    {
        // Barcode generators
        private readonly IDictionary<String, IBarcodeGenerator> m_barcodeProviders;

        /// <inheritdoc/>
        public string ServiceName => "Barcode Provider Service";

        /// <summary>
        /// New instance of the barcode provider
        /// </summary>
        public DefaultBarcodeProviderService(IServiceManager serviceManager)
        {
            var barcodeProviders = serviceManager.CreateInjectedOfAll<IBarcodeGenerator>();
            this.m_barcodeProviders = barcodeProviders.ToDictionaryIgnoringDuplicates(o => o.BarcodeAlgorithm, o => o);
        }

        /// <inheritdoc/>
        public IBarcodeGenerator GetBarcodeGenerator(string barcodeAlgorithm)
        {
            this.m_barcodeProviders.TryGetValue(barcodeAlgorithm, out var retVal);
            return retVal;
        }
    }
}
