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
using System;
using System.IO;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Manages streams of data for other services 
    /// </summary>
    public interface IDataStreamManager
    {

        /// <summary>
        /// Gets the stream data by identifier from the stream manager
        /// </summary>
        /// <param name="streamId">The id of the stream to get</param>
        /// <returns>The stream loaded from the backing store</returns>
        Stream Get(Guid streamId);

        /// <summary>
        /// Add a stream to the stream manager
        /// </summary>
        /// <param name="stream">The stream to be added</param>
        /// <returns>The stream identifier assigned to the stream</returns>
        Guid Add(Stream stream);

        /// <summary>
        /// Deletes a stream from the stream manager
        /// </summary>
        /// <param name="streamId">The identifier of the stream to remove</param>
        void Remove(Guid streamId);
    }
}
