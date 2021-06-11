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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// A caching service which permits the storage of any data regardless of type
    /// </summary>
    public interface IAdhocCacheService : IServiceImplementation
    {

        /// <summary>
        /// Add the specified object to the cache
        /// </summary>
        /// <param name="key">The key to assign the cache</param>
        /// <param name="value">The value to store in the cache</param>
        /// <param name="timeout">The timeout for the object validity</param>
        void Add<T>(String key, T value, TimeSpan? timeout = null);

        /// <summary>
        /// Gets the specified object from the cache
        /// </summary>
        /// <param name="key">The key of the object to fetch</param>
        /// <returns>The fetched value</returns>
        T Get<T>(String key);

        /// <summary>
        /// Removes the specified object from the adhoc
        /// </summary>
        bool Remove(string key);

        /// <summary>
        /// True if the specified key exists
        /// </summary>
        bool Exists(string key);
    }
}
