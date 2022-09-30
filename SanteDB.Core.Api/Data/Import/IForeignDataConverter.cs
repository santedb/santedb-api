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
 * Date: 2022-5-30
 */
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace SanteDB.Core.Data.Import
{
    /// <summary>
    /// Defines a reader which can read a format of data
    /// which is alien to SanteDB and convert it into
    /// the equivalent SanteDB records.
    /// </summary>
    public interface IForeignDataConverter
    {

        /// <summary>
        /// Gets the file extension of objects which this foreign data converter
        /// is expected to convert
        /// </summary>
        String Extension { get; }

        /// <summary>
        /// Gets a structure definition of the shape of the alien data
        /// in the specified <paramref name="inStream"/>
        /// </summary>
        /// <param name="inStream">The input stream containing a sample of alien data</param>
        /// <returns>A description of the foreign data source</returns>
        ForeignDataElementGroup GetDescriptor(Stream inStream);

        /// <summary>
        /// Converts the contents of the foreign data format into SanteDB
        /// objects
        /// </summary>
        /// <param name="inStream">The source stream containing the alien data</param>
        /// <param name="dataMap">The data map to be used to convert the foreign data into SanteDB objects</param>
        /// <returns>The converted SanteDB objects</returns>
        IEnumerable<IdentifiedData> Convert(Stream inStream, ForeignDataMap dataMap);

        /// <summary>
        /// Converts a collection of SanteDB objects to the foreign data format
        /// </summary>
        /// <param name="inData">The SanteDB objects to be converted</param>
        /// <param name="dataMap">The data mapping to convert to</param>
        /// <returns>The converted object</returns>
        Stream Convert(IEnumerable<IdentifiedData> inData, ForeignDataMap dataMap);
    }
}
