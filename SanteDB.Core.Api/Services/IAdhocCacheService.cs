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


    }
}
