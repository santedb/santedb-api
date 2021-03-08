/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Model;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Represents a barcode generator
    /// </summary>
    public interface IBarcodeProviderService : IServiceImplementation
    {

        /// <summary>
        /// Generate a barcode from the specified identifier
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to which the identifier is attached</typeparam>
        Stream Generate<TEntity>(IEnumerable<IdentifierBase<TEntity>> identifers)
            where TEntity : VersionedEntityData<TEntity>, new();

        /// <summary>
        /// Generate the barcode from raw data
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        Stream Generate(String rawData);
    }
}
