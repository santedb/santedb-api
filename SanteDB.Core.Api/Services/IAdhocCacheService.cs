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
using System;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Defines a service which can store commonly used objects in a transient cache
    /// </summary>
    /// <remarks>
    /// <para>The ad-hoc cache service differs from the data cache in that the ad-hoc cache can be used to store any data with any
    /// key and value within the caching technology implementation. The cache is commonly used to store repeat or commonly
    /// fetched data (for example policy decision outcomes, keys, reference term lookups, etc.).</para>
    /// <para>The cache can be used to save fetching and querying data to/from the persistence layer.</para>
    /// <para><strong>Note to Implementers:</strong> Your implementation of this interface should not be a persistent cache (if possible to enforce). The
    /// callers of this interface typically assume a short lifecycle of data within the cache, and transient, rapid access should be prioritized over
    /// durability.</para>
    /// </remarks>
    /// <example>
    /// <code language="cs" title="Implementing a Cache Service">
    /// <![CDATA[
    /// // A horrible implementation of the cache service that uses a simple dictionary
    /// public class DictionaryCache : IAdHocCacheService {
    ///
    ///     private ConcurrentDictionary<String, Object> m_cache = new ConcurrentDictionary<String, Object>();
    ///
    ///     // Add an object to cache
    ///     public void Add<T>(String key, T value, TimeSpan? timeout = null) {
    ///         this.m_cache.TryAdd(key, value); // note: we won't implement timeouts
    ///     }
    ///
    ///     public T Get<T>(String key) {
    ///         if(this.m_cache.TryGetValue(key, out T retVal))
    ///         {
    ///             return retVal;
    ///         }
    ///         else
    ///         {
    ///             return default(T);
    ///         }
    ///     }
    ///
    ///     public bool Remove(String key) {
    ///         this.m_cache.TryRemove(key, out _);
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// <code language="cs" title="Using the Ad-Hoc Cache">
    /// <![CDATA[
    ///     bool IsAUser(String userName) {
    ///         var cacheService = ApplicationServiceContext.Current.GetService<IAdhocCacheService>();
    ///         // Attempt to load what we're looking for in the cache
    ///         var cachedResult = cacheService?.Get<bool?>($"isAUser.{userName}");
    ///         if(!cachedResult.HasValue) {
    ///             var persistenceService = ApplicationServiceContext.Current.GetService<IRepositoryService<SecurityUser>>();
    ///             cachedResult = persistenceService.Count(o=>o.UserName == userName, AuthenticationContext.SystemPrincipal) > 0;
    ///             cacheService?.Add($"isAUser.{userName}", cacheResult);
    ///         }
    ///         return cacheResult;
    /// ]]>
    /// </code>
    /// </example>
    [System.ComponentModel.Description("Ad-Hoc Cache Provider")]
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
        /// Try to fetch <paramref name="key"/> from the cache
        /// </summary>
        /// <param name="key">The key of the object to attempt to fetch</param>
        /// <param name="value">The fetched value</param>
        /// <returns>True if the object was fetched</returns>
        bool TryGet<T>(String key, out T value);

        /// <summary>
        /// Removes the specified object from the adhoc
        /// </summary>
        bool Remove(string key);

        /// <summary>
        /// Remove all keys matching <paramref name="patternKey"/>
        /// </summary>
        /// <param name="patternKey">The pattern to match</param>
        void RemoveAll(string patternKey);

        /// <summary>
        /// Returns true if <paramref name="key"/> exists in the cache
        /// </summary>
        bool Exists(String key);
    }
}