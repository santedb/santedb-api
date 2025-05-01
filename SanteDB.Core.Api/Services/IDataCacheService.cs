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
using SanteDB.Core.Model;
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Data caching event arguments
    /// </summary>
    /// <remarks>Whenever the <see cref="IDataCachingService"/> adds or removes
    /// data to/from its cache, it will raise a series of events to indicate to callers
    /// that some cache event occurs. This is useful if there are processes which the
    /// subscriber of these events wish to action (like logging, or broadcasting updates,
    /// or maintaining private data)</remarks>
    public class DataCacheEventArgs : EventArgs
    {
        /// <summary>
        /// The object added or removed from the cache
        /// </summary>
        public Object Object { get; private set; }

        /// <summary>
        /// Data cache event args ctor
        /// </summary>
        public DataCacheEventArgs(Object obj)
        {
            this.Object = obj;
        }
    }

    /// <summary>
    /// Defines a service which can be used by callers to store full <see cref="IdentifiedData"/> RIM objects
    /// in a transient cache.
    /// </summary>
    /// <remarks>
    /// <para>The data caching service is primarily used to store fully loaded objects from the database. This loading into the cache
    /// reduces the load on the persistence layer of the SanteDB host context. The data persistence layers themselves will use implementations
    /// of this class to prevent re-loading of data to/from the disk system. The process on a read is generally: </para>
    /// <list type="number">
    ///     <item>Check the cache service to determine if the data has already been loaded</item>
    ///     <item>If not found in cache load the data from disk / database</item>
    ///     <item>Add the object to the cache</item>
    /// </list>
    /// <para>Of course, keeping the cache service in a consistent state is tantamount to the reliable functioning of SanteDB, this means that any update, delete or create on
    /// an object that already exists in cache results in its eviction from the cache via the <see cref="Remove(Guid)"/> method.</para>
    /// </remarks>
    /// <seealso cref="IAdhocCacheService"/>
    [System.ComponentModel.Description("Primary Data Caching Provider")]
    public interface IDataCachingService : IServiceImplementation
    {
        /// <summary>
        /// Fired after an object has successfully been committed to the cache
        /// </summary>
        event EventHandler<DataCacheEventArgs> Added;

        /// <summary>
        /// Fired after an object has successfully been updated within the cache
        /// </summary>
        event EventHandler<DataCacheEventArgs> Updated;

        /// <summary>
        /// Fired after an object has successfully been evicted from cache
        /// </summary>
        event EventHandler<DataCacheEventArgs> Removed;

        /// <summary>
        /// Gets the cache item specified by <paramref name="key"/> returning it as a casted instance of <typeparamref name="TData"/>. Returning the default of <typeparamref name="TData"/> if the
        /// object doesn't exist or if the object is the wrong type.
        /// </summary>
        /// <typeparam name="TData">The type of data which is expected to be found with <paramref name="key"/></typeparam>
        /// <param name="key">The key identifier of the object to fetch from cache</param>
        /// <returns>The retrieved item</returns>
        TData GetCacheItem<TData>(Guid key) where TData : IdentifiedData;

        /// <summary>
        /// Gets the cache item specified by <paramref name="key"/> regardless of the type of data
        /// </summary>
        /// <remarks>This method differs from <see cref="GetCacheItem{TData}(Guid)"/> in that it does not attempt to cast the data
        /// it locates. The data returned will be an instance of <see cref="IdentifiedData" /> </remarks>
        /// <param name="key">The key identifier of the object to fetch from cache</param>
        /// <returns>The retrieved cache item</returns>
        IdentifiedData GetCacheItem(Guid key);

        /// <summary>
        /// Adds <paramref name="data"/> to the cache
        /// </summary>
        /// <remarks>This method will use the key identifier of the <paramref name="data"/> object passed into it by the caller as the
        /// basis for storing the data in the cache. When retrieving via the <see cref="GetCacheItem(Guid)"/> method</remarks>
        /// <param name="data">The data which is to be added to the cache</param>
        void Add(IdentifiedData data);

        /// <summary>
        /// Removes/evicts an object with identifier <paramref name="key"/> from the cache
        /// </summary>
        /// <param name="key">The key of the object to be removed</param>
        void Remove(Guid key);

        /// <summary>
        /// Removes/evicts the provided object form cache if available
        /// </summary>
        /// <param name="data">The data which is to be removed from the cache</param>
        void Remove(IdentifiedData data);

        /// <summary>
        /// Purges the entire cache of all entries
        /// </summary>
        /// <remarks>The ability of a cache provider to implement this method may depend on the server environment in which the SanteDB instance is operating. For example,
        /// if running the REDIS cache provider, a call to this will require an administrative connection to the REDIS server, or else a <see cref="InvalidOperationException"/>
        /// will be thrown.</remarks>
        void Clear();

        /// <summary>
        /// Gets the current size of the cache in objects
        /// </summary>
        long Size { get; }

        /// <summary>
        /// Returns true if the specified cache item exists
        /// </summary>
        bool Exists<TData>(Guid id) where TData : IdentifiedData;
    }
}