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
using SanteDB.Core.Data.Import.Definition;
using SanteDB.Core.Model;
using System;
using System.IO;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Represents a foreign data mapper which can apply a <see cref="ForeignDataMap"/> against an input
    /// source of data.
    /// </summary>
    /// <remarks>These mappers are designed to parse incoming lines or data from a <see cref="System.IO.Stream"/> 
    /// and apply the mapping instructions on the <see cref="ForeignDataMap"/> supplied resulting in
    /// <see cref="IdentifiedData"/> instances representing the contents of the foreign data map</remarks>
    public interface IForeignDataFormat
    {

        /// <summary>
        /// Gets the file extension of this data format
        /// </summary>
        String FileExtension { get; }

        /// <summary>
        /// Open a <see cref="IForeignDataFile"/> from <paramref name="foreignDataStream"/>
        /// </summary>
        /// <returns>The created reader implementation</returns>
        /// <param name="foreignDataStream">The foreign data which should be used to open the reader</param>
        IForeignDataFile Open(Stream foreignDataStream);

    }
}
